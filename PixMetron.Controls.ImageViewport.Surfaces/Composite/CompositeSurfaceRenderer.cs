using System.Windows.Media;

using PixMetron.Controls.ImageViewport.Contracts.Surfaces;

namespace PixMetron.Controls.ImageViewport.Surfaces.Composite
{
    /// <summary>
    /// A composite surface renderer that aggregates multiple surface renderers and executes them sequentially.
    /// </summary>
    /// <remarks>
    /// This renderer acts as a container for multiple <see cref="ISurfaceRenderer"/> instances,
    /// allowing them to be treated as a single logical rendering unit. All child renderers
    /// are executed in the order they appear in the array during the render phase.
    /// The composite itself operates in Follow mode.
    /// </remarks>
    public sealed class CompositeSurfaceRenderer : ISurfaceRenderer
    {
        private readonly ISurfaceRenderer[] _renderers;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeSurfaceRenderer"/> class with the specified child renderers.
        /// </summary>
        /// <param name="renderers">An array of <see cref="ISurfaceRenderer"/> instances to be rendered in sequence.</param>
        /// <remarks>
        /// The renderers are stored by reference and will be invoked in array order.
        /// The caller is responsible for ensuring the array contents remain valid during the lifetime of this instance.
        /// </remarks>
        public CompositeSurfaceRenderer(ISurfaceRenderer[] renderers)
        {
            _renderers = renderers;
        }

        /// <summary>
        /// Gets the transform mode for this renderer, which follows the viewport transformation.
        /// </summary>
        /// <value>Always returns <see cref="SurfaceMode.Follow"/>.</value>
        public SurfaceMode TransformMode => SurfaceMode.Follow;

        /// <summary>
        /// Renders all child surface renderers sequentially to the drawing context.
        /// </summary>
        /// <param name="dc">The drawing context to render into.</param>
        /// <param name="ctx">The render context containing viewport information and transforms.</param>
        /// <remarks>
        /// Each renderer in the internal array is invoked with the same drawing context and render context.
        /// Renderers are executed in the order they were provided during construction.
        /// Each child renderer's <see cref="ISurfaceRenderer.TransformMode"/> determines its own rendering behavior.
        /// </remarks>
        public void Render(DrawingContext dc, in SurfaceRenderContext ctx)
        {
            foreach (var r in _renderers)
            {
                r.Render(dc, in ctx);
            }
        }
    }
}