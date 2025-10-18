using System;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;

namespace PixMetron.Controls.ImageViewport.Handlers.Contracts
{
    /// <summary>
    /// Provides a concrete implementation of pan and zoom configuration options for the image viewport.
    /// </summary>
    /// <remarks>
    /// This class offers default values and mutable properties for all pan and zoom settings,
    /// allowing easy customization of viewport interaction behavior. It always uses image
    /// coordinate-based zooming for consistent behavior across different viewport states.
    /// </remarks>
    public sealed class PanZoomOptions : IPanZoomOptions
    {
        /// <summary>
        /// Gets or sets the mouse button used for drag-to-pan operations.
        /// </summary>
        /// <value>A <see cref="PanButton"/> value specifying which mouse button initiates panning. Default is <see cref="PanButton.Middle"/>.</value>
        public PanButton PanButton { get; set; } = PanButton.Middle;

        /// <summary>
        /// Gets or sets a value indicating whether the Ctrl key must be pressed for mouse wheel zoom operations.
        /// </summary>
        /// <value>
        /// <c>true</c> if Ctrl key is required for wheel zoom; 
        /// <c>false</c> if wheel zoom is enabled without modifiers. Default is <c>false</c>.
        /// </value>
        public bool RequireCtrlForWheelZoom { get; set; } = false;

        /// <summary>
        /// Gets a value indicating that zoom operations always use image coordinate-based pivoting.
        /// </summary>
        /// <value>Always returns <c>true</c> to ensure consistent zoom behavior in image space.</value>
        /// <remarks>
        /// This property is read-only and overrides the <see cref="WheelPivot"/> setting,
        /// ensuring that zoom operations always maintain the image point under the cursor.
        /// </remarks>
        public bool UseImageCoordinateZoom => true;

        /// <summary>
        /// Gets or sets the pivot mode for mouse wheel zoom operations.
        /// </summary>
        /// <value>
        /// A <see cref="ZoomPivotMode"/> value determining the zoom center point. Default is <see cref="ZoomPivotMode.Mouse"/>.
        /// </value>
        /// <remarks>
        /// This property is not used when <see cref="UseImageCoordinateZoom"/> returns <c>true</c>,
        /// but is retained for interface compatibility.
        /// </remarks>
        public ZoomPivotMode WheelPivot { get; set; } = ZoomPivotMode.Mouse;

        /// <summary>
        /// Gets or sets the scale factor applied per mouse wheel notch.
        /// </summary>
        /// <value>
        /// A multiplicative factor for zoom in/out operations. Default is 1.1 (10% zoom per wheel notch).
        /// Values greater than 1.0 provide zoom in when scrolling up.
        /// </value>
        public double ScaleFactor { get; set; } = 1.1;

        /// <summary>
        /// Gets or sets the minimum allowed scale value.
        /// </summary>
        /// <value>The lower bound for zoom operations, preventing excessive zoom out. Default is 0.01.</value>
        public double MinScale { get; set; } = 0.01;

        /// <summary>
        /// Gets or sets the maximum allowed scale value.
        /// </summary>
        /// <value>The upper bound for zoom operations, preventing excessive zoom in. Default is 1000.0.</value>
        public double MaxScale { get; set; } = 1000.0;

        /// <summary>
        /// Gets or sets a function that provides a custom pivot point in window pixel coordinates.
        /// </summary>
        /// <value>
        /// A function returning a <see cref="PxPoint"/> in window coordinates, or <c>null</c> if not used.
        /// </value>
        /// <remarks>
        /// This property is not used when <see cref="UseImageCoordinateZoom"/> returns <c>true</c>,
        /// but is retained for interface compatibility. It would only be invoked when <see cref="WheelPivot"/>
        /// is set to <see cref="ZoomPivotMode.Custom"/> in window coordinate zoom mode.
        /// </remarks>
        public Func<PxPoint>? CustomPivotWindowPxProvider { get; set; }
    }
}