using System;
using System.Collections.Generic;
using System.Windows.Media;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Contracts.Abstractions.Events;
using PixMetron.Controls.ImageViewport.Contracts.Facade;
using PixMetron.Controls.ImageViewport.Contracts.Input;
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;
using PixMetron.Controls.ImageViewport.Handlers.Contracts;
using PixMetron.Controls.ImageViewport.Handlers.Routers;
using PixMetron.Controls.ImageViewport.Runtime.Services;
using PixMetron.Controls.ImageViewport.Runtime.Transforms;
using PixMetron.Controls.ImageViewport.Surfaces.Primitives;

namespace PixMetron.Controls.ImageViewport.Defaults
{
    /// <summary>
    /// Default implementation of <see cref="IViewportFacade"/> that provides basic image viewing functionality
    /// with built-in pan/zoom handlers and auto-fit capabilities.
    /// </summary>
    /// <remarks>
    /// This facade automatically manages an image surface renderer, pan/zoom input handling, and auto-fit behavior.
    /// It is designed to work with the attached properties defined in <see cref="Viewport"/> for simplified configuration.
    /// </remarks>
    public sealed class DefaultImageViewportFacade : IViewportFacade, IDisposable
    {
        private readonly ImageViewport _viewport;
        private readonly ImageSurfaceRenderer _imageLayer = new();
        private readonly AutoFitHandler _autoFit = new() { Mode = AutoFitMode.Always };

        /// <summary>
        /// Gets the viewport service that manages viewport state and transformations.
        /// </summary>
        public IViewportService Service { get; } = new BuiltInViewportService();

        // Transform cache
        private ulong _lastVersion = ulong.MaxValue;
        private IViewportTransforms? _cachedTransforms;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultImageViewportFacade"/> class.
        /// </summary>
        /// <param name="viewport">The <see cref="ImageViewport"/> control to manage.</param>
        /// <param name="options">Pan and zoom configuration options.</param>
        /// <param name="source">Optional initial image source to display.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="viewport"/> is null.</exception>
        public DefaultImageViewportFacade(ImageViewport viewport, IPanZoomOptions options, ImageSource? source)
        {
            _viewport = viewport ?? throw new ArgumentNullException(nameof(viewport));
            _imageLayer.Source = source;

            Surfaces = [_imageLayer];
            InputRouter = new PanZoomHandler(options);
            ContextMenu = null;

            // Initialize current & cache
            UpdateCache(_viewport.CurrentOrEmpty());
            if (Service is IViewportObservable obs)
                obs.ViewportChanged += OnViewportChanged;

            _viewport.WindowSizeChanged += OnWindowSizeChanged;
        }

        /// <summary>
        /// Gets the collection of surface renderers managed by this facade.
        /// Currently contains only the image layer.
        /// </summary>
        public IEnumerable<ISurfaceRenderer> Surfaces { get; }

        /// <summary>
        /// Gets the input router that handles pan and zoom interactions.
        /// </summary>
        public IInputRouter? InputRouter { get; }

        /// <summary>
        /// Gets the context menu provider. Currently returns null as no default context menu is provided.
        /// </summary>
        public IContextMenuProvider? ContextMenu { get; }

        /// <summary>
        /// Gets the current viewport information.
        /// </summary>
        public ViewportInfo Current => Service.Current;

        /// <summary>
        /// Gets the coordinate transforms for the specified viewport information snapshot.
        /// Results are cached based on the viewport version to avoid unnecessary recalculations.
        /// </summary>
        /// <param name="info">The viewport information snapshot.</param>
        /// <returns>A transforms instance for coordinate conversions.</returns>
        public IViewportTransforms GetTransforms(in ViewportInfo info)
        {
            if (_cachedTransforms is null || _lastVersion != info.Version)
            {
                _cachedTransforms = new BuiltInViewportTransforms(info);
                _lastVersion = info.Version;
            }
            return _cachedTransforms;
        }

        /// <summary>
        /// Sets the image source to display and optionally triggers auto-fit based on the current auto-fit mode.
        /// </summary>
        /// <param name="src">The image source to display, or null to clear the current image.</param>
        /// <remarks>
        /// If the source is a <see cref="System.Windows.Media.Imaging.BitmapSource"/>, the image dimensions
        /// are automatically detected and used for auto-fit calculations.
        /// </remarks>
        public void SetSource(ImageSource? src)
        {
            _imageLayer.Source = src;
            if (src is System.Windows.Media.Imaging.BitmapSource bs)
            {
                var rect = new PxRect(0, 0, bs.PixelWidth, bs.PixelHeight);
                _autoFit.SetCachedRect(rect);
                _viewport.FitImageRect(rect);
            }
        }

        /// <summary>
        /// Sets the auto-fit mode that determines when the image should automatically fit to the viewport.
        /// </summary>
        /// <param name="mode">The auto-fit mode to apply.</param>
        public void SetAutoFitMode(AutoFitMode mode) => _autoFit.Mode = mode;

        /// <summary>
        /// Handles viewport change notifications and updates the transform cache.
        /// </summary>
        private void OnViewportChanged(object? sender, ViewportInfo e) => UpdateCache(e);

        /// <summary>
        /// Handles window size changes and triggers auto-fit if applicable based on the current mode.
        /// </summary>
        private void OnWindowSizeChanged(object? sender, WindowPixelSizeChangedEventArgs e)
        {
            if (_autoFit.ShouldRefitOnWindowSizeChange(e.NewSize, out var rect) && rect.HasValue)
            {
                _viewport.FitImageRect(rect.Value);
            }
        }

        /// <summary>
        /// Updates the cached transforms when the viewport version changes.
        /// </summary>
        /// <param name="info">The new viewport information.</param>
        private void UpdateCache(ViewportInfo info)
        {
            if (info.Version == _lastVersion && _cachedTransforms is not null) return;

            _cachedTransforms = new BuiltInViewportTransforms(info); // Rebuild
            _lastVersion = info.Version;
        }

        /// <summary>
        /// Disposes the facade and unsubscribes from events.
        /// </summary>
        public void Dispose()
        {
            if (Service is IViewportObservable obs)
            {
                obs.ViewportChanged -= OnViewportChanged;
            }

            _viewport.WindowSizeChanged -= OnWindowSizeChanged;
        }
    }
}