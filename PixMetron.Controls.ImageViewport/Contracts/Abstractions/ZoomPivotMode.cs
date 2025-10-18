namespace PixMetron.Controls.ImageViewport.Contracts.Abstractions
{
    /// <summary>
    /// Specifies the pivot mode for zoom operations.
    /// </summary>
    public enum ZoomPivotMode
    {
        /// <summary>
        /// Uses the mouse position as the zoom pivot point.
        /// </summary>
        Mouse,

        /// <summary>
        /// Uses the viewport center as the zoom pivot point.
        /// </summary>
        Center,

        /// <summary>
        /// Uses a custom position as the zoom pivot point.
        /// </summary>
        Custom
    }
}