using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;

namespace PixMetron.Controls.ImageViewport.Surfaces.Primitives
{
    /// <summary>
    /// Specifies the unit of measurement for the ruler.
    /// </summary>
    public enum RulerUnits
    {
        /// <summary>Pixels.</summary>
        Pixels,
        /// <summary>Millimeters.</summary>
        Millimeters,
        /// <summary>Inches.</summary>
        Inches
    }

    /// <summary>
    /// Specifies the tick mark interval mode for the ruler.
    /// </summary>
    public enum TickMode
    {
        /// <summary>Decimal intervals (1, 2, 5, 10, 20, 50, 100, etc.).</summary>
        Decimal,
        /// <summary>Binary intervals (1, 2, 4, 8, 16, 32, 64, etc.).</summary>
        Binary
    }

    /// <summary>
    /// Renders ruler guides along the top and left edges of the viewport.
    /// </summary>
    public sealed class RulerSurfaceRenderer : ISurfaceRenderer
    {
        /// <summary>
        /// Gets or sets the thickness of the ruler in pixels.
        /// </summary>
        public double ThicknessPx { get; set; } = 24;

        /// <summary>
        /// Gets or sets the background brush for the ruler.
        /// </summary>
        public Brush Background { get; set; } = new SolidColorBrush(Color.FromArgb(180, 24, 24, 24));

        /// <summary>
        /// Gets or sets the foreground brush for text and tick marks.
        /// </summary>
        public Brush Foreground { get; set; } = Brushes.White;

        /// <summary>
        /// Gets the pen used to draw tick marks.
        /// </summary>
        public Pen TickPen { get; } = new Pen(Brushes.White, 1);

        /// <summary>
        /// Gets or sets the unit of measurement displayed on the ruler.
        /// </summary>
        public RulerUnits Units { get; set; } = RulerUnits.Pixels;

        /// <summary>
        /// Gets or sets the tick mark interval mode.
        /// </summary>
        public TickMode Mode { get; set; } = TickMode.Decimal;

        /// <summary>
        /// Gets the surface transform mode. Rulers use independent mode to render in window coordinates.
        /// </summary>
        public SurfaceMode TransformMode => SurfaceMode.Independent;

        /// <summary>
        /// Renders the ruler on the drawing context.
        /// </summary>
        /// <param name="dc">The drawing context.</param>
        /// <param name="ctx">The surface render context containing viewport information.</param>
        public void Render(DrawingContext dc, in SurfaceRenderContext ctx)
        {
            var topRect = new Rect(0, 0, ctx.WindowRect.Width, ThicknessPx);
            var leftRect = new Rect(0, 0, ThicknessPx, ctx.WindowRect.Height);
            dc.DrawRectangle(Background, null, topRect);
            dc.DrawRectangle(Background, null, leftRect);

            double pxPerUnit = Units switch
            {
                RulerUnits.Pixels => 1.0,
                RulerUnits.Millimeters => ctx.View.DpiScaleX * 96.0 / 25.4, // 96 dpi * DpiScaleX per inch, 25.4 mm/in
                RulerUnits.Inches => ctx.View.DpiScaleX * 96.0,
                _ => 1.0
            };

            // Select appropriate tick interval: aim for approximately 80px per major tick on screen
            double targetPx = 80.0;
            double[] steps = Mode == TickMode.Binary
                ? [1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024]
                : [1, 2, 5, 10, 20, 50, 100, 200, 500, 1000];
            double majorUnits = steps[0];
            foreach (var s in steps)
            {
                majorUnits = s;
                if (s * pxPerUnit * ctx.View.Scale >= targetPx) break;
            }

            double minorUnits = Mode == TickMode.Binary ? majorUnits / 4 : majorUnits / 10;
            if (minorUnits <= 0) minorUnits = majorUnits / 2;

            // Draw horizontal ruler (X-axis)
            DrawAxis(dc, isHorizontal: true, ctx.WindowRect, ctx.View, ctx.Transforms, pxPerUnit, majorUnits, minorUnits);
            // Draw vertical ruler (Y-axis)
            DrawAxis(dc, isHorizontal: false, ctx.WindowRect, ctx.View, ctx.Transforms, pxPerUnit, majorUnits, minorUnits);
        }

        /// <summary>
        /// Draws a single axis (horizontal or vertical) of the ruler.
        /// </summary>
        /// <param name="dc">The drawing context.</param>
        /// <param name="isHorizontal">True for horizontal axis, false for vertical.</param>
        /// <param name="win">The window rectangle.</param>
        /// <param name="view">The viewport information.</param>
        /// <param name="tf">The viewport transforms.</param>
        /// <param name="pxPerUnit">Pixels per unit of measurement.</param>
        /// <param name="majorUnits">Interval for major tick marks.</param>
        /// <param name="minorUnits">Interval for minor tick marks.</param>
        private void DrawAxis(DrawingContext dc, bool isHorizontal, Rect win, ViewportInfo view, IViewportTransforms tf,
                              double pxPerUnit, double majorUnits, double minorUnits)
        {
            double length = isHorizontal ? win.Width : win.Height;
            double offset = ThicknessPx;

            // Map to units
            PxPoint startPt = isHorizontal ? new PxPoint(offset, 0) : new PxPoint(0, offset);
            var startImg = tf.WindowToImage(startPt);
            double startUnits = isHorizontal ? startImg.X : startImg.Y;
            double startMajor = Math.Floor(startUnits / majorUnits) * majorUnits;

            for (double u = startMajor; ; u += minorUnits)
            {
                var img = isHorizontal ? new PxPoint(u, 0) : new PxPoint(0, u);
                var pt = tf.ImageToWindow(img);
                double pos = isHorizontal ? pt.X : pt.Y;
                if (pos > length) break;
                if (pos < offset) continue;

                bool isMajor = Math.Abs(u / majorUnits - Math.Round(u / majorUnits)) < 1e-6;
                double len = isMajor ? ThicknessPx * 0.7 : ThicknessPx * 0.4;
                if (isHorizontal)
                    dc.DrawLine(TickPen, new Point(pos, ThicknessPx), new Point(pos, ThicknessPx - len));
                else
                    dc.DrawLine(TickPen, new Point(ThicknessPx, pos), new Point(ThicknessPx - len, pos));

                if (isMajor)
                {
                    var txt = u.ToString("0.##", CultureInfo.InvariantCulture) + UnitSuffix();
                    var ft = new FormattedText(txt, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                                               new Typeface("Consolas"), 10, Foreground, 1.0);
                    if (isHorizontal) dc.DrawText(ft, new Point(pos + 2, 2));
                    else dc.DrawText(ft, new Point(2, pos - 8));
                }
            }

            string UnitSuffix() => Units switch
            {
                RulerUnits.Pixels => "px",
                RulerUnits.Millimeters => "mm",
                RulerUnits.Inches => "in",
                _ => ""
            };
        }
    }
}