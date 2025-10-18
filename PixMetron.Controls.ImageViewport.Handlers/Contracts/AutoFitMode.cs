namespace PixMetron.Controls.ImageViewport.Handlers.Contracts
{
    /// <summary>
    /// Specifies the automatic fitting mode for viewport content adaptation.
    /// </summary>
    /// <remarks>
    /// This enumeration defines how the viewport should automatically adjust its content
    /// when the window size changes, providing different strategies for maintaining
    /// optimal view of the content.
    /// </remarks>
    public enum AutoFitMode
    {
        /// <summary>
        /// Automatic fitting is disabled. The viewport will not automatically adjust to window size changes.
        /// </summary>
        Disabled,

        /// <summary>
        /// Always maintain automatic fitting. The content will continuously adapt to any window size changes.
        /// </summary>
        Always,

        /// <summary>
        /// Fit only when the window grows larger. The content will adapt when the window expands.
        /// </summary>
        OnWindowGrow,

        /// <summary>
        /// Fit only when the window shrinks smaller. The content will adapt when the window contracts.
        /// </summary>
        OnWindowShrink
    }
}