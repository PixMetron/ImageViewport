using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Contracts.Facade;
using PixMetron.Controls.ImageViewport.Contracts.Input;
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;
using PixMetron.Controls.ImageViewport.Handlers.Composite;
using PixMetron.Controls.ImageViewport.Runtime.Services;
using PixMetron.Controls.ImageViewport.Runtime.Transforms;

namespace PixMetron.Controls.ImageViewport.Facade
{
    /// <summary>
    /// A general-purpose, extensible facade for managing surface lists and input routing chains.
    /// Features:
    /// - High cohesion, low coupling: controls only depend on <see cref="IViewportFacade"/>.
    /// - Pluggable architecture: each part includes renderer, optional router, Z-index, group, visibility, and input reception flags.
    /// - Transform support: custom factory injection, lazy creation, and version-based caching.
    /// </summary>
    public sealed class ImageViewportFacade : IEditableViewportFacade, IDisposable
    {
        #region Part Definition
        /// <summary>
        /// Represents an internal part containing a surface renderer and associated metadata.
        /// </summary>
        private sealed class Part
        {
            /// <summary>
            /// Gets the unique identifier for this part.
            /// </summary>
            public string Key { get; }

            /// <summary>
            /// Gets or sets the group name for batch operations.
            /// </summary>
            public string? Group { get; set; }

            /// <summary>
            /// Gets or sets the Z-index for rendering order (higher values render on top).
            /// </summary>
            public int ZIndex { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this part is visible.
            /// </summary>
            public bool Visible { get; set; } = true;

            /// <summary>
            /// Gets or sets a value indicating whether this part receives input events.
            /// </summary>
            public bool ReceivesInput { get; set; } = true;

            /// <summary>
            /// Gets or sets the surface renderer for this part.
            /// </summary>
            public ISurfaceRenderer Renderer { get; set; }

            /// <summary>
            /// Gets or sets the optional input router for this part.
            /// </summary>
            public IInputRouter? Router { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Part"/> class.
            /// </summary>
            /// <param name="key">The unique identifier.</param>
            /// <param name="renderer">The surface renderer.</param>
            /// <param name="router">The optional input router.</param>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or <paramref name="renderer"/> is null.</exception>
            public Part(string key, ISurfaceRenderer renderer, IInputRouter? router = null)
            {
                Key = key ?? throw new ArgumentNullException(nameof(key));
                Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
                Router = router;
            }
        }
        #endregion

        // ---------- State ----------
        private readonly List<Part> _parts = new();
        private readonly List<ISurfaceRenderer> _visibleCache = new();
        private IInputRouter? _composite;
        private bool _dirtySurfaces = true;
        private bool _dirtyInput = true;
        private int _batchDepth = 0;

        // ---------- Input Priority (optional interface) ----------
        /// <summary>
        /// Gets the priority of an input router if it implements <see cref="IInputPrioritizable"/>.
        /// </summary>
        /// <param name="r">The input router.</param>
        /// <returns>The priority value, or 0 if not prioritizable.</returns>
        private static int GetPriority(IInputRouter r) => (r is IInputPrioritizable p) ? p.Priority : 0;

        // ---------- Transforms Factory & Cache ----------
        private readonly Func<ViewportInfo, IViewportTransforms> _transformsFactory;
        private ulong _lastVersion = ulong.MaxValue;
        private IViewportTransforms? _cachedTransforms;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageViewportFacade"/> class.
        /// </summary>
        /// <param name="service">The viewport service. If null, a built-in service is used.</param>
        /// <param name="transformsFactory">The factory for creating viewport transforms. If null, a built-in factory is used.</param>
        /// <param name="contextMenu">The context menu provider.</param>
        /// <param name="initialRouter">The initial input router.</param>
        public ImageViewportFacade(
            IViewportService? service = null,
            Func<ViewportInfo, IViewportTransforms>? transformsFactory = null,
            IContextMenuProvider? contextMenu = null,
            IInputRouter? initialRouter = null)
        {
            Service = service ?? new BuiltInViewportService();
            _transformsFactory = transformsFactory ?? (info => new BuiltInViewportTransforms(info));
            ContextMenu = contextMenu;
            _composite = initialRouter;

            // Initialize current & cache
            UpdateCache(Service.Current);
        }

        #region IViewportFacade
        /// <inheritdoc/>
        public IEnumerable<ISurfaceRenderer> Surfaces => GetVisibleSurfaces();

        /// <inheritdoc/>
        public IViewportService Service { get; }

        /// <inheritdoc/>
        public IInputRouter? InputRouter => GetCompositeRouter();

        /// <inheritdoc/>
        public IContextMenuProvider? ContextMenu { get; private set; }

        /// <inheritdoc/>
        public IViewportTransforms GetTransforms(in ViewportInfo info)
        {
            if (_cachedTransforms is null || _lastVersion != info.Version)
            {
                UpdateCache(info);
            }
            return _cachedTransforms!;
        }
        #endregion

        #region IEditableViewportFacade ¡ª Surface List (for third-party use)
        /// <inheritdoc/>
        public IList<ISurfaceRenderer> SurfacesMutable => GetVisibleMutableView();

        /// <summary>
        /// Gets a mutable view of visible surfaces.
        /// </summary>
        /// <returns>A mutable adapter for the visible surfaces collection.</returns>
        private IList<ISurfaceRenderer> GetVisibleMutableView()
        {
            RebuildCachesIfDirty();
            return new MutableSurfaceAdapter(this);
        }

        /// <summary>
        /// Adapter that provides a mutable view of the visible surfaces collection.
        /// </summary>
        private sealed class MutableSurfaceAdapter : IList<ISurfaceRenderer>
        {
            private readonly ImageViewportFacade _f;

            /// <summary>
            /// Initializes a new instance of the <see cref="MutableSurfaceAdapter"/> class.
            /// </summary>
            /// <param name="f">The parent facade.</param>
            public MutableSurfaceAdapter(ImageViewportFacade f) => _f = f;

            private List<ISurfaceRenderer> Snapshot => _f.GetVisibleSurfaces().ToList();

            public int Count => Snapshot.Count;
            public bool IsReadOnly => false;
            public ISurfaceRenderer this[int index] { get => Snapshot[index]; set => _f.MoveReplace(index, value); }

            public int IndexOf(ISurfaceRenderer item) => Snapshot.IndexOf(item);
            public void Insert(int index, ISurfaceRenderer item) => _f.InsertSurface(index, item);
            public void RemoveAt(int index) { var s = Snapshot[index]; _f.RemoveSurface(s); }
            public void Add(ISurfaceRenderer item) => _f.AddSurface(item);
            public void Clear() { foreach (var it in Snapshot.ToList()) _f.RemoveSurface(it); }
            public bool Contains(ISurfaceRenderer item) => Snapshot.Contains(item);
            public void CopyTo(ISurfaceRenderer[] array, int arrayIndex) => Snapshot.CopyTo(array, arrayIndex);
            public bool Remove(ISurfaceRenderer item) => _f.RemoveSurface(item);
            public IEnumerator<ISurfaceRenderer> GetEnumerator() => Snapshot.GetEnumerator();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// <inheritdoc/>
        public bool AddSurface(ISurfaceRenderer surface)
        {
            if (surface is null) return false;
            var z = _parts.Count == 0 ? 0 : _parts.Max(p => p.ZIndex) + 1;
            _parts.Add(new Part(Guid.NewGuid().ToString("N"), surface) { ZIndex = z });
            MarkDirty();
            return true;
        }

        /// <inheritdoc/>
        public bool InsertSurface(int index, ISurfaceRenderer surface)
        {
            if (surface is null) return false;
            var ordered = _parts.OrderBy(p => p.ZIndex).ToList();
            var newPart = new Part(Guid.NewGuid().ToString("N"), surface) { ZIndex = index };
            ordered.Insert(Math.Clamp(index, 0, ordered.Count), newPart);
            // Renumber ZIndex to ensure continuity
            for (int i = 0; i < ordered.Count; i++) ordered[i].ZIndex = i;
            _parts.Clear(); _parts.AddRange(ordered);
            MarkDirty();
            return true;
        }

        /// <inheritdoc/>
        public bool RemoveSurface(ISurfaceRenderer surface)
        {
            var idx = _parts.FindIndex(p => ReferenceEquals(p.Renderer, surface));
            if (idx < 0) return false;
            _parts.RemoveAt(idx);
            MarkDirty();
            return true;
        }

        /// <inheritdoc/>
        public bool SetSurfaceVisible(ISurfaceRenderer surface, bool visible)
        {
            var p = _parts.FirstOrDefault(x => ReferenceEquals(x.Renderer, surface));
            if (p is null) return false;
            if (p.Visible == visible) return true;
            p.Visible = visible;
            MarkDirty();
            return true;
        }

        /// <inheritdoc/>
        public bool SetSurfaceGroup(ISurfaceRenderer surface, string? group)
        {
            var p = _parts.FirstOrDefault(x => ReferenceEquals(x.Renderer, surface));
            if (p is null) return false;
            if (p.Group == group) return true;
            p.Group = group;
            return true;
        }

        /// <inheritdoc/>
        public bool BringToFront(ISurfaceRenderer surface)
        {
            var p = _parts.FirstOrDefault(x => ReferenceEquals(x.Renderer, surface));
            if (p is null) return false;
            p.ZIndex = (_parts.Count == 0 ? 0 : _parts.Max(x => x.ZIndex)) + 1;
            MarkDirty();
            return true;
        }

        /// <inheritdoc/>
        public bool SendToBack(ISurfaceRenderer surface)
        {
            var p = _parts.FirstOrDefault(x => ReferenceEquals(x.Renderer, surface));
            if (p is null) return false;
            p.ZIndex = (_parts.Count == 0 ? 0 : _parts.Min(x => x.ZIndex)) - 1;
            MarkDirty();
            return true;
        }

        /// <inheritdoc/>
        public bool MoveTo(ISurfaceRenderer surface, int index)
        {
            var p = _parts.FirstOrDefault(x => ReferenceEquals(x.Renderer, surface));
            if (p is null) return false;

            var ordered = _parts.OrderBy(x => x.ZIndex).ToList();
            ordered.Remove(p);
            ordered.Insert(Math.Clamp(index, 0, ordered.Count), p);
            for (int i = 0; i < ordered.Count; i++) ordered[i].ZIndex = i;

            _parts.Clear(); _parts.AddRange(ordered);
            MarkDirty();
            return true;
        }

        /// <summary>
        /// Replaces a renderer at the specified visible index.
        /// </summary>
        /// <param name="index">The visible index.</param>
        /// <param name="newRenderer">The new renderer.</param>
        private void MoveReplace(int index, ISurfaceRenderer newRenderer)
        {
            var ordered = _parts.Where(p => p.Visible).OrderBy(p => p.ZIndex).ToList();
            if (index < 0 || index >= ordered.Count) return;

            var target = ordered[index];
            var real = _parts.First(p => ReferenceEquals(p, target));
            var z = real.ZIndex;
            var group = real.Group;
            var visible = real.Visible;
            var receivesInput = real.ReceivesInput;
            var router = real.Router;

            _parts.Remove(real);
            _parts.Add(new Part(Guid.NewGuid().ToString("N"), newRenderer)
            {
                ZIndex = z,
                Group = group,
                Visible = visible,
                ReceivesInput = receivesInput,
                Router = router
            });
            MarkDirty();
        }

        /// <inheritdoc/>
        public int HideGroup(string group)
        {
            int n = 0;
            foreach (var p in _parts.Where(p => p.Group == group).ToList())
            {
                if (p.Visible) { p.Visible = false; n++; }
            }
            if (n > 0) MarkDirty();
            return n;
        }

        /// <inheritdoc/>
        public int ShowGroup(string group)
        {
            int n = 0;
            foreach (var p in _parts.Where(p => p.Group == group).ToList())
            {
                if (!p.Visible) { p.Visible = true; n++; }
            }
            if (n > 0) MarkDirty();
            return n;
        }

        /// <inheritdoc/>
        public int ClearGroup(string group)
        {
            int n = _parts.RemoveAll(p => p.Group == group);
            if (n > 0) MarkDirty();
            return n;
        }

        /// <summary>
        /// Begins a batch update operation that defers cache rebuilding until disposed.
        /// </summary>
        /// <returns>A disposable batch update token.</returns>
        public IDisposable BatchUpdate() => new Batch(this);

        /// <summary>
        /// Represents a batch update scope.
        /// </summary>
        private sealed class Batch : IDisposable
        {
            private readonly ImageViewportFacade _f;
            private bool _disposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="Batch"/> class.
            /// </summary>
            /// <param name="f">The parent facade.</param>
            public Batch(ImageViewportFacade f) { _f = f; _f._batchDepth++; }

            /// <inheritdoc/>
            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                if (--_f._batchDepth == 0) _f.RebuildCachesIfDirty();
            }
        }
        #endregion

        #region IEditableViewportFacade ¡ª Input Routing & Menu
        /// <inheritdoc/>
        public void SetInputRouter(IInputRouter? router)
        {
            _composite = router;
            _dirtyInput = false;
        }

        /// <inheritdoc/>
        public void PrependInputRouter(IInputRouter router)
        {
            var current = GetCompositeRouter();
            _composite = (current is null)
                ? router
                : new CompositeInputRouter(new[] { router, current });
        }

        /// <inheritdoc/>
        public void AppendInputRouter(IInputRouter router)
        {
            var current = GetCompositeRouter();
            _composite = (current is null)
                ? router
                : new CompositeInputRouter(new[] { current, router });
        }

        /// <inheritdoc/>
        public void SetContextMenu(IContextMenuProvider? provider) => ContextMenu = provider;
        #endregion

        #region Internal: Visible Surface List & Input Aggregation
        /// <summary>
        /// Gets the collection of visible surfaces in Z-order.
        /// </summary>
        /// <returns>An enumerable of visible surface renderers.</returns>
        private IEnumerable<ISurfaceRenderer> GetVisibleSurfaces()
        {
            RebuildCachesIfDirty();
            return _visibleCache;
        }

        /// <summary>
        /// Gets the composite input router.
        /// </summary>
        /// <returns>The composite input router, or null if none exists.</returns>
        private IInputRouter? GetCompositeRouter()
        {
            RebuildCachesIfDirty();
            return _composite;
        }

        /// <summary>
        /// Rebuilds internal caches if they are marked as dirty and not in a batch operation.
        /// </summary>
        private void RebuildCachesIfDirty()
        {
            if (_batchDepth > 0) return;

            if (_dirtySurfaces)
            {
                _visibleCache.Clear();
                foreach (var p in _parts.Where(p => p.Visible).OrderBy(p => p.ZIndex))
                    _visibleCache.Add(p.Renderer);
                _dirtySurfaces = false;
            }

            if (_dirtyInput)
            {
                var routers = _parts
                    .Where(p => p.Visible && p.ReceivesInput && p.Router != null)
                    .Select(p => (router: p.Router!, z: p.ZIndex, pri: GetPriority(p.Router!)))
                    .ToList();

                routers.Sort((a, b) =>
                {
                    int c = b.pri.CompareTo(a.pri);
                    return (c != 0) ? c : b.z.CompareTo(a.z); // Higher priority first; same priority ¡ú higher Z first
                });

                _composite = routers.Count switch
                {
                    0 => _composite, // Keep externally specified router (if any)
                    1 => (_composite != null)
                        ? new CompositeInputRouter(new[] { _composite, routers[0].router })
                        : routers[0].router,
                    _ => new CompositeInputRouter(
                        (_composite != null
                            ? new[] { _composite }.Concat(routers.Select(x => x.router))
                            : routers.Select(x => x.router)).ToArray())
                };
                _dirtyInput = false;
            }
        }

        /// <summary>
        /// Marks the caches as dirty, requiring a rebuild.
        /// </summary>
        private void MarkDirty()
        {
            _dirtySurfaces = true;
            _dirtyInput = true;
            if (_batchDepth == 0) RebuildCachesIfDirty();
        }
        #endregion

        #region Cache
        /// <summary>
        /// Updates the cached transforms for the specified viewport information.
        /// </summary>
        /// <param name="info">The viewport information.</param>
        private void UpdateCache(ViewportInfo info)
        {
            _cachedTransforms = _transformsFactory(info);
            _lastVersion = info.Version;
        }
        #endregion

        #region IDisposable
        /// <inheritdoc/>
        public void Dispose()
        {
            _parts.Clear();
            _visibleCache.Clear();
            _composite = null;
            _dirtyInput = _dirtySurfaces = false;
        }
        #endregion
    }
}