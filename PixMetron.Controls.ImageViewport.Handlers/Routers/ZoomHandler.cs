using System;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Contracts.Input;

namespace PixMetron.Controls.ImageViewport.Handlers.Routers
{
    /// <summary>
    /// A pure mouse wheel zoom handler that supports different pivot modes and scale range constraints.
    /// </summary>
    /// <remarks>
    /// This handler processes mouse wheel events to zoom the viewport in or out.
    /// It supports both static configuration via properties and dynamic configuration via function providers.
    /// The handler tracks mouse position for pivot point calculations and can operate in either
    /// window coordinate or image coordinate mode.
    /// </remarks>
    public class ZoomHandler : IWheelHandler, IMoveHandler
    {
        /// <summary>
        /// Gets or sets the scale factor applied per wheel notch.
        /// </summary>
        /// <value>The multiplicative scale factor. Default is 1.1. Used when <see cref="ScaleFactorProvider"/> is null.</value>
        public double ScaleFactor { get; set; } = 1.1;

        /// <summary>
        /// Gets or sets the minimum allowed scale value.
        /// </summary>
        /// <value>The minimum scale limit. Default is 0.01. Used when <see cref="MinScaleProvider"/> is null.</value>
        public double MinScale { get; set; } = 0.01;

        /// <summary>
        /// Gets or sets the maximum allowed scale value.
        /// </summary>
        /// <value>The maximum scale limit. Default is 100.0. Used when <see cref="MaxScaleProvider"/> is null.</value>
        public double MaxScale { get; set; } = 100.0;

        /// <summary>
        /// Gets or sets the pivot mode determining the zoom center point.
        /// </summary>
        /// <value>The <see cref="ZoomPivotMode"/> to use. Default is <see cref="ZoomPivotMode.Mouse"/>. Used when <see cref="PivotModeProvider"/> is null.</value>
        public ZoomPivotMode PivotMode { get; set; } = ZoomPivotMode.Mouse;

        /// <summary>
        /// Gets or sets the custom pivot point in window coordinates.
        /// </summary>
        /// <value>The custom pivot position. Used when <see cref="PivotMode"/> is <see cref="ZoomPivotMode.Custom"/> and <see cref="CustomPivotProvider"/> is null.</value>
        public PxPoint CustomPivot { get; set; }

        /// <summary>
        /// Gets or sets whether to use image coordinate-based zooming.
        /// </summary>
        /// <value>
        /// <c>true</c> to zoom using image coordinates (ignores pivot mode); 
        /// <c>false</c> to use window coordinates with pivot mode selection. 
        /// Default is <c>true</c>. Used when <see cref="UseImageCoordinateProvider"/> is null.
        /// </value>
        public bool UseImageCoordinate { get; set; } = true;

        /// <summary>
        /// Gets or sets a function that dynamically provides the scale factor.
        /// </summary>
        /// <value>A function returning the scale factor, or null to use <see cref="ScaleFactor"/>.</value>
        public Func<double>? ScaleFactorProvider { get; set; }

        /// <summary>
        /// Gets or sets a function that dynamically provides the minimum scale limit.
        /// </summary>
        /// <value>A function returning the minimum scale, or null to use <see cref="MinScale"/>.</value>
        public Func<double>? MinScaleProvider { get; set; }

        /// <summary>
        /// Gets or sets a function that dynamically provides the maximum scale limit.
        /// </summary>
        /// <value>A function returning the maximum scale, or null to use <see cref="MaxScale"/>.</value>
        public Func<double>? MaxScaleProvider { get; set; }

        /// <summary>
        /// Gets or sets a function that dynamically provides the pivot mode.
        /// </summary>
        /// <value>A function returning the <see cref="ZoomPivotMode"/>, or null to use <see cref="PivotMode"/>.</value>
        public Func<ZoomPivotMode>? PivotModeProvider { get; set; }

        /// <summary>
        /// Gets or sets a function that dynamically provides the custom pivot point.
        /// </summary>
        /// <value>A function returning the custom pivot position, or null to use <see cref="CustomPivot"/>.</value>
        public Func<PxPoint>? CustomPivotProvider { get; set; }

        /// <summary>
        /// Gets or sets a function that dynamically determines whether to use image coordinate-based zooming.
        /// </summary>
        /// <value>A function returning a boolean, or null to use <see cref="UseImageCoordinate"/>.</value>
        public Func<bool>? UseImageCoordinateProvider { get; set; }

        private PxPoint _lastMousePosWindow;
        private PxPoint _lastMousePosImage;

        /// <summary>
        /// Handles mouse wheel events to perform zoom operations.
        /// </summary>
        /// <param name="sender">The event sender, expected to be an <see cref="ImageViewport"/> instance.</param>
        /// <param name="p">The pointer event containing wheel delta and position information.</param>
        /// <returns><c>true</c> if the event was handled; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// <para>The zoom process:</para>
        /// <list type="number">
        /// <item>Retrieves configuration from providers or falls back to static properties.</item>
        /// <item>Calculates the scale factor based on wheel delta direction.</item>
        /// <item>Applies min/max scale constraints to the desired scale value.</item>
        /// <item>If the change is negligible, returns false without modifying the viewport.</item>
        /// <item>Performs the zoom using either image coordinates or window coordinates based on <see cref="UseImageCoordinate"/>.</item>
        /// </list>
        /// </remarks>
        public virtual bool OnWheel(object sender, PointerEvent p)
        {
            if (sender is not ImageViewport vp) return false;

            // Retrieve dynamic configuration
            var scaleFactor = ScaleFactorProvider?.Invoke() ?? ScaleFactor;
            var minScale = MinScaleProvider?.Invoke() ?? MinScale;
            var maxScale = MaxScaleProvider?.Invoke() ?? MaxScale;
            var useImageCoordinate = UseImageCoordinateProvider?.Invoke() ?? UseImageCoordinate;

            // Calculate zoom factor
            double factor = p.WheelDelta > 0 ? scaleFactor : 1.0 / scaleFactor;

            // Get current scale and apply range limits
            var current = vp.Scale <= 0 ? 1.0 : vp.Scale;
            var desired = current * factor;

            var minS = minScale > 0 ? minScale : 1e-6;
            var maxS = maxScale > minS ? maxScale : minS;
            desired = Math.Max(minS, Math.Min(maxS, desired));

            var actualFactor = desired / current;
            if (Math.Abs(actualFactor - 1.0) < 1e-9) return false; // No change

            // Choose between window or image coordinate based zooming
            if (useImageCoordinate)
            {
                vp.ZoomAtImagePx(actualFactor, _lastMousePosImage);
            } else
            {
                var pivotMode = PivotModeProvider?.Invoke() ?? PivotMode;
                var customPivot = CustomPivotProvider?.Invoke() ?? CustomPivot;

                PxPoint pivotWindow = pivotMode switch
                {
                    ZoomPivotMode.Mouse => _lastMousePosWindow,
                    ZoomPivotMode.Custom => customPivot,
                    ZoomPivotMode.Center => new PxPoint(
                        vp.WindowPixelSize.Width / 2.0,
                        vp.WindowPixelSize.Height / 2.0),
                    _ => _lastMousePosWindow
                };

                vp.ZoomAtWindowPx(actualFactor, pivotWindow);
            }

            return true;
        }

        /// <summary>
        /// Handles mouse move events to track cursor position.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="p">The pointer event containing position information.</param>
        /// <returns>Always returns <c>false</c> to allow the event to propagate.</returns>
        /// <remarks>
        /// This method updates internal position tracking fields for both window and image coordinates,
        /// which are used as potential pivot points during zoom operations. The move event itself is not intercepted.
        /// </remarks>
        public virtual bool OnMove(object sender, PointerEvent p)
        {
            // Track mouse position
            _lastMousePosWindow = p.WindowPx;
            _lastMousePosImage = p.ImagePx;
            return false; // Do not intercept move events
        }
    }
}