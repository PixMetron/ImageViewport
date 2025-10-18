using System.Windows;
using System.Windows.Media;

using PixMetron.Controls.ImageViewport.Contracts.Surfaces;

namespace PixMetron.Controls.ImageViewport.Surfaces.Primitives
{
    /// <summary>
    /// A crosshair surface renderer that draws intersecting horizontal and vertical lines at the center of the window.
    /// </summary>
    /// <remarks>
    /// This renderer operates in Independent mode, drawing directly in window coordinates.
    /// The crosshair is always centered in the viewport and does not follow the image transformation,
    /// making it useful as a static reference marker or targeting reticle.
    /// </remarks>
    public sealed class CrosshairSurfaceRenderer : ISurfaceRenderer
    {
        /// <summary>
        /// Gets or sets the brush used to draw the crosshair lines.
        /// </summary>
        /// <value>A <see cref="Brush"/> for the crosshair. Default is <see cref="Brushes.Yellow"/>.</value>
        public Brush Brush { get; set; } = Brushes.Yellow;

        /// <summary>
        /// Gets or sets the thickness of the crosshair lines in pixels.
        /// </summary>
        /// <value>The line thickness in pixels. Default is 1.0.</value>
        public double Thickness { get; set; } = 1.0;

        /// <summary>
        /// Gets the transform mode for this renderer, which is independent of viewport transforms.
        /// </summary>
        /// <value>Always returns <see cref="SurfaceMode.Independent"/>.</value>
        public SurfaceMode TransformMode => SurfaceMode.Independent;

        /// <summary>
        /// Renders the crosshair to the drawing context in window coordinate space.
        /// </summary>
        /// <param name="dc">The drawing context to render into.</param>
        /// <param name="ctx">The render context containing viewport information and transforms.</param>
        /// <remarks>
        /// <para>
        /// Draws two perpendicular lines that intersect at the center of the window:
        /// </para>
        /// <list type="bullet">
        /// <item>A horizontal line spanning the full width of the window at the vertical center.</item>
        /// <item>A vertical line spanning the full height of the window at the horizontal center.</item>
        /// </list>
        /// </remarks>
        public void Render(DrawingContext dc, in SurfaceRenderContext ctx)
        {
            var center = new Point(ctx.WindowRect.Width / 2.0, ctx.WindowRect.Height / 2.0);
            var pen = new Pen(Brush, Thickness);
            dc.DrawLine(pen, new Point(0, center.Y), new Point(ctx.WindowRect.Width, center.Y));
            dc.DrawLine(pen, new Point(center.X, 0), new Point(center.X, ctx.WindowRect.Height));
        }
    }
}