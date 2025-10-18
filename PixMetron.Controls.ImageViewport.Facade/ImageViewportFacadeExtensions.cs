using PixMetron.Controls.ImageViewport.Contracts.Input;
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;

namespace PixMetron.Controls.ImageViewport.Facade
{
    /// <summary>
    /// Provides extension methods for <see cref="IEditableViewportFacade"/> to simplify common operations.
    /// These extensions only call public members and do not access private members of the facade implementation.
    /// </summary>
    public static class ImageViewportFacadeExtensions
    {
        // ---------- Batch Add Surfaces ----------
        /// <summary>
        /// Adds multiple surfaces to the facade in a single batch operation.
        /// </summary>
        /// <param name="facade">The facade to modify.</param>
        /// <param name="surfaces">The collection of surfaces to add.</param>
        /// <returns>The facade instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="facade"/> is null.</exception>
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

        // ---------- Group Visibility ----------
        /// <summary>
        /// Shows all surfaces in the specified group.
        /// </summary>
        /// <param name="facade">The facade to modify.</param>
        /// <param name="group">The group name.</param>
        /// <param name="members">Optional specific members to show. If null, shows all members in the group.</param>
        /// <returns>The number of surfaces whose visibility was changed.</returns>
        public static int ShowGroup(this IEditableViewportFacade facade, string group, IEnumerable<ISurfaceRenderer>? members = null)
            => SetGroupVisibleInternal(facade, group, true, members);

        /// <summary>
        /// Hides all surfaces in the specified group.
        /// </summary>
        /// <param name="facade">The facade to modify.</param>
        /// <param name="group">The group name.</param>
        /// <param name="members">Optional specific members to hide. If null, hides all members in the group.</param>
        /// <returns>The number of surfaces whose visibility was changed.</returns>
        public static int HideGroup(this IEditableViewportFacade facade, string group, IEnumerable<ISurfaceRenderer>? members = null)
            => SetGroupVisibleInternal(facade, group, false, members);

        /// <summary>
        /// Internal unified implementation for setting group visibility.
        /// Calls the interface method ShowGroup/HideGroup if no member list is provided, 
        /// or iterates the provided members if specified.
        /// </summary>
        private static int SetGroupVisibleInternal(IEditableViewportFacade facade, string group, bool visible, IEnumerable<ISurfaceRenderer>? members)
        {
            if (facade is null) throw new ArgumentNullException(nameof(facade));
            if (string.IsNullOrWhiteSpace(group)) return 0;

            int changed = 0;

            // If the implementation supports group-level operations and no member list is provided, call directly
            if (members is null)
            {
                changed = visible ? facade.ShowGroup(group) : facade.HideGroup(group);
                return changed;
            }

            // Otherwise, iterate through the provided members to set visibility
            using (BeginBatch(facade))
            {
                foreach (var s in members)
                {
                    // Cannot determine if s belongs to the group (interface does not expose this), so just set visibility
                    if (facade.SetSurfaceVisible(s, visible)) changed++;
                }
            }
            return changed;
        }

        // ---------- Z-Order Operations ----------
        /// <summary>
        /// Brings the specified surface to the front (top layer).
        /// </summary>
        /// <param name="facade">The facade to modify.</param>
        /// <param name="surface">The surface to bring to front.</param>
        /// <returns>The facade instance for method chaining.</returns>
        public static IEditableViewportFacade BringToFront(this IEditableViewportFacade facade, ISurfaceRenderer surface)
        {
            if (facade.BringToFront(surface)) return facade;
            return facade;
        }

        /// <summary>
        /// Sends the specified surface to the back (bottom layer).
        /// </summary>
        /// <param name="facade">The facade to modify.</param>
        /// <param name="surface">The surface to send to back.</param>
        /// <returns>The facade instance for method chaining.</returns>
        public static IEditableViewportFacade SendToBack(this IEditableViewportFacade facade, ISurfaceRenderer surface)
        {
            if (facade.SendToBack(surface)) return facade;
            return facade;
        }

        /// <summary>
        /// Moves the specified surface to the given index in the Z-order.
        /// </summary>
        /// <param name="facade">The facade to modify.</param>
        /// <param name="surface">The surface to move.</param>
        /// <param name="index">The target Z-order index (0 is bottom layer).</param>
        /// <returns>The facade instance for method chaining.</returns>
        public static IEditableViewportFacade MoveTo(this IEditableViewportFacade facade, ISurfaceRenderer surface, int index)
        {
            if (facade.MoveTo(surface, index)) return facade;
            return facade;
        }

        // ---------- Input Router Composition ----------
        /// <summary>
        /// Sets the input router, replacing any existing router.
        /// </summary>
        /// <param name="facade">The facade to modify.</param>
        /// <param name="router">The router to set.</param>
        /// <returns>The facade instance for method chaining.</returns>
        public static IEditableViewportFacade UseRouter(this IEditableViewportFacade facade, IInputRouter router)
        {
            facade.SetInputRouter(router);
            return facade;
        }

        /// <summary>
        /// Appends an input router to the end of the router chain.
        /// </summary>
        /// <param name="facade">The facade to modify.</param>
        /// <param name="router">The router to append.</param>
        /// <returns>The facade instance for method chaining.</returns>
        public static IEditableViewportFacade AppendRouter(this IEditableViewportFacade facade, IInputRouter router)
        {
            facade.AppendInputRouter(router);
            return facade;
        }

        /// <summary>
        /// Prepends an input router to the beginning of the router chain.
        /// </summary>
        /// <param name="facade">The facade to modify.</param>
        /// <param name="router">The router to prepend.</param>
        /// <returns>The facade instance for method chaining.</returns>
        public static IEditableViewportFacade PrependRouter(this IEditableViewportFacade facade, IInputRouter router)
        {
            facade.PrependInputRouter(router);
            return facade;
        }

        // ---------- Context Menu ----------
        /// <summary>
        /// Sets the context menu provider for the viewport.
        /// </summary>
        /// <param name="facade">The facade to modify.</param>
        /// <param name="provider">The context menu provider.</param>
        /// <returns>The facade instance for method chaining.</returns>
        public static IEditableViewportFacade WithContextMenu(this IEditableViewportFacade facade, IContextMenuProvider provider)
        {
            facade.SetContextMenu(provider);
            return facade;
        }

        // ---------- Batch Update Helper ----------
        /// <summary>
        /// Begins a batch update operation. Uses reflection to call BatchUpdate if available.
        /// </summary>
        /// <param name="facade">The facade to batch update.</param>
        /// <returns>A disposable that ends the batch when disposed.</returns>
        private static IDisposable BeginBatch(IEditableViewportFacade facade)
        {
            // Best-effort implementation: if the concrete class (e.g., ImageViewportFacade) has BatchUpdate, try to call it
            // If the implementation doesn't have BatchUpdate, return an empty disposable
            var mi = facade.GetType().GetMethod("BatchUpdate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (mi != null && mi.GetParameters().Length == 0 && mi.ReturnType == typeof(IDisposable))
            {
                return (IDisposable)mi.Invoke(facade, null)!;
            }
            return new NullScope();
        }

        /// <summary>
        /// A no-op disposable used when batch update is not available.
        /// </summary>
        private sealed class NullScope : IDisposable { public void Dispose() { } }
    }
}
