using PixMetron.Controls.ImageViewport.Contracts.Input;
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;

namespace PixMetron.Controls.ImageViewport.Facade
{
    /// <summary>
    /// Represents a surface that supports both rendering and input handling.
    /// This interface combines <see cref="ISurfaceRenderer"/> for visual representation
    /// with an optional <see cref="IInputRouter"/> for processing user interactions.
    /// </summary>
    public interface IInteractiveSurface : ISurfaceRenderer
    {
        /// <summary>
        /// Gets the input router responsible for handling user input events for this surface.
        /// </summary>
        /// <value>
        /// An <see cref="IInputRouter"/> instance that processes pointer events (mouse, touch),
        /// or <c>null</c> if this surface does not handle input.
        /// </value>
        IInputRouter? Router { get; }
    }

    /// <summary>
    /// Defines a prioritizable input handler that can specify its processing order
    /// in a composite input routing chain.
    /// </summary>
    /// <remarks>
    /// Input routers implementing this interface will be sorted by priority (higher values first)
    /// when aggregated in a composite router. Routers with the same priority are then ordered
    /// by their Z-index (higher values first).
    /// </remarks>
    public interface IInputPrioritizable
    {
        /// <summary>
        /// Gets the priority value for input event processing.
        /// </summary>
        /// <value>
        /// An integer representing the priority, where higher values indicate higher priority.
        /// Default priority is 0 for routers that don't implement this interface.
        /// </value>
        int Priority { get; }
    }
}