namespace PixMetron.Controls.ImageViewport.Contracts.Abstractions
{
    /// <summary>
    /// Defines coordinate transformation methods between window pixel space and image pixel space.
    /// </summary>
    public interface IViewportTransforms
    {
        /// <summary>
        /// Converts a point from window pixel coordinates to image pixel coordinates.
        /// </summary>
        /// <param name="windowPx">The point in window pixel coordinates.</param>
        /// <returns>The corresponding point in image pixel coordinates.</returns>
        PxPoint WindowToImage(PxPoint windowPx);

        /// <summary>
        /// Converts a point from image pixel coordinates to window pixel coordinates.
        /// </summary>
        /// <param name="imagePx">The point in image pixel coordinates.</param>
        /// <returns>The corresponding point in window pixel coordinates.</returns>
        PxPoint ImageToWindow(PxPoint imagePx);

        /// <summary>
        /// Converts a rectangle from window pixel coordinates to image pixel coordinates.
        /// </summary>
        /// <param name="windowRect">The rectangle in window pixel coordinates.</param>
        /// <returns>The corresponding rectangle in image pixel coordinates.</returns>
        PxRect WindowToImage(PxRect windowRect);

        /// <summary>
        /// Converts a rectangle from image pixel coordinates to window pixel coordinates.
        /// </summary>
        /// <param name="imageRect">The rectangle in image pixel coordinates.</param>
        /// <returns>The corresponding rectangle in window pixel coordinates.</returns>
        PxRect ImageToWindow(PxRect imageRect);
    }
}