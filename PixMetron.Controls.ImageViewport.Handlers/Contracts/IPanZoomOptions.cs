using System;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;

namespace PixMetron.Controls.ImageViewport.Handlers.Contracts
{
    public interface IPanZoomOptions
    {
        // 拖拽平移使用哪一个鼠标键
        PanButton PanButton { get; }

        // 滚轮缩放是否需要 Ctrl
        bool RequireCtrlForWheelZoom { get; }

        // 是否使用图像坐标缩放
        bool UseImageCoordinateZoom { get; }

        // 滚轮缩放枢轴
        ZoomPivotMode WheelPivot { get; }

        // 缩放因子
        double ScaleFactor { get; }

        // 最小缩放
        double MinScale { get; }

        // 最大缩放
        double MaxScale { get; }

        // 当 WheelPivot = Custom 时提供自定义窗口像素点
        Func<PxPoint>? CustomPivotWindowPxProvider { get; }
    }
}