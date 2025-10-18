using PixMetron.Controls.ImageViewport.Contracts.Abstractions;

namespace PixMetron.Controls.ImageViewport.Contracts.Input
{
    /// <summary>
    /// 窗口尺寸变化处理器接口。
    /// </summary>
    public interface IWindowSizeHandler
    {
        bool OnWindowSizeChanged(object sender, PxSize newSize);
    }
}