namespace PixMetron.Controls.ImageViewport.Contracts.Abstractions
{
    /// <summary>
    /// 提供视口的相关信息。
    /// </summary>
    public sealed class ViewportInfo
    {
        /// <summary>版本号：任何影响矩阵/映射的可见变化都会自增。</summary>
        public ulong Version { get; init; }

        /// <summary>
        /// 获取或设置窗口的像素尺寸。
        /// </summary>
        public PxSize WindowPixelSize { get; init; }

        /// <summary>
        /// 获取或设置图像视图在图像像素中的区域。
        /// </summary>
        public PxRect ViewportRectInImage { get; init; }

        /// <summary>
        /// 获取或设置缩放比例。
        /// </summary>
        public double Scale { get; init; }

        /// <summary>
        /// 获取或设置 X 方向的 DPI 缩放比例。
        /// </summary>
        public double DpiScaleX { get; init; } = 1.0;

        /// <summary>
        /// 获取或设置 Y 方向的 DPI 缩放比例。
        /// </summary>
        public double DpiScaleY { get; init; } = 1.0;
    }
}