using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;
namespace PixMetron.Controls.ImageViewport.Surfaces.Primitives
{
    public enum RulerUnits { Pixels, Millimeters, Inches }
    public enum TickMode { Decimal, Binary }

    public sealed class RulerSurfaceRenderer : ISurfaceRenderer
    {
        public double ThicknessPx { get; set; } = 24;
        public Brush Background { get; set; } = new SolidColorBrush(Color.FromArgb(180, 24, 24, 24));
        public Brush Foreground { get; set; } = Brushes.White;
        public Pen TickPen { get; } = new Pen(Brushes.White, 1);

        public RulerUnits Units { get; set; } = RulerUnits.Pixels;
        public TickMode Mode { get; set; } = TickMode.Decimal;

        public SurfaceMode TransformMode => SurfaceMode.Independent;

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

            // 选择合适的主刻度间隔（屏幕上约每 ~80px 一个主刻度）
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

            // 顶部刻度（X）
            DrawAxis(dc, isHorizontal: true, ctx.WindowRect, ctx.View, ctx.Transforms, pxPerUnit, majorUnits, minorUnits);
            // 左侧刻度（Y）
            DrawAxis(dc, isHorizontal: false, ctx.WindowRect, ctx.View, ctx.Transforms, pxPerUnit, majorUnits, minorUnits);
        }

        private void DrawAxis(DrawingContext dc, bool isHorizontal, Rect win, ViewportInfo view, IViewportTransforms tf,
                              double pxPerUnit, double majorUnits, double minorUnits)
        {
            double length = isHorizontal ? win.Width : win.Height;
            double offset = ThicknessPx;

            // 对齐到单位
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

            string UnitSuffix() => Units switch { RulerUnits.Pixels => "px", RulerUnits.Millimeters => "mm", RulerUnits.Inches => "in", _ => "" };
        }
    }
}