using System.Windows;
using System.Windows.Media;

using PixMetron.Controls.ImageViewport.Contracts.Facade;
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;

namespace PixMetron.Controls.ImageViewport
{
    /// <summary>
    /// A framework element that hosts and renders layered surfaces from a viewport facade.
    /// Manages transform state and render order for both follow-viewport and independent surfaces.
    /// </summary>
    public sealed class LayeredSurfaceHost : FrameworkElement
    {
        private IViewportFacade? _facade;

        /// <summary>
        /// Binds the host to a viewport facade and triggers a visual update.
        /// </summary>
        /// <param name="facade">The facade providing surfaces and viewport state.</param>
        public void Bind(IViewportFacade facade)
        {
            _facade = facade;
            InvalidateVisual();
        }

        /// <summary>
        /// Renders all surfaces from the bound facade. Surfaces with <see cref="SurfaceMode.Follow"/> 
        /// are rendered with the viewport transform applied, while <see cref="SurfaceMode.Independent"/> 
        /// surfaces render in window coordinates.
        /// </summary>
        /// <param name="dc">The drawing context to render into.</param>
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (_facade is null) return;

            var winRect = new Rect(RenderSize);

            // Get viewport state once per frame for consistency
            var view = _facade.Service.Current;
            var transforms = _facade.GetTransforms(in view);

            // Viewport matrix (image -> window) & inverse matrix
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

        /// <summary>
        /// Invalidates the visual to trigger a re-render on the next render pass.
        /// </summary>
        public void Invalidate() => InvalidateVisual();

        /// <summary>
        /// Handles render size changes by forcing a visual update.
        /// </summary>
        /// <param name="sizeInfo">Information about the size change.</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            InvalidateVisual();                 // Force redraw when host size changes
        }
    }
}