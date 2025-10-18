using System.Windows;
using System.Windows.Input;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Contracts.Input;
using PixMetron.Controls.ImageViewport.Handlers.Contracts;

namespace PixMetron.Controls.ImageViewport.Handlers.Routers
{
    /// <summary>
    /// A composite handler that combines drag-to-pan and mouse wheel zoom operations.
    /// </summary>
    /// <remarks>
    /// This handler internally composes a <see cref="PanHandler"/> and a <see cref="ZoomHandler"/>
    /// to provide unified pan and zoom interaction. It supports configurable keyboard modifiers
    /// to conditionally enable or disable specific gestures, such as requiring Ctrl for zoom
    /// or blocking pan when Shift is pressed.
    /// </remarks>
    public sealed class PanZoomHandler : IInputRouter
    {
        private readonly PanHandler _panHandler;
        private readonly ZoomHandler _zoomHandler;
        private readonly IPanZoomOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="PanZoomHandler"/> class with the specified options.
        /// </summary>
        /// <param name="options">
        /// The configuration options for pan and zoom behavior, or null to use default options.
        /// </param>
        /// <remarks>
        /// <para>
        /// This constructor initializes the internal pan and zoom handlers with provider delegates
        /// that dynamically retrieve configuration from the options instance. This allows for
        /// runtime configuration changes without recreating the handler.
        /// </para>
        /// <para>
        /// If no options are provided, a default <see cref="PanZoomOptions"/> instance is used.
        /// </para>
        /// </remarks>
        public PanZoomHandler(IPanZoomOptions? options = null)
        {
            _options = options ?? new PanZoomOptions();

            // Initialize child handlers
            _panHandler = new PanHandler
            {
                PanButtonProvider = () => _options.PanButton
            };

            _zoomHandler = new ZoomHandler
            {
                ScaleFactorProvider = () => _options.ScaleFactor,
                MinScaleProvider = () => _options.MinScale,
                MaxScaleProvider = () => _options.MaxScale,
                PivotModeProvider = () => _options.WheelPivot,
                CustomPivotProvider = () => _options.CustomPivotWindowPxProvider?.Invoke() ?? new PxPoint()
            };
        }

        /// <summary>
        /// Handles mouse wheel events to perform zoom operations.
        /// </summary>
        /// <param name="sender">The event sender, expected to be an <see cref="ImageViewport"/> instance.</param>
        /// <param name="p">The pointer event containing wheel delta and position information.</param>
        /// <returns>
        /// <c>true</c> if the event was handled and zoom was performed; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// If <see cref="IPanZoomOptions.RequireCtrlForWheelZoom"/> is true, the Ctrl key must be pressed
        /// for the zoom operation to proceed. Otherwise, the event is ignored and returns false.
        /// </remarks>
        public bool OnWheel(object sender, PointerEvent p)
        {
            // Check Ctrl modifier requirement
            if (_options.RequireCtrlForWheelZoom && !p.Modifiers.HasFlag(ModifierKeys.Control))
                return false;

            return _zoomHandler.OnWheel(sender, p);
        }

        /// <summary>
        /// Handles mouse button down events to initiate panning operations.
        /// </summary>
        /// <param name="sender">The event sender, expected to be an <see cref="IInputElement"/> for mouse capture.</param>
        /// <param name="p">The pointer event containing button state and position information.</param>
        /// <returns>
        /// <c>true</c> if the event was handled and panning was initiated; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// If the Shift key is pressed, panning is blocked to allow other interactions (such as selection).
        /// Otherwise, the event is forwarded to the internal <see cref="PanHandler"/>.
        /// </remarks>
        public bool OnMouseDown(object sender, PointerEvent p)
        {
            // Check Shift modifier (blocks dragging)
            if (p.Modifiers.HasFlag(ModifierKeys.Shift))
                return false;

            return _panHandler.OnMouseDown(sender, p);
        }

        /// <summary>
        /// Handles mouse move events to perform panning and update zoom pivot tracking.
        /// </summary>
        /// <param name="sender">The event sender, expected to be an <see cref="ImageViewport"/> instance.</param>
        /// <param name="p">The pointer event containing current position and button state information.</param>
        /// <returns>
        /// <c>true</c> if the event was handled by panning; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method prioritizes pan handling, then updates the zoom handler's mouse position tracking
        /// for future pivot point calculations. The zoom handler does not intercept the move event,
        /// only observing the position.
        /// </remarks>
        public bool OnMove(object sender, PointerEvent p)
        {
            // Prioritize pan handling, then update zoom anchor
            var handled = _panHandler.OnMove(sender, p);
            _zoomHandler.OnMove(sender, p); // Does not intercept, only updates position
            return handled;
        }

        /// <summary>
        /// Handles mouse button up events to terminate panning operations.
        /// </summary>
        /// <param name="sender">The event sender, expected to be an <see cref="IInputElement"/> for mouse capture release.</param>
        /// <param name="p">The pointer event containing button state information.</param>
        /// <returns>
        /// <c>true</c> if the event was handled and dragging was terminated; otherwise, <c>false</c>.
        /// </returns>
        public bool OnMouseUp(object sender, PointerEvent p)
        {
            return _panHandler.OnMouseUp(sender, p);
        }

        // IInputRouter default implementations (provided by the interface)
        // OnLeftDown, OnLeftUp, OnRightDown, OnRightUp, OnMiddleDown, OnMiddleUp
        // all use default implementations (return false)
    }
}