namespace PixMetron.Controls.ImageViewport.Contracts.Abstractions.Events
{
    /// <summary>
    /// Provides data for the window pixel size changed event.
    /// </summary>
    public sealed class WindowPixelSizeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowPixelSizeChangedEventArgs"/> class.
        /// </summary>
        /// <param name="newSize">The new pixel size of the window.</param>
        public WindowPixelSizeChangedEventArgs(PxSize newSize)
        {
            NewSize = newSize;
        }

        /// <summary>
        /// Gets the new pixel size of the window.
        /// </summary>
        public PxSize NewSize { get; }
    }
}