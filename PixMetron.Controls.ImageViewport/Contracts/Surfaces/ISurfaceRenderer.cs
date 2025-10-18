using System.Windows.Media;

namespace PixMetron.Controls.ImageViewport.Contracts.Surfaces
{
    /// <summary>
    /// Defines a renderer that draws content onto a surface within the viewport.
    /// Surfaces can render either in image coordinate space (following the viewport transform) 
    /// or in window coordinate space (independent rendering).
    /// </summary>
    public interface ISurfaceRenderer
    {
        /// <summary>
        /// Gets the transform mode for this surface layer, determining whether it follows the viewport transform or renders independently.
        /// </summary>
        /// <value>
        /// <see cref="SurfaceMode.Follow"/> if the surface should render in image coordinates with the viewport transform applied,
        /// or <see cref="SurfaceMode.Independent"/> if the surface should render in window coordinates without transform.
        /// </value>
        SurfaceMode TransformMode { get; }

        /// <summary>
        /// Renders the surface content to the drawing context.
        /// </summary>
        /// <param name="dc">The drawing context to render into.</param>
        /// <param name="ctx">The render context containing viewport information, transforms, and rendering state.</param>
        /// <remarks>
        /// <para>
        /// When <see cref="TransformMode"/> is <see cref="SurfaceMode.Follow"/>:
        /// The control has already pushed the viewport matrix (<see cref="SurfaceRenderContext.ViewportMatrix"/>),
        /// so rendering should be done using image pixel coordinates. The coordinate system is automatically 
        /// transformed from image space to window space.
        /// </para>
        /// <para>
        /// When <see cref="TransformMode"/> is <see cref="SurfaceMode.Independent"/>:
        /// The control does not apply any matrix transform. Rendering should be done using window pixel coordinates
        /// or with custom transformation logic. This mode is useful for UI overlays, rulers, and other window-aligned elements.
        /// </para>
        /// </remarks>
        void Render(DrawingContext dc, in SurfaceRenderContext ctx);
    }
}