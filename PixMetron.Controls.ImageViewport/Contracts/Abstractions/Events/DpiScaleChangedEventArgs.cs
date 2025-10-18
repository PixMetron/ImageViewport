namespace PixMetron.Controls.ImageViewport.Contracts.Abstractions.Events
{
    /// <summary>
    /// Provides data for the DPI scale changed event.
    /// </summary>
    public sealed class DpiScaleChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DpiScaleChangedEventArgs"/> class.
        /// </summary>
        /// <param name="x">The new horizontal DPI scale factor.</param>
        /// <param name="y">The new vertical DPI scale factor.</param>
        public DpiScaleChangedEventArgs(double x, double y)
        {
            DpiScaleX = x;
            DpiScaleY = y;
        }

        /// <summary>
        /// Gets the horizontal DPI scale factor.
        /// </summary>
        public double DpiScaleX { get; }

        /// <summary>
        /// Gets the vertical DPI scale factor.
        /// </summary>
        public double DpiScaleY { get; }
    }
}
