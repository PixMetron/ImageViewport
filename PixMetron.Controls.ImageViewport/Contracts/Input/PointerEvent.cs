using System.Windows.Input;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;

namespace PixMetron.Controls.ImageViewport.Contracts.Input
{
    /// <summary>
    /// 表示指针（鼠标）事件的信息。
    /// </summary>
    public sealed class PointerEvent
    {
        /// <summary>
        /// 获取或设置事件发生的时间戳。
        /// </summary>
        public DateTime Timestamp { get; init; }

        /// <summary>
        /// 获取或设置指针在窗口中的像素坐标。
        /// </summary>
        public PxPoint WindowPx { get; init; }

        /// <summary>
        /// 获取或设置指针在图像中的像素坐标。
        /// </summary>
        public PxPoint ImagePx { get; init; }

        /// <summary>
        /// 获取或设置鼠标滚轮的增量。
        /// </summary>
        public double WheelDelta { get; init; }

        /// <summary>
        /// 获取或设置点击次数。
        /// </summary>
        public int ClickCount { get; init; }

        /// <summary>
        /// 获取或设置修饰键状态。
        /// </summary>
        public ModifierKeys Modifiers { get; init; }

        /// <summary>
        /// 获取或设置当前左键是否处于按下状态。
        /// 该值直接反映此刻物理鼠标左键的真实状态（通常由 MouseEventArgs.LeftButton == Pressed 得到），
        /// 用于实时判断物理鼠标状态。
        /// </summary>
        public bool CurrentLeftPressed { get; init; }

        /// <summary>
        /// 获取或设置当前右键是否处于按下状态。
        /// 该值直接反映此刻物理鼠标右键的真实状态（通常由 MouseEventArgs.RightButton == Pressed 得到），
        /// 用于实时判断物理鼠标状态。
        /// </summary>
        public bool CurrentRightPressed { get; init; }

        /// <summary>
        /// 获取或设置当前中键是否处于按下状态。
        /// 该值直接反映此刻物理鼠标中键的真实状态（通常由 MouseEventArgs.MiddleButton == Pressed 得到），
        /// 用于实时判断物理鼠标状态。
        /// </summary>
        public bool CurrentMiddlePressed { get; init; }

        /// <summary>
        /// 由路由器设置：要求控件不要弹出上下文菜单。
        /// 默认为 false；每次事件构造时都会重置。
        /// </summary>
        public bool SuppressContextMenu { get; set; }
    }
}