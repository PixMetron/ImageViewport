namespace PixMetron.Controls.ImageViewport.Handlers.Contracts
{
    /// <summary>
    /// Specifies which mouse button is used for pan operations in the image viewport.
    /// </summary>
    /// <remarks>
    /// This enumeration defines the available mouse button options for initiating
    /// drag-to-pan interactions, allowing users to customize the pan gesture
    /// based on their workflow preferences.
    /// </remarks>
    public enum PanButton
    {
        /// <summary>
        /// The left mouse button is used for panning.
        /// </summary>
        Left,

        /// <summary>
        /// The middle mouse button (wheel click) is used for panning.
        /// </summary>
        Middle,

        /// <summary>
        /// The right mouse button is used for panning.
        /// </summary>
        Right
    }
}