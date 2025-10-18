namespace PixMetron.Controls.ImageViewport.Contracts.Surfaces
{
    /// <summary>
    /// Specifies the rendering mode for a surface.
    /// </summary>
    public enum SurfaceMode
    {
        /// <summary>
        /// The control uniformly pushes the viewport matrix. Drawing is performed using image pixel coordinates.
        /// </summary>
        Follow,

        /// <summary>
        /// The control does not perform matrix operations. The surface handles transformations independently (using window coordinates or custom matrix operations).
        /// </summary>
        Independent
    }
}