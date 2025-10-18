using System;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;

namespace PixMetron.Controls.ImageViewport.Handlers.Contracts
{
    public sealed class PanZoomOptions : IPanZoomOptions
    {
        /// <summary>用于拖拽平移的鼠标键（默认 Middle）。</summary>
        public PanButton PanButton { get; set; } = PanButton.Middle;

        /// <summary>滚轮缩放是否需要 Ctrl（默认 false）。</summary>
        public bool RequireCtrlForWheelZoom { get; set; } = false;

        public bool UseImageCoordinateZoom => true;

        /// <summary>滚轮缩放枢轴：Center / Mouse / Custom（默认 Mouse）。</summary>
        public ZoomPivotMode WheelPivot { get; set; } = ZoomPivotMode.Mouse;

        public double ScaleFactor { get; set; } = 1.1;

        public double MinScale { get; set; } = 0.01;

        public double MaxScale { get; set; } = 1000.0;

        /// <summary>当 WheelPivot = Custom 时使用的窗口像素点提供者。</summary>
        public Func<PxPoint>? CustomPivotWindowPxProvider { get; set; }
    }
}