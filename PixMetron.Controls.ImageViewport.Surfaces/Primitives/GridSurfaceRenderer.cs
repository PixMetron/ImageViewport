using System.Windows;
using System.Windows.Media;

using PixMetron.Controls.ImageViewport.Contracts.Surfaces;

namespace PixMetron.Controls.ImageViewport.Surfaces.Primitives
{
    /// <summary>
    /// A grid surface renderer that draws a uniform grid pattern in window coordinate space.
    /// </summary>
    /// <remarks>
    /// This renderer operates in Independent mode, drawing directly in window coordinates without
    /// following the viewport transformation. The grid consists of minor lines at regular intervals
    /// and major lines at multiples of the minor step, providing visual reference for navigation.
    /// </remarks>
    public sealed class GridSurfaceRenderer : ISurfaceRenderer
    {
        /// <summary>
        /// Gets or sets the base step size in pixels between minor grid lines.
        /// </summary>
        /// <value>The spacing in pixels between minor grid lines. Default is 16.</value>
        public double BaseMinorStepPx { get; set; } = 16;

        /// <summary>
        /// Gets or sets the interval at which major grid lines appear.
        /// </summary>
        /// <value>The number of minor steps before a major line is drawn. Default is 4.</value>
        public int MajorEvery { get; set; } = 4;

        /// <summary>
        /// Gets or sets the brush used to draw minor grid lines.
        /// </summary>
        /// <value>A <see cref="Brush"/> for minor lines. Default is semi-transparent white (alpha 60).</value>
        public Brush MinorBrush { get; set; } = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255));

        /// <summary>
        /// Gets or sets the brush used to draw major grid lines.
        /// </summary>
        /// <value>A <see cref="Brush"/> for major lines. Default is semi-transparent white (alpha 110).</value>
        public Brush MajorBrush { get; set; } = new SolidColorBrush(Color.FromArgb(110, 255, 255, 255));

        /// <summary>
        /// Gets the transform mode for this renderer, which is independent of viewport transforms.
        /// </summary>
        /// <value>Always returns <see cref="SurfaceMode.Independent"/>.</value>
        public SurfaceMode TransformMode => SurfaceMode.Independent;

        private readonly Pen _minorPen;
        private readonly Pen _majorPen;

        /// <summary>
        /// Initializes a new instance of the <see cref="GridSurfaceRenderer"/> class.
        /// </summary>
        /// <remarks>
        /// Creates frozen pens for optimal rendering performance from the configured brushes.
        /// </remarks>
        public GridSurfaceRenderer()
        {
            _minorPen = new Pen(MinorBrush, 1.0);
            _majorPen = new Pen(MajorBrush, 1.0);
            if (_minorPen.CanFreeze) _minorPen.Freeze();
            if (_majorPen.CanFreeze) _majorPen.Freeze();
        }

        /// <summary>
        /// Renders the grid to the drawing context in window coordinate space.
        /// </summary>
        /// <param name="dc">The drawing context to render into.</param>
        /// <param name="ctx">The render context containing viewport information and transforms.</param>
        /// <remarks>
        /// <para>
        /// The grid is aligned to the window coordinate system:
        /// </para>
        /// <list type="number">
        /// <item>Calculates the starting position based on the step size to ensure grid alignment.</item>
        /// <item>Draws vertical lines from left to right at <see cref="BaseMinorStepPx"/> intervals.</item>
        /// <item>Draws horizontal lines from top to bottom at <see cref="BaseMinorStepPx"/> intervals.</item>
        /// <item>Every <see cref="MajorEvery"/>-th line is drawn using the major pen for emphasis.</item>
        /// </list>
        /// </remarks>
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