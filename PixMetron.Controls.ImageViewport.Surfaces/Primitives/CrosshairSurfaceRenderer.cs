using System.Windows;
using System.Windows.Media;

using PixMetron.Controls.ImageViewport.Contracts.Surfaces;

namespace PixMetron.Controls.ImageViewport.Surfaces.Primitives
{
    public sealed class CrosshairSurfaceRenderer : ISurfaceRenderer
    {
        public Brush Brush { get; set; } = Brushes.Yellow;
        public double Thickness { get; set; } = 1.0;

        public SurfaceMode TransformMode => SurfaceMode.Independent;

        public void Render(DrawingContext dc, in SurfaceRenderContext ctx)
        {
            var center = new Point(ctx.WindowRect.Width / 2.0, ctx.WindowRect.Height / 2.0);
            var pen = new Pen(Brush, Thickness);
            dc.DrawLine(pen, new Point(0, center.Y), new Point(ctx.WindowRect.Width, center.Y));
            dc.DrawLine(pen, new Point(center.X, 0), new Point(center.X, ctx.WindowRect.Height));
        }
    }
}