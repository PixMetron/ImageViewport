using System;
using System.Windows;
using System.Windows.Media;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Handlers.Contracts;

namespace PixMetron.Controls.ImageViewport.Defaults
{
    public static class Viewport
    {
        // -------- Source ----------
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached(
                "Source", typeof(ImageSource), typeof(Viewport),
                new PropertyMetadata(null, OnAnyDefaultChanged));
        public static void SetSource(DependencyObject obj, ImageSource value) => obj.SetValue(SourceProperty, value);
        public static ImageSource? GetSource(DependencyObject obj) => (ImageSource?)obj.GetValue(SourceProperty);

        // -------- Pan/Zoom DPs（全部动态读取，无需推送） ----------
        public static readonly DependencyProperty PanButtonProperty =
            DependencyProperty.RegisterAttached(
                "PanButton", typeof(PanButton), typeof(Viewport),
                new PropertyMetadata(PanButton.Middle, OnAnyDefaultChanged));
        public static void SetPanButton(DependencyObject obj, PanButton value) => obj.SetValue(PanButtonProperty, value);
        public static PanButton GetPanButton(DependencyObject obj) => (PanButton)obj.GetValue(PanButtonProperty);

        public static readonly DependencyProperty RequireCtrlForWheelZoomProperty =
            DependencyProperty.RegisterAttached(
                "RequireCtrlForWheelZoom", typeof(bool), typeof(Viewport),
                new PropertyMetadata(false, OnAnyDefaultChanged));
        public static void SetRequireCtrlForWheelZoom(DependencyObject obj, bool value) => obj.SetValue(RequireCtrlForWheelZoomProperty, value);
        public static bool GetRequireCtrlForWheelZoom(DependencyObject obj) => (bool)obj.GetValue(RequireCtrlForWheelZoomProperty);

        public static readonly DependencyProperty UseImageCoordinateZoomProperty =
            DependencyProperty.RegisterAttached(
                "UseImageCoordinateZoom", typeof(bool), typeof(Viewport),
                new PropertyMetadata(true, OnAnyDefaultChanged));
        public static void SetUseImageCoordinateZoom(DependencyObject obj, bool value) => obj.SetValue(UseImageCoordinateZoomProperty, value);
        public static bool GetUseImageCoordinateZoom(DependencyObject obj) => (bool)obj.GetValue(UseImageCoordinateZoomProperty);

        public static readonly DependencyProperty WheelPivotProperty =
            DependencyProperty.RegisterAttached(
                "WheelPivot", typeof(ZoomPivotMode), typeof(Viewport),
                new PropertyMetadata(ZoomPivotMode.Mouse, OnAnyDefaultChanged));
        public static void SetWheelPivot(DependencyObject obj, ZoomPivotMode value) => obj.SetValue(WheelPivotProperty, value);
        public static ZoomPivotMode GetWheelPivot(DependencyObject obj) => (ZoomPivotMode)obj.GetValue(WheelPivotProperty);

        public static readonly DependencyProperty ScaleFactorProperty =
            DependencyProperty.RegisterAttached(
                "ScaleFactor", typeof(double), typeof(Viewport),
                new PropertyMetadata(1.1, OnAnyDefaultChanged));
        public static void SetScaleFactor(DependencyObject obj, double value) => obj.SetValue(ScaleFactorProperty, value);
        public static double GetScaleFactor(DependencyObject obj) => (double)obj.GetValue(ScaleFactorProperty);

        public static readonly DependencyProperty MinScaleProperty =
            DependencyProperty.RegisterAttached(
                "MinScale", typeof(double), typeof(Viewport),
                new PropertyMetadata(0.02, OnAnyDefaultChanged));
        public static void SetMinScale(DependencyObject obj, double value) => obj.SetValue(MinScaleProperty, value);
        public static double GetMinScale(DependencyObject obj) => (double)obj.GetValue(MinScaleProperty);

        public static readonly DependencyProperty MaxScaleProperty =
            DependencyProperty.RegisterAttached(
                "MaxScale", typeof(double), typeof(Viewport),
                new PropertyMetadata(40.0, OnAnyDefaultChanged));
        public static void SetMaxScale(DependencyObject obj, double value) => obj.SetValue(MaxScaleProperty, value);
        public static double GetMaxScale(DependencyObject obj) => (double)obj.GetValue(MaxScaleProperty);

        public static readonly DependencyProperty CustomPivotWindowPxProviderProperty =
            DependencyProperty.RegisterAttached(
                "CustomPivotWindowPxProvider", typeof(Func<PxPoint>), typeof(Viewport),
                new PropertyMetadata(null, OnAnyDefaultChanged));
        public static void SetCustomPivotWindowPxProvider(DependencyObject obj, Func<PxPoint>? value) => obj.SetValue(CustomPivotWindowPxProviderProperty, value);
        public static Func<PxPoint>? GetCustomPivotWindowPxProvider(DependencyObject obj) => (Func<PxPoint>?)obj.GetValue(CustomPivotWindowPxProviderProperty);

        // -------- AutoFit ----------
        public static readonly DependencyProperty AutoFitModeProperty =
            DependencyProperty.RegisterAttached(
                "AutoFitMode", typeof(AutoFitMode), typeof(Viewport),
                new PropertyMetadata(AutoFitMode.Always, OnAnyDefaultChanged));
        public static void SetAutoFitMode(DependencyObject obj, AutoFitMode value) => obj.SetValue(AutoFitModeProperty, value);
        public static AutoFitMode GetAutoFitMode(DependencyObject obj) => (AutoFitMode)obj.GetValue(AutoFitModeProperty);

        // -------- Runtime 挂件（Options + Facade 持有处） ----------
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

        private static readonly DependencyProperty RuntimeProperty =
            DependencyProperty.RegisterAttached(
                "Runtime", typeof(RuntimeState), typeof(Viewport),
                new PropertyMetadata(null));
        private static RuntimeState? GetRuntime(DependencyObject obj) => (RuntimeState?)obj.GetValue(RuntimeProperty);
        private static void SetRuntime(DependencyObject obj, RuntimeState? value) => obj.SetValue(RuntimeProperty, value);

        // 统一入口：任意默认 DP 变更即保证装配
        private static void OnAnyDefaultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ImageViewport vp) return;

            var rt = EnsureAttached(vp);

            // 需要主动"推送"的：Source / AutoFitMode
            if (e.Property == SourceProperty)
            {
                rt.Facade?.SetSource(e.NewValue as ImageSource);
            } else if (e.Property == AutoFitModeProperty && rt.Facade is not null)
            {
                rt.Facade.SetAutoFitMode((AutoFitMode)e.NewValue!);
            }
            // 其它诸如 PanButton/ScaleFactor/MinScale/MaxScale/Pivot/RequireCtrl...：
            // 都是被 ZoomHandler 动态读取，不需要此处推送。
        }

        // 没有 Enabled；当任意 Default 属性出现时才装配
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
                // 初始 AutoFitMode 同步一次
                rt.Facade.SetAutoFitMode(GetAutoFitMode(vp));
            }

            vp.Unloaded -= OnViewportUnloaded;
            vp.Unloaded += OnViewportUnloaded;
            return rt;
        }

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