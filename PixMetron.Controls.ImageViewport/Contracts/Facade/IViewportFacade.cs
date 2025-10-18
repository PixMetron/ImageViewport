using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Contracts.Input;
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;

namespace PixMetron.Controls.ImageViewport.Contracts.Facade
{
    public interface IViewportFacade
    {
        /// <summary>必需：后端服务（控件通过它获取 Current、订阅变化等）。</summary>
        IViewportService Service { get; }

        /// <summary>必需：参与绘制的 Surface（底->顶）。</summary>
        IEnumerable<ISurfaceRenderer> Surfaces { get; }

        /// <summary>可选：输入路由器。</summary>
        IInputRouter? InputRouter { get; }

        /// <summary>可选：右键菜单。</summary>
        IContextMenuProvider? ContextMenu { get; }

        /// <summary>
        /// 根据调用者提供的不可变快照，返回与其 Version 一致的 IViewportTransforms。
        /// 每帧/每次渲染，控件会先拿到一个快照再调用它。
        /// </summary>
        IViewportTransforms GetTransforms(in ViewportInfo info);

        /// <summary>多域扩展位（可选）。</summary>
        IViewportTransforms? TryGetTransforms(string domain) => null;
    }
}