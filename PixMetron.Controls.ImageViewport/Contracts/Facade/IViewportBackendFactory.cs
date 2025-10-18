using PixMetron.Controls.ImageViewport.Contracts.Abstractions;

namespace PixMetron.Controls.ImageViewport.Contracts.Facade
{
    /// <summary>
    /// Factory interface for creating viewport backend components.
    /// Provides a centralized way to instantiate viewport services and coordinate transforms.
    /// </summary>
    public interface IViewportBackendFactory
    {
        /// <summary>
        /// Creates a new instance of the viewport service that manages viewport state and operations.
        /// </summary>
        /// <returns>A new <see cref="IViewportService"/> implementation.</returns>
        IViewportService CreateViewportService();

        /// <summary>
        /// Creates a coordinate transformation instance based on the provided viewport snapshot.
        /// </summary>
        /// <param name="snapshot">The viewport information snapshot containing scale, position, and DPI data.</param>
        /// <returns>A new <see cref="IViewportTransforms"/> instance for coordinate conversions.</returns>
        IViewportTransforms CreateTransforms(ViewportInfo snapshot);
    }
}
