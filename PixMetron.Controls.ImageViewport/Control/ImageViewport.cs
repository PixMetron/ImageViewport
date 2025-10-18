using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Contracts.Abstractions.Events;
using PixMetron.Controls.ImageViewport.Contracts.Facade;
using PixMetron.Controls.ImageViewport.Contracts.Input;
using PixMetron.Controls.ImageViewport.Runtime.Display;
using PixMetron.Controls.ImageViewport.Runtime.Services;

namespace PixMetron.Controls.ImageViewport
{
    /// <summary>
    /// 图像视口控件 - 仅负责核心基础设施和状态管理。
    /// 业务逻辑应通过 Facade/Handlers/Surfaces 扩展。
    /// </summary>
    [TemplatePart(Name = PartHost, Type = typeof(LayeredSurfaceHost))]
    public sealed class ImageViewport : Control
    {
        #region 常量
        internal const string PartHost = "PART_SurfaceHost";

        // 保留：精度常量（用于内部计算）
        const double ScalePrecision = 1e-9;
        const double DpiScalePrecision = 1e-6;

        /// <summary>
        /// 平移距离的最小阈值（用于判断平移是否有效，单位：像素）。
        /// </summary>
        const double PanThreshold = 0.01;

        static bool AreClose(double v1, double v2, double precision)
            => Math.Abs(v1 - v2) < precision;
        #endregion

        #region 私有字段（仅基础设施）
        LayeredSurfaceHost? _host;
        IDisposable? _dpiSubscription;

        IViewportFacade? _facade;
        IViewportService? _service;
        IInputRouter? _defaultRouter;
        static readonly IInputRouter NoopRouter = new NoopInputRouter();

        readonly MatrixTransform _viewportMatrix = new(Matrix.Identity);
        #endregion

        static ImageViewport()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ImageViewport),
                new FrameworkPropertyMetadata(typeof(ImageViewport)));
        }

        public ImageViewport()
        {
            Focusable = true;
            SetValue(ViewportMatrixPropertyKey, _viewportMatrix);
            WeakEventManager<FrameworkElement, RoutedEventArgs>
                .AddHandler(this, nameof(Unloaded), OnControlUnloaded);
        }

        #region 核心依赖属性

        #region Facade
        public IViewportFacade? Facade
        {
            get => (IViewportFacade?)GetValue(FacadeProperty);
            set => SetValue(FacadeProperty, value);
        }

        public static readonly DependencyProperty FacadeProperty =
            DependencyProperty.Register(nameof(Facade), typeof(IViewportFacade),
                typeof(ImageViewport),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, _) => ((ImageViewport)d).Rebind()));
        #endregion

        #region 只读状态属性

        #region ViewportMatrix
        static readonly DependencyPropertyKey ViewportMatrixPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ViewportMatrix),
                typeof(MatrixTransform), typeof(ImageViewport),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ViewportMatrixProperty =
            ViewportMatrixPropertyKey.DependencyProperty;

        public MatrixTransform ViewportMatrix =>
            (MatrixTransform)GetValue(ViewportMatrixProperty);
        #endregion

        static readonly DependencyPropertyKey ImageViewRectPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ImageViewRect),
                typeof(PxRect), typeof(ImageViewport),
                new FrameworkPropertyMetadata(new PxRect(0, 0, 0, 0)));

        public static readonly DependencyProperty ImageViewRectProperty =
            ImageViewRectPropertyKey.DependencyProperty;

        public PxRect ImageViewRect => (PxRect)GetValue(ImageViewRectProperty);

        static readonly DependencyPropertyKey WindowPixelSizePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(WindowPixelSize),
                typeof(PxSize), typeof(ImageViewport),
                new FrameworkPropertyMetadata(new PxSize(0, 0)));

        public static readonly DependencyProperty WindowPixelSizeProperty =
            WindowPixelSizePropertyKey.DependencyProperty;

        public PxSize WindowPixelSize => (PxSize)GetValue(WindowPixelSizeProperty);

        static readonly DependencyPropertyKey ScalePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Scale),
                typeof(double), typeof(ImageViewport),
                new FrameworkPropertyMetadata(1.0));

        public static readonly DependencyProperty ScaleProperty =
            ScalePropertyKey.DependencyProperty;

        public double Scale => (double)GetValue(ScaleProperty);

        static readonly DependencyPropertyKey DpiScaleXPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(DpiScaleX),
                typeof(double), typeof(ImageViewport),
                new FrameworkPropertyMetadata(1.0));

        public static readonly DependencyProperty DpiScaleXProperty =
            DpiScaleXPropertyKey.DependencyProperty;

        public double DpiScaleX => (double)GetValue(DpiScaleXProperty);

        static readonly DependencyPropertyKey DpiScaleYPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(DpiScaleY),
                typeof(double), typeof(ImageViewport),
                new FrameworkPropertyMetadata(1.0));

        public static readonly DependencyProperty DpiScaleYProperty =
            DpiScaleYPropertyKey.DependencyProperty;

        public double DpiScaleY => (double)GetValue(DpiScaleYProperty);
        #endregion

        #endregion

        #region 控件生命周期
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _host = GetTemplateChild(PartHost) as LayeredSurfaceHost
                    ?? throw new InvalidOperationException("PART_SurfaceHost not found.");
            Rebind();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            _service?.SetWindowSize(new PxSize(
                sizeInfo.NewSize.Width,
                sizeInfo.NewSize.Height));
            _host?.Invalidate();                // 同步刷新 Host，避免一帧滞后
        }

        void OnControlUnloaded(object? sender, RoutedEventArgs e)
        {
            _dpiSubscription?.Dispose();
            _dpiSubscription = null;

            if (_service is IViewportObservable obs)
            {
                obs.ViewportChanged -= OnViewportChangedInternal;
            }
        }
        #endregion

        #region 服务绑定
        void Rebind()
        {
            if (_host is null) return;

            if (_service is IViewportObservable oldObs)
            {
                oldObs.ViewportChanged -= OnViewportChangedInternal;
            }
            _dpiSubscription?.Dispose();

            _facade = Facade ?? throw new InvalidOperationException("Facade is required.");
            _service = _facade.Service ?? new BuiltInViewportService();
            if (_service is IViewportObservable newObs)
            {
                newObs.ViewportChanged += OnViewportChangedInternal;
            }

            _dpiSubscription = DpiObserver.Attach(this, _service);
            _host.Bind(_facade);
            _service.SetWindowSize(new PxSize(ActualWidth, ActualHeight));

            InvalidateVisual();
        }
        #endregion

        #region 底层 API（薄封装）
        /// <summary>
        /// 以窗口像素为锚点进行缩放。
        /// </summary>
        public void ZoomAtWindowPx(double factor, PxPoint windowPx)
        {
            if (factor <= 0 || _service is null) return;
            _service.ZoomAtWindowPx(factor, windowPx);
        }

        /// <summary>
        /// 以图像像素为锚点进行缩放。
        /// </summary>
        public void ZoomAtImagePx(double factor, PxPoint imagePx)
        {
            if (factor <= 0 || _service is null) return;
            // 1. 缩放前，锚点在窗口中的位置
            var anchorWinPxBefore = ImageToWindow(imagePx);

            // 2. 以该窗口位置为锚点缩放
            ZoomAtWindowPx(factor, anchorWinPxBefore);

            // 3. 缩放后，锚点在窗口中的新位置
            var anchorWinPxAfter = ImageToWindow(imagePx);

            // 4. 做补偿平移（使用统一的平移阈值）
            var dx = anchorWinPxBefore.X - anchorWinPxAfter.X;
            var dy = anchorWinPxBefore.Y - anchorWinPxAfter.Y;
            if (Math.Abs(dx) > PanThreshold || Math.Abs(dy) > PanThreshold)
            {
                PanWindowPx(dx, dy);
            }
        }

        /// <summary>
        /// 以窗口像素为单位进行平移。
        /// </summary>
        public void PanWindowPx(double dx, double dy)
        {
            if (_service is null) return;
            _service.PanWindowPx(dx, dy);
        }

        // 只读属性用于内部/Facade初始读取
        public ViewportInfo CurrentOrEmpty() => _service?.Current ?? new ViewportInfo();

        /// <summary>
        /// 窗口像素→图像像素坐标。
        /// </summary>
        public PxPoint WindowToImage(PxPoint windowPx)
        {
            if (_facade is null || _service is null) return new PxPoint();

            var view = CurrentOrEmpty();
            var tf = _facade.GetTransforms(in view);
            return tf.WindowToImage(windowPx);
        }

        /// <summary>
        /// 图像像素→窗口像素坐标。
        /// </summary>
        public PxPoint ImageToWindow(PxPoint imagePx)
        {
            if (_facade is null || _service is null) return new PxPoint();
            var view = CurrentOrEmpty();
            var tf = _facade.GetTransforms(in view);
            return tf.ImageToWindow(imagePx);
        }

        /// <summary>
        /// 窗口矩形→图像像素矩形。
        /// </summary>
        public PxRect WindowRectToImageRect(PxRect windowRect)
        {
            if (_facade is null || _service is null) return new PxRect();

            var view = CurrentOrEmpty();
            var tf = _facade.GetTransforms(in view);
            return tf.WindowToImage(windowRect);
        }

        /// <summary>
        /// 图像像素矩形→窗口矩形。
        /// </summary>
        public PxRect ImageRectToWindowRect(PxRect imageRect)
        {
            if (_facade is null || _service is null) return new PxRect();

            var view = CurrentOrEmpty();
            var tf = _facade.GetTransforms(in view);
            return tf.ImageToWindow(imageRect);
        }

        /// <summary>
        /// 将指定的图像矩形适配到视口。
        /// </summary>
        public void FitImageRect(PxRect imageRect)
        {
            if (_service is null) return;

            // 验证矩形有效性
            if (double.IsNaN(imageRect.X) || double.IsNaN(imageRect.Y)
                || double.IsNaN(imageRect.Width) || double.IsNaN(imageRect.Height)
                || imageRect.Width <= 0 || imageRect.Height <= 0)
            {
                return;
            }

            _service.FitImageRect(imageRect);
        }

        public void InvilidateViewport()
        {
            _host?.Invalidate();
        }

        #endregion

        #region 公开事件

        #region 事件说明
        /// <summary>
        /// 控件的这些事件是必要的，从 IViewportObservable 拿到视口变化事件后，
        /// 以便外部订阅视口状态变化能够直接订阅 ImageViewport 的事件，而不需要额外订阅 Facade.Service 的事件。
        /// 但是，ScaleChanged 、PanChanged 和 DpiScaleChanged 等事件是否需要保留，仍有待商榷。
        /// 这些事件不应该被控件本身使用，以避免循环调用。
        /// </summary>
        #endregion

        /// 视口变化事件，注意，如果业务要实现图像自动适配视口功能，不能订阅此事件以避免循环调用。
        public event EventHandler<ViewportInfo>? ViewportChanged;

        /// <summary>
        /// ·窗口和 DPI 变化事件。如果业务要实现图像自动适配视口功能，可以订阅这些事件。
        /// </summary>
        public event EventHandler<WindowPixelSizeChangedEventArgs>? WindowSizeChanged;
        public event EventHandler<DpiScaleChangedEventArgs>? DpiScaleChanged;

        public event EventHandler<ScaleChangedEventArgs>? ScaleChanged;
        public event EventHandler<PanChangedEventArgs>? PanChanged;
        public event EventHandler<PointerEvent>? ContextMenuRequested;
        #endregion

        #region 状态同步
        void OnViewportChangedInternal(object? sender, ViewportInfo e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => ApplyViewportInfo(e));
                return;
            }
            ApplyViewportInfo(e);
        }

        void ApplyViewportInfo(ViewportInfo e)
        {
            var oldScale = Scale;
            var oldImageViewRect = ImageViewRect;
            var oldWindowPixelSize = WindowPixelSize;
            var oldDpiScaleX = DpiScaleX;
            var oldDpiScaleY = DpiScaleY;

            SetValue(ScalePropertyKey, e.Scale);
            SetValue(ImageViewRectPropertyKey, e.ViewportRectInImage);
            SetValue(WindowPixelSizePropertyKey, e.WindowPixelSize);
            SetValue(DpiScaleXPropertyKey, e.DpiScaleX);
            SetValue(DpiScaleYPropertyKey, e.DpiScaleY);

            var s = e.Scale;
            var tl = e.ViewportRectInImage.TopLeft;
            _viewportMatrix.Matrix = new Matrix(s, 0, 0, s, -tl.X * s, -tl.Y * s);

            // 关键修复:触发渲染刷新
            _host?.Invalidate();

            if (!AreClose(oldScale, e.Scale, ScalePrecision))
            {
                ScaleChanged?.Invoke(this, new ScaleChangedEventArgs(e.Scale));
            }

            if (!AreClose(oldImageViewRect.X, e.ViewportRectInImage.X, PanThreshold)
                || !AreClose(oldImageViewRect.Y, e.ViewportRectInImage.Y, PanThreshold))
            {
                PanChanged?.Invoke(this, new PanChangedEventArgs(e.ViewportRectInImage.TopLeft));
            }

            if (!AreClose(oldDpiScaleX, e.DpiScaleX, DpiScalePrecision)
                || !AreClose(oldDpiScaleY, e.DpiScaleY, DpiScalePrecision))
            {
                DpiScaleChanged?.Invoke(this, new DpiScaleChangedEventArgs(e.DpiScaleX, e.DpiScaleY));
            }

            if (oldWindowPixelSize != e.WindowPixelSize)
            {
                WindowSizeChanged?.Invoke(this, new WindowPixelSizeChangedEventArgs(e.WindowPixelSize));
            }

            ViewportChanged?.Invoke(this, e);
        }
        #endregion

        #region 输入路由（简化）
        IInputRouter GetEffectiveRouter()
        {
            return _facade?.InputRouter ?? (_defaultRouter ??= NoopRouter);
        }

        private sealed class NoopInputRouter : IInputRouter
        {
            public bool OnWheel(object s, PointerEvent p) => false;
            public bool OnMove(object s, PointerEvent p) => false;
            public bool OnMouseDown(object s, PointerEvent p) => false;
            public bool OnMouseUp(object s, PointerEvent p) => false;
        }
        #endregion

        #region 输入处理（简化）

        PointerEvent BuildPointerEvent(MouseEventArgs e, Point winPt, double wheelDelta = 0, int clickCount = 0)
        {
            if (_service is null) return new PointerEvent();

            PxPoint pxWin = new(winPt.X, winPt.Y);
            PxPoint pxImg;

            if (_facade is not null)
            {
                var view = CurrentOrEmpty();
                var tf = _facade.GetTransforms(in view);
                pxImg = tf.WindowToImage(pxWin);
            } else
            {
                // 兜底：仍可手算（理论上不会走到这里）
                var view = _service.Snapshot();
                pxImg = new PxPoint(view.ViewportRectInImage.X + pxWin.X / view.Scale,
                                    view.ViewportRectInImage.Y + pxWin.Y / view.Scale);
            }

            var mods = ModifierKeys.None;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                mods |= ModifierKeys.Control;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                mods |= ModifierKeys.Shift;
            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                mods |= ModifierKeys.Alt;

            return new PointerEvent
            {
                Timestamp = DateTime.UtcNow,
                WindowPx = pxWin,
                ImagePx = pxImg,
                WheelDelta = wheelDelta,
                ClickCount = clickCount,
                Modifiers = mods,
                CurrentLeftPressed = e.LeftButton == MouseButtonState.Pressed,
                CurrentRightPressed = e.RightButton == MouseButtonState.Pressed,
                CurrentMiddlePressed = e.MiddleButton == MouseButtonState.Pressed
            };
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_host is null) return;

            var p = e.GetPosition(_host);
            var pe = BuildPointerEvent(e, p, wheelDelta: e.Delta);

            if (GetEffectiveRouter().OnWheel(this, pe))
            {
                _host.Invalidate();
                e.Handled = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_host is null) return;

            var p = e.GetPosition(_host);
            var pe = BuildPointerEvent(e, p);

            if (GetEffectiveRouter().OnMove(this, pe))
            {
                _host.Invalidate();
                e.Handled = true;
            }
        }

        /// <summary>
        /// 鼠标左键按下事件处理。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (_host is null)
            {
                return;
            }

            Focus();

            var p = e.GetPosition(_host);

            var pe = BuildPointerEvent(e, p, clickCount: e.ClickCount);
            var router = GetEffectiveRouter();
            bool handled = router.OnLeftDown(this, pe) || router.OnMouseDown(this, pe);
            if (handled)
            {
                _host.Invalidate();
                e.Handled = true;
            }
        }

        /// <summary>
        /// 鼠标左键释放事件处理。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (_host is null)
            {
                return;
            }

            var p = e.GetPosition(_host);

            var pe = BuildPointerEvent(e, p);
            var router = GetEffectiveRouter();
            bool handled = router.OnLeftUp(this, pe) || router.OnMouseUp(this, pe);
            if (handled)
            {
                _host.Invalidate();
                e.Handled = true;
            }
        }

        #region 右键特殊处理（右键单击弹出菜单）

        const double ContextClickMoveThreshold = 4.0;   // 像素
        static readonly TimeSpan ContextClickTimeThreshold = TimeSpan.FromMilliseconds(600);

        Point _rightDownPos;
        DateTime _rightDownTime;
        bool _rightDown;

        /// <summary>
        /// 鼠标右键按下事件处理。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            if (_host is null) return;

            _rightDown = true;
            _rightDownPos = e.GetPosition(_host);
            _rightDownTime = DateTime.UtcNow;

            var p = _rightDownPos;
            var pe = BuildPointerEvent(e, p, clickCount: e.ClickCount);
            var router = GetEffectiveRouter();
            bool handled = router.OnRightDown(this, pe) || router.OnMouseDown(this, pe);
            if (handled) { _host.Invalidate(); e.Handled = true; }
        }

        /// <summary>
        /// 鼠标右键释放事件处理。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            if (_host is null) return;

            var p = e.GetPosition(_host);
            var pe = BuildPointerEvent(e, p);
            var router = GetEffectiveRouter();
            bool handled = router.OnRightUp(this, pe) || router.OnMouseUp(this, pe);

            // 若路由器声明禁止菜单，则直接跳过菜单逻辑
            if (!handled && !pe.SuppressContextMenu)
            {
                // 保留"点击阈值"判断以区分点击/拖拽
                var moved = p - _rightDownPos;
                var movedLen = Math.Sqrt(moved.X * moved.X + moved.Y * moved.Y);
                var dur = DateTime.UtcNow - _rightDownTime;

                if (_rightDown &&
                    movedLen <= ContextClickMoveThreshold &&
                    dur <= ContextClickTimeThreshold)
                {
                    ContextMenuRequested?.Invoke(this, pe);
                    var menu = _facade?.ContextMenu?.BuildContextMenu(pe);
                    if (menu != null)
                    {
                        menu.PlacementTarget = this;
                        menu.IsOpen = true;
                        handled = true;
                    }
                }
            }

            _rightDown = false;

            if (handled)
            {
                _host?.Invalidate();
                e.Handled = true;
            }
        }

        #endregion

        /// <summary>
        /// 鼠标按下事件处理（支持三键）。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (_host is null)
            {
                return;
            }

            var router = GetEffectiveRouter();
            var p = e.GetPosition(_host);
            var pe = BuildPointerEvent(e, p, clickCount: e.ClickCount);
            bool handled = router.OnMouseDown(this, pe);
            if (handled) { _host.Invalidate(); e.Handled = true; }
        }

        /// <summary>
        /// 鼠标释放事件处理（支持三键）。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (_host is null)
            {
                return;
            }

            var p = e.GetPosition(_host);
            var pe = BuildPointerEvent(e, p);
            var router = GetEffectiveRouter();
            bool handled = router.OnMouseUp(this, pe);
            if (handled) { _host.Invalidate(); e.Handled = true; }
        }

        #endregion
    }
}