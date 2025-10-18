using System;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;

namespace PixMetron.Controls.ImageViewport.Handlers.Contracts
{
    /// <summary>
    /// Defines configuration options for pan and zoom operations in the image viewport.
    /// </summary>
    /// <remarks>
    /// This interface provides a centralized configuration contract for controlling
    /// pan and zoom behavior, including mouse button mappings, keyboard modifiers,
    /// coordinate systems, pivot modes, and scale constraints.
    /// </remarks>
    public interface IPanZoomOptions
    {
        /// <summary>
        /// Gets the mouse button used for drag-to-pan operations.
        /// </summary>
        /// <value>A <see cref="PanButton"/> value specifying which mouse button initiates panning.</value>
        PanButton PanButton { get; }

        /// <summary>
        /// Gets a value indicating whether the Ctrl key must be pressed for mouse wheel zoom operations.
        /// </summary>
        /// <value>
        /// <c>true</c> if Ctrl key is required for wheel zoom; 
        /// <c>false</c> if wheel zoom is enabled without modifiers.
        /// </value>
        bool RequireCtrlForWheelZoom { get; }

        /// <summary>
        /// Gets a value indicating whether zoom operations use image coordinate-based pivoting.
        /// </summary>
        /// <value>
        /// <c>true</c> to zoom around the mouse position in image coordinate space;
        /// <c>false</c> to use window coordinate space with the specified <see cref="WheelPivot"/> mode.
        /// </value>
        bool UseImageCoordinateZoom { get; }

        /// <summary>
        /// Gets the pivot mode for mouse wheel zoom operations.
        /// </summary>
        /// <value>
        /// A <see cref="ZoomPivotMode"/> value determining the zoom center point.
        /// This is only used when <see cref="UseImageCoordinateZoom"/> is <c>false</c>.
        /// </value>
        ZoomPivotMode WheelPivot { get; }

        /// <summary>
        /// Gets the scale factor applied per mouse wheel notch.
        /// </summary>
        /// <value>
        /// A multiplicative factor greater than 1.0 for zoom in/out operations.
        /// For example, 1.1 means 10% zoom per wheel notch.
        /// </value>
        double ScaleFactor { get; }

        /// <summary>
        /// Gets the minimum allowed scale value.
        /// </summary>
        /// <value>The lower bound for zoom operations, preventing excessive zoom out.</value>
        double MinScale { get; }

        /// <summary>
        /// Gets the maximum allowed scale value.
        /// </summary>
        /// <value>The upper bound for zoom operations, preventing excessive zoom in.</value>
        double MaxScale { get; }

        /// <summary>
        /// Gets a function that provides a custom pivot point in window pixel coordinates.
        /// </summary>
        /// <value>
        /// A function returning a <see cref="PxPoint"/> in window coordinates, or <c>null</c> if not used.
        /// This is only invoked when <see cref="WheelPivot"/> is set to <see cref="ZoomPivotMode.Custom"/>
        /// and <see cref="UseImageCoordinateZoom"/> is <c>false</c>.
        /// </value>
        Func<PxPoint>? CustomPivotWindowPxProvider { get; }
    }
}