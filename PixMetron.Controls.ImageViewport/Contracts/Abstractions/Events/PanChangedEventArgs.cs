namespace PixMetron.Controls.ImageViewport.Contracts.Abstractions.Events
{
    /// <summary>
    /// Provides data for the pan changed event.
    /// </summary>
    public sealed class PanChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PanChangedEventArgs"/> class.
        /// </summary>
        /// <param name="newTopLeft">The new top-left position in image pixel coordinates.</param>
        public PanChangedEventArgs(PxPoint newTopLeft)
        {
            NewTopLeftInImagePx = newTopLeft;
        }

        /// <summary>
        /// Gets the new top-left position in image pixel coordinates.
        /// </summary>
        public PxPoint NewTopLeftInImagePx { get; }
    }
}