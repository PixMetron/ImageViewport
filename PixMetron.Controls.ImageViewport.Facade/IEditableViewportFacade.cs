
// File: PixMetron.Controls.ImageViewport.Facade.Abstractions/IEditableViewportFacade.cs
using PixMetron.Controls.ImageViewport.Contracts.Input;
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;

namespace PixMetron.Controls.ImageViewport.Facade
{
    /// <summary>
    /// 面向“可编辑”的通用 Facade 细化接口；不改变既有 IViewportFacade 契约。
    /// </summary>
    public interface IEditableViewportFacade : Contracts.Facade.IViewportFacade
    {
        // ---- Surface 管理（顺序即 Z 序；越靠后越上层） ----
        /// <summary>可编辑的 Surface 列表（与 Facade 内部保持同步）。</summary>
        IList<ISurfaceRenderer> SurfacesMutable { get; }

        /// <summary>插入到顶层（末尾）。</summary>
        bool AddSurface(ISurfaceRenderer surface);

        /// <summary>在 index 插入（0 为最底层）。</summary>
        bool InsertSurface(int index, ISurfaceRenderer surface);

        /// <summary>按实例移除。</summary>
        bool RemoveSurface(ISurfaceRenderer surface);

        /// <summary>控制显隐（隐藏后不渲染也不接收输入）。</summary>
        bool SetSurfaceVisible(ISurfaceRenderer surface, bool visible);

        /// <summary>Z 序调整。</summary>
        bool BringToFront(ISurfaceRenderer surface);
        bool SendToBack(ISurfaceRenderer surface);
        bool MoveTo(ISurfaceRenderer surface, int index);

        /// <summary>为 Surface 设定/变更组名（可用于批量操作和分层场景切换）。</summary>
        bool SetSurfaceGroup(ISurfaceRenderer surface, string? group);

        // ---- 组操作（便捷）----
        int HideGroup(string group);
        int ShowGroup(string group);
        int ClearGroup(string group);

        // ---- 输入路由拼接 ----
        void SetInputRouter(IInputRouter? router);            // 覆盖
        void PrependInputRouter(IInputRouter router);         // 头部拼接
        void AppendInputRouter(IInputRouter router);          // 尾部拼接

        // ---- 右键菜单 ----
        void SetContextMenu(IContextMenuProvider? provider);
    }
}
