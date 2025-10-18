namespace PixMetron.Controls.ImageViewport.Contracts.Abstractions
{
    /// <summary>
    /// 提供视口（Viewport）相关操作的服务接口，支持快照、缩放、平移、适应图像区域、窗口尺寸和 DPI 设置等功能。
    /// </summary>
    public interface IViewportService
    {
        /// <summary>
        /// 获取当前视口的快照信息。每次调用均返回新的实例。
        /// </summary>
        /// <returns>包含当前视口状态的 <see cref="ViewportInfo"/> 实例。</returns>
        ViewportInfo Snapshot();

        /// <summary>
        /// 统一的不可变快照；任何可见变更都 ++Version 并生成新快照
        /// 同一 Version 期间返回同一实例
        /// </summary>
        ViewportInfo Current { get; }

        /// <summary>
        /// 以窗口像素为基准，在指定点进行缩放操作。
        /// </summary>
        /// <param name="scaleFactor">缩放因子。</param>
        /// <param name="windowPx">窗口像素坐标，作为缩放中心。</param>
        void ZoomAtWindowPx(double scaleFactor, PxPoint windowPx);

        /// <summary>
        /// 以窗口像素为单位进行平移操作。
        /// </summary>
        /// <param name="dx">水平方向平移的像素数。</param>
        /// <param name="dy">垂直方向平移的像素数。</param>
        void PanWindowPx(double dx, double dy);

        /// <summary>
        /// 使视口适应指定的图像像素区域。
        /// </summary>
        /// <param name="imagePxRect">需要适应的图像像素区域。</param>
        void FitImageRect(PxRect imagePxRect);

        /// <summary>
        /// 设置视口的窗口尺寸（以像素为单位）。
        /// </summary>
        /// <param name="size">窗口像素尺寸。</param>
        void SetWindowSize(PxSize size);

        /// <summary>
        /// 设置视口的 DPI 缩放比例。
        /// </summary>
        /// <param name="x">X 方向的 DPI 缩放比例。</param>
        /// <param name="y">Y 方向的 DPI 缩放比例。</param>
        void SetDpi(double x, double y);
    }
}