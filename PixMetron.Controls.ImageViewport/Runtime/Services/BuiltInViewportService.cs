using PixMetron.Controls.ImageViewport.Contracts.Abstractions;

namespace PixMetron.Controls.ImageViewport.Runtime.Services
{
    /// <summary>
    /// Built-in implementation of the viewport service that manages viewport state, transformations, and notifications.
    /// </summary>
    public sealed class BuiltInViewportService : IViewportService, IViewportObservable
    {
        PxSize _window = new(0, 0);
        PxRect _imageView = new(0, 0, 100, 100);
        double _scale = 1.0;
        double _dpiX = 1.0, _dpiY = 1.0;
        ulong _version;
        ViewportInfo? _current;

        /// <summary>
        /// Occurs when the viewport state changes.
        /// </summary>
        public event EventHandler<ViewportInfo>? ViewportChanged;

        /// <summary>
        /// Gets the current viewport information. Returns a cached instance for the current version.
        /// </summary>
        public ViewportInfo Current => _current ?? Snapshot();

        /// <summary>
        /// Creates a snapshot of the current viewport state.
        /// </summary>
        /// <returns>A new <see cref="ViewportInfo"/> instance representing the current state.</returns>
        public ViewportInfo Snapshot() => new()
        {
            Version = _version,
            WindowPixelSize = _window,
            ViewportRectInImage = _imageView,
            Scale = _scale,
            DpiScaleX = _dpiX,
            DpiScaleY = _dpiY
        };

        /// <summary>
        /// Zooms the viewport at the specified window pixel position.
        /// </summary>
        /// <param name="factor">The zoom factor to apply.</param>
        /// <param name="win">The pivot point in window pixel coordinates.</param>
        public void ZoomAtWindowPx(double factor, PxPoint win)
        {
            if (factor <= 0) return;
            var old = _scale;
            _scale *= factor;
            var cx = _imageView.X + win.X / old;
            var cy = _imageView.Y + win.Y / old;
            _imageView = new PxRect(
                cx - _window.Width / _scale / 2.0,
                cy - _window.Height / _scale / 2.0,
                _window.Width / _scale,
                _window.Height / _scale);

            Raise();
        }

        /// <summary>
        /// Pans the viewport by the specified offset in window pixels.
        /// </summary>
        /// <param name="dx">The horizontal offset in window pixels.</param>
        /// <param name="dy">The vertical offset in window pixels.</param>
        public void PanWindowPx(double dx, double dy)
        {
            if (dx == 0 && dy == 0) return;
            _imageView = new PxRect(
                _imageView.X - dx / _scale,
                _imageView.Y - dy / _scale,
                _imageView.Width,
                _imageView.Height);
            Raise();
        }

        /// <summary>
        /// Fits the specified image rectangle to the viewport by adjusting scale and position.
        /// </summary>
        /// <param name="img">The image rectangle to fit.</param>
        public void FitImageRect(PxRect img)
        {
            // Ensure window and image dimensions are valid to prevent division by zero
            if (_window.Width <= 0 || _window.Height <= 0 || img.Width <= 0 || img.Height <= 0)
            {
                return;
            }

            // 1. Calculate scale factors for horizontal and vertical directions
            var scaleX = _window.Width / img.Width;
            var scaleY = _window.Height / img.Height;

            // 2. Use the smaller scale to ensure the entire image fits
            _scale = Math.Min(scaleX, scaleY);

            // 3. Calculate the new viewport dimensions in image coordinates based on the new scale
            var newViewWidth = _window.Width / _scale;
            var newViewHeight = _window.Height / _scale;

            // 4. Calculate top-left coordinates to center the image
            var newX = img.X + (img.Width - newViewWidth) / 2.0;
            var newY = img.Y + (img.Height - newViewHeight) / 2.0;

            _imageView = new PxRect(newX, newY, newViewWidth, newViewHeight);
            Raise();
        }

        /// <summary>
        /// Sets the window size and adjusts the viewport to maintain the center point.
        /// </summary>
        /// <param name="size">The new window size in pixels.</param>
        public void SetWindowSize(PxSize size)
        {
            // Boundary check
            if (size.Width <= 0 || size.Height <= 0)
            {
                _window = size;
                return;
            }

            // If in initialization state, set directly
            if (_window.Width <= 0 || _window.Height <= 0)
            {
                _window = size;
                _imageView = new PxRect(
                    _imageView.X,
                    _imageView.Y,
                    size.Width / _scale,
                    size.Height / _scale);
                Raise();
                return;
            }

            // 1. Calculate the center position in image coordinates (before change)
            var centerImageX = _imageView.X + _imageView.Width / 2.0;
            var centerImageY = _imageView.Y + _imageView.Height / 2.0;

            // 2. Update window size
            _window = size;

            // 3. Calculate new viewport dimensions (keeping scale unchanged)
            var newViewWidth = size.Width / _scale;
            var newViewHeight = size.Height / _scale;

            // 4. Adjust viewport position to maintain the center point
            _imageView = new PxRect(
                centerImageX - newViewWidth / 2.0,
                centerImageY - newViewHeight / 2.0,
                newViewWidth,
                newViewHeight);

            Raise();
        }

        /// <summary>
        /// Sets the DPI scale factors for the viewport.
        /// </summary>
        /// <param name="x">The horizontal DPI scale factor.</param>
        /// <param name="y">The vertical DPI scale factor.</param>
        public void SetDpi(double x, double y)
        {
            _dpiX = x;
            _dpiY = y;
            Raise();
        }

        /// <summary>
        /// Rebuilds and caches the current viewport information.
        /// </summary>
        void RebuildCurrent()
        {
            _current = Snapshot();
        }

        /// <summary>
        /// Raises the viewport changed event after incrementing the version and rebuilding the cache.
        /// </summary>
        private void Raise()
        {
            // All visible changes increment the version
            _version++;

            // Rebuild and cache first
            RebuildCurrent();

            ViewportChanged?.Invoke(this, _current!);
        }
    }
}