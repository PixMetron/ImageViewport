namespace PixMetron.Controls.ImageViewport.Contracts.Abstractions.Events
{
    /// <summary>
    /// Provides data for the scale changed event.
    /// </summary>
    public sealed class ScaleChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleChangedEventArgs"/> class.
        /// </summary>
        /// <param name="newScale">The new scale value.</param>
        public ScaleChangedEventArgs(double newScale)
        {
            NewScale = newScale;
        }

        /// <summary>
        /// Gets the new scale value.
        /// </summary>
        public double NewScale { get; }
    }
}