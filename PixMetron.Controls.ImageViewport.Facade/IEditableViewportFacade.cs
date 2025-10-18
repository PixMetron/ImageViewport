// File: PixMetron.Controls.ImageViewport.Facade.Abstractions/IEditableViewportFacade.cs
using PixMetron.Controls.ImageViewport.Contracts.Facade;
using PixMetron.Controls.ImageViewport.Contracts.Input;
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;

namespace PixMetron.Controls.ImageViewport.Facade
{
    /// <summary>
    /// Represents an editable, general-purpose facade interface that extends the base <see cref="IViewportFacade"/> contract.
    /// Provides comprehensive control over surface management, grouping, Z-order manipulation, input routing composition, and context menu configuration.
    /// </summary>
    /// <remarks>
    /// This interface enables dynamic modification of the viewport's rendering layers and input handling behavior at runtime.
    /// It supports advanced scenarios such as:
    /// <list type="bullet">
    /// <item><description>Dynamic surface addition, removal, and reordering for complex multi-layer visualizations</description></item>
    /// <item><description>Group-based batch operations for scene management and layer switching</description></item>
    /// <item><description>Composable input routing chains with priority control</description></item>
    /// <item><description>Runtime context menu customization</description></item>
    /// </list>
    /// </remarks>
    public interface IEditableViewportFacade : Contracts.Facade.IViewportFacade
    {
        // ---- Surface Collection (ordered by Z-index: higher value = top layer) ----

        /// <summary>
        /// Gets the mutable list of visible surfaces ordered by Z-index (bottom to top).
        /// </summary>
        /// <value>
        /// A mutable collection of <see cref="ISurfaceRenderer"/> instances that are currently visible.
        /// The collection reflects the rendering order where index 0 represents the bottom layer.
        /// </value>
        /// <remarks>
        /// The facade implementation may synchronize this collection internally with visibility and Z-order changes.
        /// Direct manipulation through collection methods will be reflected in the underlying surface management system.
        /// </remarks>
        IList<ISurfaceRenderer> SurfacesMutable { get; }

        /// <summary>
        /// Adds a surface to the top layer (end of the rendering stack).
        /// </summary>
        /// <param name="surface">The surface renderer to add. Cannot be null.</param>
        /// <returns><c>true</c> if the surface was added successfully; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// The surface will be assigned a Z-index higher than all existing surfaces,
        /// ensuring it renders on top of all other layers.
        /// </remarks>
        bool AddSurface(ISurfaceRenderer surface);

        /// <summary>
        /// Inserts a surface at the specified Z-order index.
        /// </summary>
        /// <param name="index">The zero-based index at which to insert the surface. Index 0 represents the bottom layer.</param>
        /// <param name="surface">The surface renderer to insert. Cannot be null.</param>
        /// <returns><c>true</c> if the surface was inserted successfully; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Existing surfaces at or above the specified index will have their Z-indices adjusted to accommodate the new surface.
        /// If the index exceeds the current surface count, the surface will be added to the top layer.
        /// </remarks>
        bool InsertSurface(int index, ISurfaceRenderer surface);

        /// <summary>
        /// Removes the specified surface instance from the rendering stack.
        /// </summary>
        /// <param name="surface">The surface renderer to remove.</param>
        /// <returns><c>true</c> if the surface was found and removed; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// The removal is based on reference equality. Associated input routers and metadata will also be removed.
        /// </remarks>
        bool RemoveSurface(ISurfaceRenderer surface);

        /// <summary>
        /// Sets the visibility of the specified surface.
        /// </summary>
        /// <param name="surface">The surface renderer to modify.</param>
        /// <param name="visible"><c>true</c> to make the surface visible; <c>false</c> to hide it.</param>
        /// <returns><c>true</c> if the visibility state was changed; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Hiding a surface removes it from rendering but preserves its Z-order and other properties.
        /// Hidden surfaces may still process input events if their associated routers are configured accordingly.
        /// </remarks>
        bool SetSurfaceVisible(ISurfaceRenderer surface, bool visible);

        // ---- Z-Order Operations ----

        /// <summary>
        /// Brings the specified surface to the front, making it the topmost layer.
        /// </summary>
        /// <param name="surface">The surface renderer to bring to front.</param>
        /// <returns><c>true</c> if the operation succeeded; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// The surface will be assigned a Z-index higher than all existing surfaces,
        /// ensuring it renders on top of all other layers and receives input events first (if configured).
        /// </remarks>
        bool BringToFront(ISurfaceRenderer surface);

        /// <summary>
        /// Sends the specified surface to the back, making it the bottommost layer.
        /// </summary>
        /// <param name="surface">The surface renderer to send to back.</param>
        /// <returns><c>true</c> if the operation succeeded; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// The surface will be assigned a Z-index lower than all existing surfaces,
        /// ensuring it renders below all other layers and receives input events last (if configured).
        /// </remarks>
        bool SendToBack(ISurfaceRenderer surface);

        /// <summary>
        /// Moves the specified surface to the given Z-order index.
        /// </summary>
        /// <param name="surface">The surface renderer to move.</param>
        /// <param name="index">The target zero-based index. Index 0 represents the bottom layer.</param>
        /// <returns><c>true</c> if the operation succeeded; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Other surfaces will have their Z-indices adjusted to accommodate the moved surface.
        /// If the index is out of bounds, it will be clamped to the valid range.
        /// </remarks>
        bool MoveTo(ISurfaceRenderer surface, int index);

        /// <summary>
        /// Assigns or updates the group name for a surface.
        /// </summary>
        /// <param name="surface">The surface renderer to modify.</param>
        /// <param name="group">The group name to assign, or <c>null</c> to remove the surface from any group.</param>
        /// <returns><c>true</c> if the group assignment was successful; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Groups enable batch operations such as hiding, showing, or clearing multiple surfaces simultaneously.
        /// This is particularly useful for layered scene switching and managing related visualization elements.
        /// Group assignment does not affect Z-order or rendering behavior directly.
        /// </remarks>
        bool SetSurfaceGroup(ISurfaceRenderer surface, string? group);

        // ---- Group Operations ----

        /// <summary>
        /// Hides all surfaces in the specified group.
        /// </summary>
        /// <param name="group">The name of the group to hide. Cannot be null or empty.</param>
        /// <returns>The number of surfaces that were hidden as a result of this operation.</returns>
        /// <remarks>
        /// This operation preserves all surface properties (Z-order, group membership) except visibility.
        /// Surfaces can be shown again using <see cref="ShowGroup"/>.
        /// </remarks>
        int HideGroup(string group);

        /// <summary>
        /// Shows all surfaces in the specified group.
        /// </summary>
        /// <param name="group">The name of the group to show. Cannot be null or empty.</param>
        /// <returns>The number of surfaces that were made visible as a result of this operation.</returns>
        /// <remarks>
        /// This operation makes previously hidden group members visible again, restoring them to their original Z-order positions.
        /// </remarks>
        int ShowGroup(string group);

        /// <summary>
        /// Removes all surfaces in the specified group from the rendering stack.
        /// </summary>
        /// <param name="group">The name of the group to clear. Cannot be null or empty.</param>
        /// <returns>The number of surfaces that were removed as a result of this operation.</returns>
        /// <remarks>
        /// This operation permanently removes all group members. Removed surfaces cannot be recovered
        /// and must be re-added if needed. This is useful for disposing of temporary visualization layers.
        /// </remarks>
        int ClearGroup(string group);

        // ---- Input Router Composition ----

        /// <summary>
        /// Sets the primary input router, replacing any existing router configuration.
        /// </summary>
        /// <param name="router">The input router to set, or <c>null</c> to clear all input routing.</param>
        /// <remarks>
        /// This operation replaces the entire input routing chain. Any previously configured routers
        /// (including those from surfaces) will be discarded unless they are re-added through composition methods.
        /// </remarks>
        void SetInputRouter(IInputRouter? router);

        /// <summary>
        /// Prepends an input router to the beginning of the router chain, giving it highest priority.
        /// </summary>
        /// <param name="router">The input router to prepend. Cannot be null.</param>
        /// <remarks>
        /// Prepended routers receive input events before any existing routers, including surface-specific routers.
        /// This is useful for implementing global input interceptors or modality systems.
        /// </remarks>
        void PrependInputRouter(IInputRouter router);

        /// <summary>
        /// Appends an input router to the end of the router chain, giving it lowest priority.
        /// </summary>
        /// <param name="router">The input router to append. Cannot be null.</param>
        /// <remarks>
        /// Appended routers receive input events only if all preceding routers (including surface-specific routers)
        /// have not handled the event. This is useful for implementing fallback or default input behaviors.
        /// </remarks>
        void AppendInputRouter(IInputRouter router);

        // ---- Context Menu ----

        /// <summary>
        /// Sets the context menu provider for the viewport.
        /// </summary>
        /// <param name="provider">The context menu provider to use, or <c>null</c> to disable context menus.</param>
        /// <remarks>
        /// The provider will be invoked when the user triggers a context menu action (typically right-click).
        /// The provider can return different menus based on the pointer event information, enabling context-sensitive menus.
        /// </remarks>
        void SetContextMenu(IContextMenuProvider? provider);
    }
}