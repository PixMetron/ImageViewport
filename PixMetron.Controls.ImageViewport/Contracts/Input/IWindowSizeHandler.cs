using PixMetron.Controls.ImageViewport.Contracts.Abstractions;

namespace PixMetron.Controls.ImageViewport.Contracts.Input
{
    /// <summary>
    /// Defines a handler for window size change events.
    /// </summary>
    public interface IWindowSizeHandler
    {
        /// <summary>
        /// Handles the window size changed event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="newSize">The new window size in pixels.</param>
        /// <returns>True if the event was handled; otherwise, false.</returns>
        bool OnWindowSizeChanged(object sender, PxSize newSize);
    }
}