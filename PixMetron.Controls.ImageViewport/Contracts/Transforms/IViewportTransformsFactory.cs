using PixMetron.Controls.ImageViewport.Contracts.Abstractions;

namespace PixMetron.Controls.ImageViewport.Contracts.Transforms
{
    /// <summary>
    /// 将不可变的 ViewportInfo 快照构造成与其 Version 一致的 IViewportTransforms。
    /// 由上层（调用方/项目）注入，避免 Facade 强绑具体实现。
    /// </summary>
    public interface IViewportTransformsFactory
    {
        IViewportTransforms Create(in ViewportInfo info);
    }
}