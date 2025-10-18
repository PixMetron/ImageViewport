using System.Windows;
using System.Windows.Media;

using PixMetron.Controls.ImageViewport.Contracts.Facade;
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;

namespace PixMetron.Controls.ImageViewport
{
    public sealed class LayeredSurfaceHost : FrameworkElement
    {
        private IViewportFacade? _facade;

        public void Bind(IViewportFacade facade)
        {
            _facade = facade;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (_facade is null) return;

            var winRect = new Rect(RenderSize);

            // 一帧只取一次，保持一致
            var view = _facade.Service.Current;
            var transforms = _facade.GetTransforms(in view);

            // 视口矩阵（image -> window） & 逆矩阵
            var s = view.Scale;
            var tl = view.ViewportRectInImage.TopLeft;
            var M = new Matrix(s, 0, 0, s, -tl.X * s, -tl.Y * s);
            var Minv = M; Minv.Invert();
            var ctx = new SurfaceRenderContext(winRect, view, transforms, M, Minv, view.DpiScaleX, view.DpiScaleY);

            var mt = new MatrixTransform(M);

            bool pushed = false;
            foreach (var layer in _facade.Surfaces)
            {
                if (layer.TransformMode == SurfaceMode.Follow)
                {
                    if (!pushed) { dc.PushTransform(mt); pushed = true; }
                    layer.Render(dc, in ctx);
                } else // Independent
                {
                    if (pushed) { dc.Pop(); pushed = false; }
                    layer.Render(dc, in ctx);
                }
            }
            if (pushed) dc.Pop();
        }

        public void Invalidate() => InvalidateVisual();

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            InvalidateVisual();                 // 主机自身尺寸变化强制重绘
        }
    }
}