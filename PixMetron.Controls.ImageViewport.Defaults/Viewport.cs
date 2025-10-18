using System;
using System.Windows;
using System.Windows.Media;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Handlers.Contracts;

namespace PixMetron.Controls.ImageViewport.Defaults
{
    /// <summary>
    /// Provides attached properties for configuring default viewport behaviors including image source,
    /// pan/zoom settings, and auto-fit modes. Automatically creates and manages a <see cref="DefaultImageViewportFacade"/>
    /// when any default property is set.
    /// </summary>
    public static class Viewport
    {
        // -------- Source ----------

        /// <summary>
        /// Identifies the Source attached property, which specifies the image source to display in the viewport.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached(
                "Source", typeof(ImageSource), typeof(Viewport),
                new PropertyMetadata(null, OnAnyDefaultChanged));

        /// <summary>
        /// Sets the image source for the specified <see cref="ImageViewport"/>.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <param name="value">The image source to display.</param>
        public static void SetSource(DependencyObject obj, ImageSource value) => obj.SetValue(SourceProperty, value);

        /// <summary>
        /// Gets the image source from the specified <see cref="ImageViewport"/>.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <returns>The current image source, or null if not set.</returns>
        public static ImageSource? GetSource(DependencyObject obj) => (ImageSource?)obj.GetValue(SourceProperty);

        // -------- Pan/Zoom DPs (all dynamically read, no push needed) ----------

        /// <summary>
        /// Identifies the PanButton attached property, which specifies which mouse button triggers panning.
        /// </summary>
        public static readonly DependencyProperty PanButtonProperty =
            DependencyProperty.RegisterAttached(
                "PanButton", typeof(PanButton), typeof(Viewport),
                new PropertyMetadata(PanButton.Middle, OnAnyDefaultChanged));

        /// <summary>
        /// Sets the mouse button that triggers panning for the specified <see cref="ImageViewport"/>.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <param name="value">The mouse button to use for panning.</param>
        public static void SetPanButton(DependencyObject obj, PanButton value) => obj.SetValue(PanButtonProperty, value);

        /// <summary>
        /// Gets the mouse button that triggers panning for the specified <see cref="ImageViewport"/>.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <returns>The configured pan button.</returns>
        public static PanButton GetPanButton(DependencyObject obj) => (PanButton)obj.GetValue(PanButtonProperty);

        /// <summary>
        /// Identifies the RequireCtrlForWheelZoom attached property, which specifies whether the Ctrl key must be held for mouse wheel zoom.
        /// </summary>
        public static readonly DependencyProperty RequireCtrlForWheelZoomProperty =
            DependencyProperty.RegisterAttached(
                "RequireCtrlForWheelZoom", typeof(bool), typeof(Viewport),
                new PropertyMetadata(false, OnAnyDefaultChanged));

        /// <summary>
        /// Sets whether the Ctrl key is required for mouse wheel zoom.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <param name="value">True to require Ctrl for wheel zoom; otherwise, false.</param>
        public static void SetRequireCtrlForWheelZoom(DependencyObject obj, bool value) => obj.SetValue(RequireCtrlForWheelZoomProperty, value);

        /// <summary>
        /// Gets whether the Ctrl key is required for mouse wheel zoom.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <returns>True if Ctrl is required; otherwise, false.</returns>
        public static bool GetRequireCtrlForWheelZoom(DependencyObject obj) => (bool)obj.GetValue(RequireCtrlForWheelZoomProperty);

        /// <summary>
        /// Identifies the UseImageCoordinateZoom attached property, which specifies whether zoom operations use image coordinates as the pivot point.
        /// </summary>
        public static readonly DependencyProperty UseImageCoordinateZoomProperty =
            DependencyProperty.RegisterAttached(
                "UseImageCoordinateZoom", typeof(bool), typeof(Viewport),
                new PropertyMetadata(true, OnAnyDefaultChanged));

        /// <summary>
        /// Sets whether zoom operations use image coordinate space for the pivot point.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <param name="value">True to use image coordinates; false to use window coordinates.</param>
        public static void SetUseImageCoordinateZoom(DependencyObject obj, bool value) => obj.SetValue(UseImageCoordinateZoomProperty, value);

        /// <summary>
        /// Gets whether zoom operations use image coordinate space for the pivot point.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <returns>True if image coordinates are used; otherwise, false.</returns>
        public static bool GetUseImageCoordinateZoom(DependencyObject obj) => (bool)obj.GetValue(UseImageCoordinateZoomProperty);

        /// <summary>
        /// Identifies the WheelPivot attached property, which specifies the pivot mode for mouse wheel zoom operations.
        /// </summary>
        public static readonly DependencyProperty WheelPivotProperty =
            DependencyProperty.RegisterAttached(
                "WheelPivot", typeof(ZoomPivotMode), typeof(Viewport),
                new PropertyMetadata(ZoomPivotMode.Mouse, OnAnyDefaultChanged));

        /// <summary>
        /// Sets the zoom pivot mode for mouse wheel operations.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <param name="value">The pivot mode to use.</param>
        public static void SetWheelPivot(DependencyObject obj, ZoomPivotMode value) => obj.SetValue(WheelPivotProperty, value);

        /// <summary>
        /// Gets the zoom pivot mode for mouse wheel operations.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <returns>The configured pivot mode.</returns>
        public static ZoomPivotMode GetWheelPivot(DependencyObject obj) => (ZoomPivotMode)obj.GetValue(WheelPivotProperty);

        /// <summary>
        /// Identifies the ScaleFactor attached property, which specifies the zoom factor applied per mouse wheel notch.
        /// </summary>
        public static readonly DependencyProperty ScaleFactorProperty =
            DependencyProperty.RegisterAttached(
                "ScaleFactor", typeof(double), typeof(Viewport),
                new PropertyMetadata(1.1, OnAnyDefaultChanged));

        /// <summary>
        /// Sets the zoom scale factor per mouse wheel notch.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <param name="value">The scale factor (e.g., 1.1 for 10% zoom per notch).</param>
        public static void SetScaleFactor(DependencyObject obj, double value) => obj.SetValue(ScaleFactorProperty, value);

        /// <summary>
        /// Gets the zoom scale factor per mouse wheel notch.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <returns>The configured scale factor.</returns>
        public static double GetScaleFactor(DependencyObject obj) => (double)obj.GetValue(ScaleFactorProperty);

        /// <summary>
        /// Identifies the MinScale attached property, which specifies the minimum allowed zoom scale.
        /// </summary>
        public static readonly DependencyProperty MinScaleProperty =
            DependencyProperty.RegisterAttached(
                "MinScale", typeof(double), typeof(Viewport),
                new PropertyMetadata(0.02, OnAnyDefaultChanged));

        /// <summary>
        /// Sets the minimum allowed zoom scale.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <param name="value">The minimum scale value.</param>
        public static void SetMinScale(DependencyObject obj, double value) => obj.SetValue(MinScaleProperty, value);

        /// <summary>
        /// Gets the minimum allowed zoom scale.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <returns>The minimum scale value.</returns>
        public static double GetMinScale(DependencyObject obj) => (double)obj.GetValue(MinScaleProperty);

        /// <summary>
        /// Identifies the MaxScale attached property, which specifies the maximum allowed zoom scale.
        /// </summary>
        public static readonly DependencyProperty MaxScaleProperty =
            DependencyProperty.RegisterAttached(
                "MaxScale", typeof(double), typeof(Viewport),
                new PropertyMetadata(40.0, OnAnyDefaultChanged));

        /// <summary>
        /// Sets the maximum allowed zoom scale.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <param name="value">The maximum scale value.</param>
        public static void SetMaxScale(DependencyObject obj, double value) => obj.SetValue(MaxScaleProperty, value);

        /// <summary>
        /// Gets the maximum allowed zoom scale.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <returns>The maximum scale value.</returns>
        public static double GetMaxScale(DependencyObject obj) => (double)obj.GetValue(MaxScaleProperty);

        /// <summary>
        /// Identifies the CustomPivotWindowPxProvider attached property, which provides a custom pivot point for zoom operations when <see cref="ZoomPivotMode.Custom"/> is used.
        /// </summary>
        public static readonly DependencyProperty CustomPivotWindowPxProviderProperty =
            DependencyProperty.RegisterAttached(
                "CustomPivotWindowPxProvider", typeof(Func<PxPoint>), typeof(Viewport),
                new PropertyMetadata(null, OnAnyDefaultChanged));

        /// <summary>
        /// Sets a custom pivot point provider for zoom operations.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <param name="value">A function that returns the custom pivot point in window pixel coordinates, or null to use default behavior.</param>
        public static void SetCustomPivotWindowPxProvider(DependencyObject obj, Func<PxPoint>? value) => obj.SetValue(CustomPivotWindowPxProviderProperty, value);

        /// <summary>
        /// Gets the custom pivot point provider for zoom operations.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <returns>The custom pivot point provider function, or null if not set.</returns>
        public static Func<PxPoint>? GetCustomPivotWindowPxProvider(DependencyObject obj) => (Func<PxPoint>?)obj.GetValue(CustomPivotWindowPxProviderProperty);

        // -------- AutoFit ----------

        /// <summary>
        /// Identifies the AutoFitMode attached property, which specifies when the image should automatically fit to the viewport.
        /// </summary>
        public static readonly DependencyProperty AutoFitModeProperty =
            DependencyProperty.RegisterAttached(
                "AutoFitMode", typeof(AutoFitMode), typeof(Viewport),
                new PropertyMetadata(AutoFitMode.Always, OnAnyDefaultChanged));

        /// <summary>
        /// Sets the auto-fit mode for the viewport.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <param name="value">The auto-fit mode to use.</param>
        public static void SetAutoFitMode(DependencyObject obj, AutoFitMode value) => obj.SetValue(AutoFitModeProperty, value);

        /// <summary>
        /// Gets the auto-fit mode for the viewport.
        /// </summary>
        /// <param name="obj">The target <see cref="ImageViewport"/>.</param>
        /// <returns>The configured auto-fit mode.</returns>
        public static AutoFitMode GetAutoFitMode(DependencyObject obj) => (AutoFitMode)obj.GetValue(AutoFitModeProperty);

        // -------- Runtime Attachment (Options + Facade Holder) ----------

        /// <summary>
        /// Holds runtime state including the facade and implements <see cref="IPanZoomOptions"/> by dynamically reading attached properties.
        /// </summary>
        private sealed class RuntimeState : IPanZoomOptions, IDisposable
        {
            public DefaultImageViewportFacade? Facade;
            public ImageViewport? Viewport;

            public PanButton PanButton => GetPanButton(Viewport!);
            public bool RequireCtrlForWheelZoom => GetRequireCtrlForWheelZoom(Viewport!);
            public bool UseImageCoordinateZoom => GetUseImageCoordinateZoom(Viewport!);
            public ZoomPivotMode WheelPivot => GetWheelPivot(Viewport!);
            public double ScaleFactor => GetScaleFactor(Viewport!);
            public double MinScale => GetMinScale(Viewport!);
            public double MaxScale => GetMaxScale(Viewport!);
            public Func<PxPoint>? CustomPivotWindowPxProvider => GetCustomPivotWindowPxProvider(Viewport!);

            public void Dispose()
            {
                Facade?.Dispose();
                Facade = null;
                Viewport = null;
            }
        }

        /// <summary>
        /// Internal attached property that stores the runtime state for each viewport.
        /// </summary>
        private static readonly DependencyProperty RuntimeProperty =
            DependencyProperty.RegisterAttached(
                "Runtime", typeof(RuntimeState), typeof(Viewport),
                new PropertyMetadata(null));

        private static RuntimeState? GetRuntime(DependencyObject obj) => (RuntimeState?)obj.GetValue(RuntimeProperty);
        private static void SetRuntime(DependencyObject obj, RuntimeState? value) => obj.SetValue(RuntimeProperty, value);

        /// <summary>
        /// Unified entry point: any default attached property change triggers attachment.
        /// </summary>
        private static void OnAnyDefaultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ImageViewport vp) return;

            var rt = EnsureAttached(vp);

            // Properties that need active "push": Source / AutoFitMode
            if (e.Property == SourceProperty)
            {
                rt.Facade?.SetSource(e.NewValue as ImageSource);
            } else if (e.Property == AutoFitModeProperty && rt.Facade is not null)
            {
                rt.Facade.SetAutoFitMode((AutoFitMode)e.NewValue!);
            }
            // Other properties like PanButton/ScaleFactor/MinScale/MaxScale/Pivot/RequireCtrl...
            // are dynamically read by ZoomHandler, no push needed here.
        }

        /// <summary>
        /// Ensures the runtime state is attached when any default property is set.
        /// No explicit "Enabled" property; attachment occurs automatically when any default property appears.
        /// </summary>
        private static RuntimeState EnsureAttached(ImageViewport vp)
        {
            var rt = GetRuntime(vp);
            if (rt != null) return rt;

            rt = new RuntimeState { Viewport = vp };
            SetRuntime(vp, rt);

            if (vp.Facade is null)
            {
                var src = GetSource(vp);
                rt.Facade = new DefaultImageViewportFacade(vp, rt, src);
                vp.Facade = rt.Facade;

                if (src != null) rt.Facade.SetSource(src);
                // Synchronize initial AutoFitMode
                rt.Facade.SetAutoFitMode(GetAutoFitMode(vp));
            }

            vp.Unloaded -= OnViewportUnloaded;
            vp.Unloaded += OnViewportUnloaded;
            return rt;
        }

        /// <summary>
        /// Handles viewport unload by disposing runtime state and cleaning up resources.
        /// </summary>
        private static void OnViewportUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is ImageViewport vp)
            {
                var rt = GetRuntime(vp);
                rt?.Dispose();
                SetRuntime(vp, null);
            }
        }
    }
}