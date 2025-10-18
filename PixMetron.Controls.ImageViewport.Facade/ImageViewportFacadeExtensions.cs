
using PixMetron.Controls.ImageViewport.Contracts.Input;
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;

namespace PixMetron.Controls.ImageViewport.Facade
{
    /// <summary>
    /// 便捷扩展：只依赖 IEditableViewportFacade 的公开方法，不调用实现内私有成员。
    /// </summary>
    public static class ImageViewportFacadeExtensions
    {
        // ---------- 批量添加 ----------
        public static IEditableViewportFacade AddSurfaces(
            this IEditableViewportFacade facade,
            IEnumerable<ISurfaceRenderer> surfaces)
        {
            if (facade is null) throw new ArgumentNullException(nameof(facade));
            if (surfaces is null) return facade;

            using (BeginBatch(facade))
            {
                foreach (var s in surfaces) facade.AddSurface(s);
            }
            return facade;
        }

        // ---------- 分组显隐 ----------
        public static int ShowGroup(this IEditableViewportFacade facade, string group, IEnumerable<ISurfaceRenderer>? members = null)
            => SetGroupVisibleInternal(facade, group, true, members);

        public static int HideGroup(this IEditableViewportFacade facade, string group, IEnumerable<ISurfaceRenderer>? members = null)
            => SetGroupVisibleInternal(facade, group, false, members);

        /// <summary>内部统一实现：设置组可见性。优先利用接口自带的 ShowGroup/HideGroup；没有成员列表就按组名操作；提供 members 时逐个设置。</summary>
        private static int SetGroupVisibleInternal(IEditableViewportFacade facade, string group, bool visible, IEnumerable<ISurfaceRenderer>? members)
        {
            if (facade is null) throw new ArgumentNullException(nameof(facade));
            if (string.IsNullOrWhiteSpace(group)) return 0;

            int changed = 0;

            // 如果实现类支持组级别操作（我们接口里已有），直接用
            if (members is null)
            {
                changed = visible ? facade.ShowGroup(group) : facade.HideGroup(group);
                return changed;
            }

            // 否则对给定成员逐个设置可见性
            using (BeginBatch(facade))
            {
                foreach (var s in members)
                {
                    // 无法判断 s 属于哪个组（接口未暴露），这里仅做显隐控制
                    if (facade.SetSurfaceVisible(s, visible)) changed++;
                }
            }
            return changed;
        }

        // ---------- Z 序操作 ----------
        public static IEditableViewportFacade BringToFront(this IEditableViewportFacade facade, ISurfaceRenderer surface)
        {
            if (facade.BringToFront(surface)) return facade;
            return facade;
        }

        public static IEditableViewportFacade SendToBack(this IEditableViewportFacade facade, ISurfaceRenderer surface)
        {
            if (facade.SendToBack(surface)) return facade;
            return facade;
        }

        public static IEditableViewportFacade MoveTo(this IEditableViewportFacade facade, ISurfaceRenderer surface, int index)
        {
            if (facade.MoveTo(surface, index)) return facade;
            return facade;
        }

        // ---------- 输入路由拼接 ----------
        public static IEditableViewportFacade UseRouter(this IEditableViewportFacade facade, IInputRouter router)
        {
            facade.SetInputRouter(router);
            return facade;
        }

        public static IEditableViewportFacade AppendRouter(this IEditableViewportFacade facade, IInputRouter router)
        {
            facade.AppendInputRouter(router);
            return facade;
        }

        public static IEditableViewportFacade PrependRouter(this IEditableViewportFacade facade, IInputRouter router)
        {
            facade.PrependInputRouter(router);
            return facade;
        }

        // ---------- 菜单 ----------
        public static IEditableViewportFacade WithContextMenu(this IEditableViewportFacade facade, IContextMenuProvider provider)
        {
            facade.SetContextMenu(provider);
            return facade;
        }

        // ---------- 帮助：批量更新 ----------
        private static IDisposable BeginBatch(IEditableViewportFacade facade)
        {
            // 对常见实现（如本文的 ImageViewportFacade）可反射尝试拿 BatchUpdate；
            // 如果实现没有 BatchUpdate，可返回空对象。
            var mi = facade.GetType().GetMethod("BatchUpdate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (mi != null && mi.GetParameters().Length == 0 && mi.ReturnType == typeof(IDisposable))
            {
                return (IDisposable)mi.Invoke(facade, null)!;
            }
            return new NullScope();
        }

        private sealed class NullScope : IDisposable { public void Dispose() { } }
    }
}
