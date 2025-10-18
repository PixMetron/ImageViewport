using PixMetron.Controls.ImageViewport.Contracts.Abstractions;

namespace PixMetron.Controls.ImageViewport.Runtime.Services
{
    public sealed class BuiltInViewportService : IViewportService, IViewportObservable
    {
        PxSize _window = new(0, 0);
        PxRect _imageView = new(0, 0, 100, 100);
        double _scale = 1.0;
        double _dpiX = 1.0, _dpiY = 1.0;
        ulong _version;
        ViewportInfo? _current;

        public event EventHandler<ViewportInfo>? ViewportChanged;

        public ViewportInfo Current => _current ?? Snapshot();

        public ViewportInfo Snapshot() => new()
        {
            Version = _version,
            WindowPixelSize = _window,
            ViewportRectInImage = _imageView,
            Scale = _scale,
            DpiScaleX = _dpiX,
            DpiScaleY = _dpiY
        };

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

        public void FitImageRect(PxRect img)
        {
            // 确保窗口和图像尺寸有效，防止除零
            if (_window.Width <= 0 || _window.Height <= 0 || img.Width <= 0 || img.Height <= 0)
            {
                return;
            }

            // 1. 计算水平和垂直方向的缩放比例
            var scaleX = _window.Width / img.Width;
            var scaleY = _window.Height / img.Height;

            // 2. 取较小的比例以确保整个图像都能被容纳
            _scale = Math.Min(scaleX, scaleY);

            // 3. 基于新比例计算视口在图像坐标系下的新尺寸
            var newViewWidth = _window.Width / _scale;
            var newViewHeight = _window.Height / _scale;

            // 4. 计算左上角坐标，使图像居中
            var newX = img.X + (img.Width - newViewWidth) / 2.0;
            var newY = img.Y + (img.Height - newViewHeight) / 2.0;

            _imageView = new PxRect(newX, newY, newViewWidth, newViewHeight);
            Raise();
        }

        public void SetWindowSize(PxSize size)
        {
            // 边界检查
            if (size.Width <= 0 || size.Height <= 0)
            {
                _window = size;
                return;
            }

            // 如果是初始化状态,直接设置
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

            // 1. 计算窗口中心在图像坐标系中的位置(变化前)
            var centerImageX = _imageView.X + _imageView.Width / 2.0;
            var centerImageY = _imageView.Y + _imageView.Height / 2.0;

            // 2. 更新窗口尺寸
            _window = size;

            // 3. 计算新的视口尺寸(保持缩放比例不变)
            var newViewWidth = size.Width / _scale;
            var newViewHeight = size.Height / _scale;

            // 4. 调整视口位置,使中心点保持不变
            _imageView = new PxRect(
                centerImageX - newViewWidth / 2.0,
                centerImageY - newViewHeight / 2.0,
                newViewWidth,
                newViewHeight);

            Raise();
        }

        public void SetDpi(double x, double y)
        {
            _dpiX = x;
            _dpiY = y;
            Raise();
        }

        void RebuildCurrent()
        {
            _current = Snapshot();
        }

        private void Raise()
        {
            // 所有可见变化统一 ++Version
            _version++;

            // 先重建并缓存
            RebuildCurrent();

            ViewportChanged?.Invoke(this, _current!);
        }
    }
}