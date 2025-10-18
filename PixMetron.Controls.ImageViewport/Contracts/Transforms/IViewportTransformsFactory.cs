using PixMetron.Controls.ImageViewport.Contracts.Abstractions;

namespace PixMetron.Controls.ImageViewport.Contracts.Transforms
{
    /// <summary>
    /// Factory interface for creating <see cref="IViewportTransforms"/> instances from viewport snapshots.
    /// Constructs transforms that are consistent with the version of the immutable <see cref="ViewportInfo"/> snapshot.
    /// This interface is injected by the upper layer (caller/project) to avoid the Facade being tightly coupled to a specific implementation.
    /// </summary>
    public interface IViewportTransformsFactory
    {
        /// <summary>
        /// Creates an <see cref="IViewportTransforms"/> instance from the specified viewport information.
        /// </summary>
        /// <param name="info">The viewport information snapshot containing scale, position, and DPI data.</param>
        /// <returns>A new <see cref="IViewportTransforms"/> instance for coordinate transformations.</returns>
        IViewportTransforms Create(in ViewportInfo info);
    }
}