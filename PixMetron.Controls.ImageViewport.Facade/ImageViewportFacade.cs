
using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Contracts.Input;
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;
using PixMetron.Controls.ImageViewport.Handlers.Composite;
using PixMetron.Controls.ImageViewport.Runtime.Services;
using PixMetron.Controls.ImageViewport.Runtime.Transforms;

namespace PixMetron.Controls.ImageViewport.Facade
{
    /// <summary>
    /// 通用、可扩展的 Facade:自由管理 Surface 列表与输入路由链。
    /// - 高内聚低耦合:控件仅依赖 IViewportFacade;
    /// - 可插拔:Part(Renderer + 可选 Router + ZIndex + Group + Visible + ReceivesInput)
    /// - Transforms:支持外部自定义工厂 + 懒创建 + 版本缓存
    /// </summary>
    public sealed class ImageViewportFacade : IEditableViewportFacade, IDisposable
    {
        #region Part 定义
        private sealed class Part
        {
            public string Key { get; }
            public string? Group { get; set; }
            public int ZIndex { get; set; }
            public bool Visible { get; set; } = true;
            public bool ReceivesInput { get; set; } = true;

            public ISurfaceRenderer Renderer { get; set; }
            public IInputRouter? Router { get; set; }

            public Part(string key, ISurfaceRenderer renderer, IInputRouter? router = null)
            {
                Key = key ?? throw new ArgumentNullException(nameof(key));
                Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
                Router = router;
            }
        }
        #endregion

        // ---------- 状态 ----------
        private readonly List<Part> _parts = new();
        private readonly List<ISurfaceRenderer> _visibleCache = new();
        private IInputRouter? _composite;
        private bool _dirtySurfaces = true;
        private bool _dirtyInput = true;
        private int _batchDepth = 0;

        // ---------- 输入优先级(可选接口) ----------
        private static int GetPriority(IInputRouter r) => (r is IInputPrioritizable p) ? p.Priority : 0;

        // ---------- Transforms 工厂与缓存 ----------
        private readonly Func<ViewportInfo, IViewportTransforms> _transformsFactory;
        private ulong _lastVersion = ulong.MaxValue;
        private IViewportTransforms? _cachedTransforms;

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

            // 初始化 current & 缓存
            UpdateCache(Service.Current);
        }

        #region IViewportFacade
        public IEnumerable<ISurfaceRenderer> Surfaces => GetVisibleSurfaces();
        public IViewportService Service { get; }
        public IInputRouter? InputRouter => GetCompositeRouter();
        public IContextMenuProvider? ContextMenu { get; private set; }

        public IViewportTransforms GetTransforms(in ViewportInfo info)
        {
            if (_cachedTransforms is null || _lastVersion != info.Version)
            {
                UpdateCache(info);
            }
            return _cachedTransforms!;
        }
        #endregion

        #region IEditableViewportFacade —— Surface 列表(面向第三方)
        // 外界看到的是"只包含可见项、有序"的可变集合视图。
        // 简化处理:返回一个投影集合(修改通过下方 API 完成),避免直接篡改内部 _parts。
        public IList<ISurfaceRenderer> SurfacesMutable => GetVisibleMutableView();

        private IList<ISurfaceRenderer> GetVisibleMutableView()
        {
            RebuildCachesIfDirty();
            // 提供一个可编辑适配器:对它的修改转发为下方 API 调用
            return new MutableSurfaceAdapter(this);
        }

        private sealed class MutableSurfaceAdapter : IList<ISurfaceRenderer>
        {
            private readonly ImageViewportFacade _f;
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

        public bool AddSurface(ISurfaceRenderer surface)
        {
            if (surface is null) return false;
            var z = _parts.Count == 0 ? 0 : _parts.Max(p => p.ZIndex) + 1;
            _parts.Add(new Part(Guid.NewGuid().ToString("N"), surface) { ZIndex = z });
            MarkDirty();
            return true;
        }

        public bool InsertSurface(int index, ISurfaceRenderer surface)
        {
            if (surface is null) return false;
            // 将可见视图的 index 映射回内部 ZIndex:简单策略=直接赋值 index,再整体稳定排序
            var ordered = _parts.OrderBy(p => p.ZIndex).ToList();
            var newPart = new Part(Guid.NewGuid().ToString("N"), surface) { ZIndex = index };
            ordered.Insert(Math.Clamp(index, 0, ordered.Count), newPart);
            // 重新编号,保证 ZIndex 连续
            for (int i = 0; i < ordered.Count; i++) ordered[i].ZIndex = i;
            _parts.Clear(); _parts.AddRange(ordered);
            MarkDirty();
            return true;
        }

        public bool RemoveSurface(ISurfaceRenderer surface)
        {
            var idx = _parts.FindIndex(p => ReferenceEquals(p.Renderer, surface));
            if (idx < 0) return false;
            _parts.RemoveAt(idx);
            MarkDirty();
            return true;
        }

        public bool SetSurfaceVisible(ISurfaceRenderer surface, bool visible)
        {
            var p = _parts.FirstOrDefault(x => ReferenceEquals(x.Renderer, surface));
            if (p is null) return false;
            if (p.Visible == visible) return true;
            p.Visible = visible;
            MarkDirty();
            return true;
        }

        public bool SetSurfaceGroup(ISurfaceRenderer surface, string? group)
        {
            var p = _parts.FirstOrDefault(x => ReferenceEquals(x.Renderer, surface));
            if (p is null) return false;
            if (p.Group == group) return true;
            p.Group = group;
            // 分组变更不影响渲染顺序或输入,无需 MarkDirty
            return true;
        }

        public bool BringToFront(ISurfaceRenderer surface)
        {
            var p = _parts.FirstOrDefault(x => ReferenceEquals(x.Renderer, surface));
            if (p is null) return false;
            p.ZIndex = (_parts.Count == 0 ? 0 : _parts.Max(x => x.ZIndex)) + 1;
            MarkDirty();
            return true;
        }

        public bool SendToBack(ISurfaceRenderer surface)
        {
            var p = _parts.FirstOrDefault(x => ReferenceEquals(x.Renderer, surface));
            if (p is null) return false;
            p.ZIndex = (_parts.Count == 0 ? 0 : _parts.Min(x => x.ZIndex)) - 1;
            MarkDirty();
            return true;
        }

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

        private void MoveReplace(int index, ISurfaceRenderer newRenderer)
        {
            // 将可见视图 index 映射回真实 Part,并替换 Renderer(通过移除旧的、插入新的方式)
            var ordered = _parts.Where(p => p.Visible).OrderBy(p => p.ZIndex).ToList();
            if (index < 0 || index >= ordered.Count) return;

            var target = ordered[index];
            // 找回原 Part
            var real = _parts.First(p => ReferenceEquals(p, target));
            var z = real.ZIndex;
            var group = real.Group;
            var visible = real.Visible;
            var receivesInput = real.ReceivesInput;
            var router = real.Router;

            // 在相同位置插入新 renderer 并删除旧的
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

        public int ClearGroup(string group)
        {
            int n = _parts.RemoveAll(p => p.Group == group);
            if (n > 0) MarkDirty();
            return n;
        }

        public IDisposable BatchUpdate() => new Batch(this);
        private sealed class Batch : IDisposable
        {
            private readonly ImageViewportFacade _f;
            private bool _disposed;
            public Batch(ImageViewportFacade f) { _f = f; _f._batchDepth++; }
            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                if (--_f._batchDepth == 0) _f.RebuildCachesIfDirty();
            }
        }
        #endregion

        #region IEditableViewportFacade —— 输入路由 & 菜单
        public void SetInputRouter(IInputRouter? router)
        {
            _composite = router;
            _dirtyInput = false; // 替换整体时,不需要重建
        }

        public void PrependInputRouter(IInputRouter router)
        {
            var current = GetCompositeRouter();
            _composite = (current is null)
                ? router
                : new CompositeInputRouter(new[] { router, current });
        }

        public void AppendInputRouter(IInputRouter router)
        {
            var current = GetCompositeRouter();
            _composite = (current is null)
                ? router
                : new CompositeInputRouter(new[] { current, router });
        }

        public void SetContextMenu(IContextMenuProvider? provider) => ContextMenu = provider;
        #endregion

        #region 内部:可见 Surface 列表 & 输入聚合
        private IEnumerable<ISurfaceRenderer> GetVisibleSurfaces()
        {
            RebuildCachesIfDirty();
            return _visibleCache;
        }

        private IInputRouter? GetCompositeRouter()
        {
            RebuildCachesIfDirty();
            return _composite;
        }

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
                    return (c != 0) ? c : b.z.CompareTo(a.z); // 优先级高在前;同优先级 Z 高在前
                });

                _composite = routers.Count switch
                {
                    0 => _composite, // 保持外部指定的 router(若有)
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

        private void MarkDirty()
        {
            _dirtySurfaces = true;
            _dirtyInput = true;
            if (_batchDepth == 0) RebuildCachesIfDirty();
        }
        #endregion

        #region 缓存
        private void UpdateCache(ViewportInfo info)
        {
            // 按需创建 + 版本缓存;工厂可由上层注入(非强制 BuiltIn)
            _cachedTransforms = _transformsFactory(info);
            _lastVersion = info.Version;
        }
        #endregion

        #region IDisposable
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