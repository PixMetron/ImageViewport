using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Contracts.Input;
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;

namespace PixMetron.Controls.ImageViewport.Contracts.Facade
{
    /// <summary>
    /// Represents the core facade interface for the image viewport control.
    /// Provides access to the viewport service, surface renderers, input routing, context menus, and coordinate transformations.
    /// </summary>
    public interface IViewportFacade
    {
        /// <summary>
        /// Gets the backend viewport service. Required. The control uses this to obtain the current viewport state and subscribe to changes.
        /// </summary>
        IViewportService Service { get; }

        /// <summary>
        /// Gets the collection of surfaces participating in rendering, ordered from bottom to top (Z-order). Required.
        /// </summary>
        IEnumerable<ISurfaceRenderer> Surfaces { get; }

        /// <summary>
        /// Gets the input router for handling user interactions. Optional.
        /// </summary>
        IInputRouter? InputRouter { get; }

        /// <summary>
        /// Gets the context menu provider for right-click menus. Optional.
        /// </summary>
        IContextMenuProvider? ContextMenu { get; }

        /// <summary>
        /// Returns an <see cref="IViewportTransforms"/> instance that is consistent with the version of the provided immutable snapshot.
        /// The control calls this method with a snapshot before each frame/render pass.
        /// </summary>
        /// <param name="info">The immutable viewport information snapshot.</param>
        /// <returns>A transforms instance for coordinate conversions matching the snapshot's version.</returns>
        IViewportTransforms GetTransforms(in ViewportInfo info);

        /// <summary>
        /// Multi-domain extension point. Optional. Allows retrieval of transforms for specific coordinate domains.
        /// </summary>
        /// <param name="domain">The name of the coordinate domain.</param>
        /// <returns>A transforms instance for the specified domain, or null if not supported.</returns>
        IViewportTransforms? TryGetTransforms(string domain) => null;
    }
}