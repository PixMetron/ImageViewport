using System;
using System.Windows;
using System.Windows.Input;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Contracts.Input;
using PixMetron.Controls.ImageViewport.Handlers.Contracts;

namespace PixMetron.Controls.ImageViewport.Handlers.Routers
{
    /// <summary>
    /// A pure drag-to-pan handler that supports configurable mouse button mappings.
    /// </summary>
    /// <remarks>
    /// This handler enables viewport panning through mouse drag operations.
    /// It supports configuration of which mouse button (left, middle, or right) triggers panning,
    /// and can optionally suppress context menus when using the right button.
    /// The handler maintains mouse capture during drag operations to ensure consistent behavior
    /// even when the cursor moves outside the control boundaries.
    /// </remarks>
    public class PanHandler : IMouseButtonHandler, IMoveHandler
    {
        /// <summary>
        /// Gets or sets the mouse button used for drag-to-pan operations.
        /// </summary>
        /// <value>
        /// A <see cref="PanButton"/> value specifying which mouse button initiates panning.
        /// Default is <see cref="PanButton.Left"/>.
        /// Used when <see cref="PanButtonProvider"/> is null.
        /// </value>
        public PanButton PanButton { get; set; } = PanButton.Left;

        /// <summary>
        /// Gets or sets a function that dynamically provides the pan button configuration.
        /// </summary>
        /// <value>
        /// A function returning a <see cref="PanButton"/> value, or null to use the <see cref="PanButton"/> property.
        /// When set, this provider takes precedence over the static <see cref="PanButton"/> property,
        /// allowing for runtime configuration changes.
        /// </value>
        public Func<PanButton>? PanButtonProvider { get; set; }

        // Business state: whether dragging is in progress
        private bool _isDragging;
        private PxPoint _lastWindowPos;

        /// <summary>
        /// Handles mouse button down events to initiate panning operations.
        /// </summary>
        /// <param name="sender">The event sender, expected to be an <see cref="IInputElement"/> for mouse capture.</param>
        /// <param name="p">The pointer event containing button state and position information.</param>
        /// <returns>
        /// <c>true</c> if the event was handled and panning was initiated; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// When the configured pan button is pressed:
        /// </para>
        /// <list type="number">
        /// <item>Enters dragging mode and captures the initial window position.</item>
        /// <item>If using the right button, suppresses context menu to prevent interference.</item>
        /// <item>Captures the mouse to receive events even when cursor moves outside the control.</item>
        /// </list>
        /// </remarks>
        public bool OnMouseDown(object sender, PointerEvent p)
        {
            if (!IsPanButtonPressed(p))
                return false;

            _isDragging = true;
            _lastWindowPos = p.WindowPx;

            // If the configured drag button is Right, suppress context menu
            var button = PanButtonProvider?.Invoke() ?? PanButton;
            if (button == PanButton.Right)
            {
                p.SuppressContextMenu = true;
            }

            if (sender is IInputElement el)
            {
                el.CaptureMouse();
            }

            return true;
        }

        /// <summary>
        /// Handles mouse move events to perform panning operations.
        /// </summary>
        /// <param name="sender">The event sender, expected to be an <see cref="ImageViewport"/> instance.</param>
        /// <param name="p">The pointer event containing current position and button state information.</param>
        /// <returns>
        /// <c>true</c> if the event was handled and viewport was panned; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// During active dragging:
        /// </para>
        /// <list type="number">
        /// <item>Verifies the physical button state to handle asynchronous scenarios (e.g., button released outside window).</item>
        /// <item>If the button is no longer pressed, exits dragging mode and releases mouse capture.</item>
        /// <item>Calculates the movement delta in window coordinates.</item>
        /// <item>Applies the pan offset to the viewport if movement exceeds epsilon threshold.</item>
        /// <item>Updates the tracked position for the next move event.</item>
        /// </list>
        /// <para>
        /// Context menu suppression is maintained throughout the drag when using the right button.
        /// </para>
        /// </remarks>
        public bool OnMove(object sender, PointerEvent p)
        {
            if (!_isDragging)
                return false;

            // If the configured drag button is Right, suppress context menu
            var button = PanButtonProvider?.Invoke() ?? PanButton;
            if (button == PanButton.Right)
            {
                p.SuppressContextMenu = true;
            }

            // Check physical button state (prevent async issues: e.g., button released after moving out of window)
            if (!IsPanButtonPressed(p))
            {
                _isDragging = false;

                if (sender is IInputElement el && Mouse.Captured == el)
                {
                    el.ReleaseMouseCapture(); // Release capture
                }

                return false;
            }

            if (sender is not ImageViewport vp)
                return false;

            var dx = p.WindowPx.X - _lastWindowPos.X;
            var dy = p.WindowPx.Y - _lastWindowPos.Y;

            if (Math.Abs(dx) > double.Epsilon || Math.Abs(dy) > double.Epsilon)
            {
                vp.PanWindowPx(dx, dy);
                _lastWindowPos = p.WindowPx;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles mouse button up events to terminate panning operations.
        /// </summary>
        /// <param name="sender">The event sender, expected to be an <see cref="IInputElement"/> for mouse capture release.</param>
        /// <param name="p">The pointer event containing button state information.</param>
        /// <returns>
        /// <c>true</c> if the event was handled and dragging was terminated; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// When the mouse button is released during active dragging:
        /// </para>
        /// <list type="number">
        /// <item>Exits dragging mode.</item>
        /// <item>Releases mouse capture if it was previously acquired.</item>
        /// <item>If using the right button, maintains context menu suppression to prevent spurious menu display.</item>
        /// </list>
        /// </remarks>
        public bool OnMouseUp(object sender, PointerEvent p)
        {
            if (!_isDragging)
                return false;

            // If the configured drag button is Right, suppress context menu
            var button = PanButtonProvider?.Invoke() ?? PanButton;
            if (button == PanButton.Right)
            {
                p.SuppressContextMenu = true;
            }

            _isDragging = false;
            if (sender is IInputElement el && Mouse.Captured == el)
            {
                el.ReleaseMouseCapture(); // Release capture
            }

            return true;
        }

        /// <summary>
        /// Checks whether the configured pan button is currently pressed (physical state).
        /// </summary>
        /// <param name="p">The pointer event containing current button states.</param>
        /// <returns>
        /// <c>true</c> if the configured pan button is physically pressed; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method uses the provider delegate if available to dynamically retrieve the latest configuration,
        /// then checks the corresponding physical button state from the pointer event.
        /// This ensures correct behavior even if the button configuration changes during a drag operation.
        /// </remarks>
        private bool IsPanButtonPressed(PointerEvent p)
        {
            // Prioritize delegate to dynamically get the latest configuration
            var button = PanButtonProvider?.Invoke() ?? PanButton;

            return button switch
            {
                PanButton.Left => p.CurrentLeftPressed,
                PanButton.Middle => p.CurrentMiddlePressed,
                PanButton.Right => p.CurrentRightPressed,
                _ => false
            };
        }
    }
}