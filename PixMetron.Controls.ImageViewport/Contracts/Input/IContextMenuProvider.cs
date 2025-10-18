using System.Windows.Controls;

namespace PixMetron.Controls.ImageViewport.Contracts.Input
{
    /// <summary>
    /// Defines a provider for building context menus based on pointer events.
    /// </summary>
    public interface IContextMenuProvider
    {
        /// <summary>
        /// Builds a context menu for the specified pointer event.
        /// </summary>
        /// <param name="p">The pointer event containing information about the mouse position and state.</param>
        /// <returns>A <see cref="ContextMenu"/> instance to display, or null if no menu should be shown.</returns>
        ContextMenu? BuildContextMenu(PointerEvent p);
    }
}