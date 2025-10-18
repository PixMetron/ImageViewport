using PixMetron.Controls.ImageViewport.Contracts.Abstractions;

namespace PixMetron.Controls.ImageViewport.Runtime.Transforms
{
    /// <summary>
    /// 内置视口坐标变换实现。
    /// 提供窗口像素坐标与图像像素坐标之间的双向转换功能。
    /// </summary>
    public sealed class BuiltInViewportTransforms : IViewportTransforms
    {
        private readonly ViewportInfo _viewportInfo;

        /// <summary>
        /// 初始化 <see cref="BuiltInViewportTransforms"/> 类的新实例。
        /// </summary>
        /// <param name="viewportInfo">视口信息。</param>
        public BuiltInViewportTransforms(ViewportInfo viewportInfo)
        {
            _viewportInfo = viewportInfo;
        }

        /// <summary>
        /// 将窗口像素坐标转换为图像像素坐标。
        /// </summary>
        /// <param name="windowPx">窗口像素坐标。</param>
        /// <returns>图像像素坐标。</returns>
        public PxPoint WindowToImage(PxPoint windowPx) => new(
            _viewportInfo.ViewportRectInImage.X + windowPx.X / _viewportInfo.Scale,
            _viewportInfo.ViewportRectInImage.Y + windowPx.Y / _viewportInfo.Scale);

        /// <summary>
        /// 将图像像素坐标转换为窗口像素坐标。
        /// </summary>
        /// <param name="imagePx">图像像素坐标。</param>
        /// <returns>窗口像素坐标。</returns>
        public PxPoint ImageToWindow(PxPoint imagePx) => new(
            (imagePx.X - _viewportInfo.ViewportRectInImage.X) * _viewportInfo.Scale,
            (imagePx.Y - _viewportInfo.ViewportRectInImage.Y) * _viewportInfo.Scale);

        /// <summary>
        /// 将窗口像素矩形转换为图像像素矩形。
        /// </summary>
        /// <param name="windowRect">窗口像素矩形。</param>
        /// <returns>图像像素矩形。</returns>
        public PxRect WindowToImage(PxRect windowRect) => new(
            _viewportInfo.ViewportRectInImage.X + windowRect.X / _viewportInfo.Scale,
            _viewportInfo.ViewportRectInImage.Y + windowRect.Y / _viewportInfo.Scale,
            windowRect.Width / _viewportInfo.Scale,
            windowRect.Height / _viewportInfo.Scale);

        /// <summary>
        /// 将图像像素矩形转换为窗口像素矩形。
        /// </summary>
        /// <param name="imageRect">图像像素矩形。</param>
        /// <returns>窗口像素矩形。</returns>
        public PxRect ImageToWindow(PxRect imageRect) => new(
            (imageRect.X - _viewportInfo.ViewportRectInImage.X) * _viewportInfo.Scale,
            (imageRect.Y - _viewportInfo.ViewportRectInImage.Y) * _viewportInfo.Scale,
            imageRect.Width * _viewportInfo.Scale,
            imageRect.Height * _viewportInfo.Scale);
    }
}