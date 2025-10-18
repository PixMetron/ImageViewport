using System.Windows;
using System.Windows.Media;

using PixMetron.Controls.ImageViewport.Contracts.Surfaces;

namespace PixMetron.Controls.ImageViewport.Surfaces.Primitives
{
    public sealed class GridSurfaceRenderer : ISurfaceRenderer
    {
        public double BaseMinorStepPx { get; set; } = 16;
        public int MajorEvery { get; set; } = 4;
        public Brush MinorBrush { get; set; } = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255));
        public Brush MajorBrush { get; set; } = new SolidColorBrush(Color.FromArgb(110, 255, 255, 255));

        public SurfaceMode TransformMode => SurfaceMode.Independent;

        private readonly Pen _minorPen;
        private readonly Pen _majorPen;
        public GridSurfaceRenderer()
        {
            _minorPen = new Pen(MinorBrush, 1.0);
            _majorPen = new Pen(MajorBrush, 1.0);
            if (_minorPen.CanFreeze) _minorPen.Freeze();
            if (_majorPen.CanFreeze) _majorPen.Freeze();
        }

        public void Render(DrawingContext dc, in SurfaceRenderContext ctx)
        {
            var step = BaseMinorStepPx;
            if (step <= 1) step = 1;

            var left = ctx.WindowRect.Left;
            var top = ctx.WindowRect.Top;
            var right = ctx.WindowRect.Right;
            var bottom = ctx.WindowRect.Bottom;

            double startX = left - left % step;
            double startY = top - top % step;

            int i = 0;
            for (double x = startX; x <= right; x += step, i++)
            {
                var pen = i % MajorEvery == 0 ? _majorPen : _minorPen;
                dc.DrawLine(pen, new Point(x, top), new Point(x, bottom));
            }

            i = 0;
            for (double y = startY; y <= bottom; y += step, i++)
            {
                var pen = i % MajorEvery == 0 ? _majorPen : _minorPen;
                dc.DrawLine(pen, new Point(left, y), new Point(right, y));
            }
        }
    }
}