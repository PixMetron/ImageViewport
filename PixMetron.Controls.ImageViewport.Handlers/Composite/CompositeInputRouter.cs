using PixMetron.Controls.ImageViewport.Contracts.Input;

namespace PixMetron.Controls.ImageViewport.Handlers.Composite
{
    /// <summary>
    /// A composite input router that aggregates multiple input routers and executes them sequentially.
    /// </summary>
    /// <remarks>
    /// This router acts as a container for multiple <see cref="IInputRouter"/> instances,
    /// allowing them to be treated as a single logical input handling unit. All child routers
    /// are invoked for each event in the order they appear in the array, and the composite
    /// returns true if any of the child routers handled the event.
    /// </remarks>
    public sealed class CompositeInputRouter : IInputRouter
    {
        private readonly IInputRouter[] _routers;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeInputRouter"/> class with the specified child routers.
        /// </summary>
        /// <param name="routers">An array of <see cref="IInputRouter"/> instances to be invoked in sequence.</param>
        /// <remarks>
        /// The routers are stored by reference and will be invoked in array order for each event.
        /// The caller is responsible for ensuring the array contents remain valid during the lifetime of this instance.
        /// </remarks>
        public CompositeInputRouter(IInputRouter[] routers) { _routers = routers; }

        /// <summary>
        /// Handles left mouse button down events by invoking all child routers.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="p">The pointer event containing button state and position information.</param>
        /// <returns><c>true</c> if any child router handled the event; otherwise, <c>false</c>.</returns>
        public bool OnLeftDown(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnLeftDown(sender, p) || h; return h; }

        /// <summary>
        /// Handles left mouse button up events by invoking all child routers.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="p">The pointer event containing button state and position information.</param>
        /// <returns><c>true</c> if any child router handled the event; otherwise, <c>false</c>.</returns>
        public bool OnLeftUp(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnLeftUp(sender, p) || h; return h; }

        /// <summary>
        /// Handles right mouse button down events by invoking all child routers.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="p">The pointer event containing button state and position information.</param>
        /// <returns><c>true</c> if any child router handled the event; otherwise, <c>false</c>.</returns>
        public bool OnRightDown(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnRightDown(sender, p) || h; return h; }

        /// <summary>
        /// Handles right mouse button up events by invoking all child routers.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="p">The pointer event containing button state and position information.</param>
        /// <returns><c>true</c> if any child router handled the event; otherwise, <c>false</c>.</returns>
        public bool OnRightUp(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnRightUp(sender, p) || h; return h; }

        /// <summary>
        /// Handles mouse move events by invoking all child routers.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="p">The pointer event containing current position and button state information.</param>
        /// <returns><c>true</c> if any child router handled the event; otherwise, <c>false</c>.</returns>
        public bool OnMove(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnMove(sender, p) || h; return h; }

        /// <summary>
        /// Handles mouse wheel events by invoking all child routers.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="p">The pointer event containing wheel delta and position information.</param>
        /// <returns><c>true</c> if any child router handled the event; otherwise, <c>false</c>.</returns>
        public bool OnWheel(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnWheel(sender, p) || h; return h; }

        /// <summary>
        /// Handles generic mouse button down events by invoking all child routers.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="p">The pointer event containing button state and position information.</param>
        /// <returns><c>true</c> if any child router handled the event; otherwise, <c>false</c>.</returns>
        public bool OnMouseDown(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnMouseDown(sender, p) || h; return h; }

        /// <summary>
        /// Handles generic mouse button up events by invoking all child routers.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="p">The pointer event containing button state and position information.</param>
        /// <returns><c>true</c> if any child router handled the event; otherwise, <c>false</c>.</returns>
        public bool OnMouseUp(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnMouseUp(sender, p) || h; return h; }

        /// <summary>
        /// Handles middle mouse button down events by invoking all child routers.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="p">The pointer event containing button state and position information.</param>
        /// <returns><c>true</c> if any child router handled the event; otherwise, <c>false</c>.</returns>
        public bool OnMiddleDown(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnMiddleDown(sender, p) || h; return h; }

        /// <summary>
        /// Handles middle mouse button up events by invoking all child routers.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="p">The pointer event containing button state and position information.</param>
        /// <returns><c>true</c> if any child router handled the event; otherwise, <c>false</c>.</returns>
        public bool OnMiddleUp(object sender, PointerEvent p) { bool h = false; foreach (var r in _routers) h = r.OnMiddleUp(sender, p) || h; return h; }
    }
}