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
    /// Image viewport control - responsible only for core infrastructure and state management.
    /// Business logic should be extended through Facade/Handlers/Surfaces.
    /// </summary>
    [TemplatePart(Name = PartHost, Type = typeof(LayeredSurfaceHost))]
    public sealed class ImageViewport : Control
    {
        #region Constants
        internal const string PartHost = "PART_SurfaceHost";

        // Precision constants for internal calculations
        const double ScalePrecision = 1e-9;
        const double DpiScalePrecision = 1e-6;

        /// <summary>
        /// Minimum threshold for pan distance to be considered valid (in pixels).
        /// </summary>
        const double PanThreshold = 0.01;

        /// <summary>
        /// Determines if two double values are close within the specified precision.
        /// </summary>
        static bool AreClose(double v1, double v2, double precision)
            => Math.Abs(v1 - v2) < precision;
        #endregion

        #region Private Fields (Infrastructure Only)
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageViewport"/> class.
        /// </summary>
        public ImageViewport()
        {
            Focusable = true;
            SetValue(ViewportMatrixPropertyKey, _viewportMatrix);
            WeakEventManager<FrameworkElement, RoutedEventArgs>
                .AddHandler(this, nameof(Unloaded), OnControlUnloaded);
        }

        #region Core Dependency Properties

        #region Facade
        /// <summary>
        /// Gets or sets the viewport facade that provides surfaces, services, and input routing.
        /// </summary>
        public IViewportFacade? Facade
        {
            get => (IViewportFacade?)GetValue(FacadeProperty);
            set => SetValue(FacadeProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Facade"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FacadeProperty =
            DependencyProperty.Register(nameof(Facade), typeof(IViewportFacade),
                typeof(ImageViewport),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, _) => ((ImageViewport)d).Rebind()));
        #endregion

        #region Read-Only State Properties

        #region ViewportMatrix
        static readonly DependencyPropertyKey ViewportMatrixPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ViewportMatrix),
                typeof(MatrixTransform), typeof(ImageViewport),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Identifies the <see cref="ViewportMatrix"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ViewportMatrixProperty =
            ViewportMatrixPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the viewport transformation matrix that maps from image coordinates to window coordinates.
        /// </summary>
        public MatrixTransform ViewportMatrix =>
            (MatrixTransform)GetValue(ViewportMatrixProperty);
        #endregion

        static readonly DependencyPropertyKey ImageViewRectPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ImageViewRect),
                typeof(PxRect), typeof(ImageViewport),
                new FrameworkPropertyMetadata(new PxRect(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the <see cref="ImageViewRect"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageViewRectProperty =
            ImageViewRectPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the viewport rectangle in image pixel coordinates.
        /// </summary>
        public PxRect ImageViewRect => (PxRect)GetValue(ImageViewRectProperty);

        static readonly DependencyPropertyKey WindowPixelSizePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(WindowPixelSize),
                typeof(PxSize), typeof(ImageViewport),
                new FrameworkPropertyMetadata(new PxSize(0, 0)));

        /// <summary>
        /// Identifies the <see cref="WindowPixelSize"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WindowPixelSizeProperty =
            WindowPixelSizePropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the current window size in pixels.
        /// </summary>
        public PxSize WindowPixelSize => (PxSize)GetValue(WindowPixelSizeProperty);

        static readonly DependencyPropertyKey ScalePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Scale),
                typeof(double), typeof(ImageViewport),
                new FrameworkPropertyMetadata(1.0));

        /// <summary>
        /// Identifies the <see cref="Scale"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ScaleProperty =
            ScalePropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the current zoom scale factor.
        /// </summary>
        public double Scale => (double)GetValue(ScaleProperty);

        static readonly DependencyPropertyKey DpiScaleXPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(DpiScaleX),
                typeof(double), typeof(ImageViewport),
                new FrameworkPropertyMetadata(1.0));

        /// <summary>
        /// Identifies the <see cref="DpiScaleX"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DpiScaleXProperty =
            DpiScaleXPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the horizontal DPI scale factor.
        /// </summary>
        public double DpiScaleX => (double)GetValue(DpiScaleXProperty);

        static readonly DependencyPropertyKey DpiScaleYPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(DpiScaleY),
                typeof(double), typeof(ImageViewport),
                new FrameworkPropertyMetadata(1.0));

        /// <summary>
        /// Identifies the <see cref="DpiScaleY"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DpiScaleYProperty =
            DpiScaleYPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the vertical DPI scale factor.
        /// </summary>
        public double DpiScaleY => (double)GetValue(DpiScaleYProperty);
        #endregion

        #endregion

        #region Control Lifecycle
        /// <summary>
        /// Called when the template is applied. Retrieves the surface host and binds to the facade.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _host = GetTemplateChild(PartHost) as LayeredSurfaceHost
                    ?? throw new InvalidOperationException("PART_SurfaceHost not found.");
            Rebind();
        }

        /// <summary>
        /// Handles render size changes and updates the viewport service accordingly.
        /// </summary>
        /// <param name="sizeInfo">Information about the size change.</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            _service?.SetWindowSize(new PxSize(
                sizeInfo.NewSize.Width,
                sizeInfo.NewSize.Height));
            _host?.Invalidate();                // Synchronously refresh host to avoid one-frame lag
        }

        /// <summary>
        /// Handles the control unloaded event and performs cleanup.
        /// </summary>
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

        #region Service Binding
        /// <summary>
        /// Rebinds the control to the current facade and viewport service.
        /// </summary>
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

        #region Low-Level API (Thin Wrappers)
        /// <summary>
        /// Zooms at the specified window pixel position as the anchor point.
        /// </summary>
        /// <param name="factor">The zoom factor to apply.</param>
        /// <param name="windowPx">The anchor point in window pixel coordinates.</param>
        public void ZoomAtWindowPx(double factor, PxPoint windowPx)
        {
            if (factor <= 0 || _service is null) return;
            _service.ZoomAtWindowPx(factor, windowPx);
        }

        /// <summary>
        /// Zooms at the specified image pixel position as the anchor point.
        /// Applies compensation panning to keep the anchor point stationary in window coordinates.
        /// </summary>
        /// <param name="factor">The zoom factor to apply.</param>
        /// <param name="imagePx">The anchor point in image pixel coordinates.</param>
        public void ZoomAtImagePx(double factor, PxPoint imagePx)
        {
            if (factor <= 0 || _service is null) return;
            // 1. Position of anchor point in window before zoom
            var anchorWinPxBefore = ImageToWindow(imagePx);

            // 2. Zoom at that window position
            ZoomAtWindowPx(factor, anchorWinPxBefore);

            // 3. Position of anchor point in window after zoom
            var anchorWinPxAfter = ImageToWindow(imagePx);

            // 4. Apply compensation pan (using unified pan threshold)
            var dx = anchorWinPxBefore.X - anchorWinPxAfter.X;
            var dy = anchorWinPxBefore.Y - anchorWinPxAfter.Y;
            if (Math.Abs(dx) > PanThreshold || Math.Abs(dy) > PanThreshold)
            {
                PanWindowPx(dx, dy);
            }
        }

        /// <summary>
        /// Pans the viewport by the specified offset in window pixels.
        /// </summary>
        /// <param name="dx">Horizontal pan offset in pixels.</param>
        /// <param name="dy">Vertical pan offset in pixels.</param>
        public void PanWindowPx(double dx, double dy)
        {
            if (_service is null) return;
            _service.PanWindowPx(dx, dy);
        }

        /// <summary>
        /// Gets the current viewport information, or an empty snapshot if no service is available.
        /// Used internally and for Facade initial reads.
        /// </summary>
        /// <returns>The current viewport information.</returns>
        public ViewportInfo CurrentOrEmpty() => _service?.Current ?? new ViewportInfo();

        /// <summary>
        /// Converts a point from window pixel coordinates to image pixel coordinates.
        /// </summary>
        /// <param name="windowPx">The point in window coordinates.</param>
        /// <returns>The corresponding point in image coordinates.</returns>
        public PxPoint WindowToImage(PxPoint windowPx)
        {
            if (_facade is null || _service is null) return new PxPoint();

            var view = CurrentOrEmpty();
            var tf = _facade.GetTransforms(in view);
            return tf.WindowToImage(windowPx);
        }

        /// <summary>
        /// Converts a point from image pixel coordinates to window pixel coordinates.
        /// </summary>
        /// <param name="imagePx">The point in image coordinates.</param>
        /// <returns>The corresponding point in window coordinates.</returns>
        public PxPoint ImageToWindow(PxPoint imagePx)
        {
            if (_facade is null || _service is null) return new PxPoint();
            var view = CurrentOrEmpty();
            var tf = _facade.GetTransforms(in view);
            return tf.ImageToWindow(imagePx);
        }

        /// <summary>
        /// Converts a rectangle from window pixel coordinates to image pixel coordinates.
        /// </summary>
        /// <param name="windowRect">The rectangle in window coordinates.</param>
        /// <returns>The corresponding rectangle in image coordinates.</returns>
        public PxRect WindowRectToImageRect(PxRect windowRect)
        {
            if (_facade is null || _service is null) return new PxRect();

            var view = CurrentOrEmpty();
            var tf = _facade.GetTransforms(in view);
            return tf.WindowToImage(windowRect);
        }

        /// <summary>
        /// Converts a rectangle from image pixel coordinates to window pixel coordinates.
        /// </summary>
        /// <param name="imageRect">The rectangle in image coordinates.</param>
        /// <returns>The corresponding rectangle in window coordinates.</returns>
        public PxRect ImageRectToWindowRect(PxRect imageRect)
        {
            if (_facade is null || _service is null) return new PxRect();

            var view = CurrentOrEmpty();
            var tf = _facade.GetTransforms(in view);
            return tf.ImageToWindow(imageRect);
        }

        /// <summary>
        /// Fits the specified image rectangle to the viewport by adjusting scale and position.
        /// </summary>
        /// <param name="imageRect">The image rectangle to fit.</param>
        public void FitImageRect(PxRect imageRect)
        {
            if (_service is null) return;

            // Validate rectangle
            if (double.IsNaN(imageRect.X) || double.IsNaN(imageRect.Y)
                || double.IsNaN(imageRect.Width) || double.IsNaN(imageRect.Height)
                || imageRect.Width <= 0 || imageRect.Height <= 0)
            {
                return;
            }

            _service.FitImageRect(imageRect);
        }

        /// <summary>
        /// Invalidates the viewport to trigger a re-render.
        /// </summary>
        public void InvilidateViewport()
        {
            _host?.Invalidate();
        }

        #endregion

        #region Public Events

        #region Event Documentation
        // <summary>
        // These control events are necessary. After receiving viewport change events from IViewportObservable,
        // external subscribers can directly subscribe to ImageViewport events without separately subscribing to Facade.Service events.
        // However, whether ScaleChanged, PanChanged, and DpiScaleChanged events should be retained is still under consideration.
        // These events should not be used by the control itself to avoid circular invocations.
        // </summary>
        #endregion

        /// <summary>
        /// Occurs when the viewport state changes. Note: If implementing auto-fit functionality, do not subscribe to this event to avoid circular calls.
        /// </summary>
        public event EventHandler<ViewportInfo>? ViewportChanged;

        /// <summary>
        /// Occurs when the window size changes. Suitable for implementing auto-fit functionality.
        /// </summary>
        public event EventHandler<WindowPixelSizeChangedEventArgs>? WindowSizeChanged;

        /// <summary>
        /// Occurs when the DPI scale changes. Suitable for implementing auto-fit functionality.
        /// </summary>
        public event EventHandler<DpiScaleChangedEventArgs>? DpiScaleChanged;

        /// <summary>
        /// Occurs when the zoom scale changes.
        /// </summary>
        public event EventHandler<ScaleChangedEventArgs>? ScaleChanged;

        /// <summary>
        /// Occurs when the viewport is panned.
        /// </summary>
        public event EventHandler<PanChangedEventArgs>? PanChanged;

        /// <summary>
        /// Occurs when a context menu is requested (typically right-click).
        /// </summary>
        public event EventHandler<PointerEvent>? ContextMenuRequested;
        #endregion

        #region State Synchronization
        /// <summary>
        /// Handles viewport change notifications from the service, marshalling to the UI thread if necessary.
        /// </summary>
        void OnViewportChangedInternal(object? sender, ViewportInfo e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => ApplyViewportInfo(e));
                return;
            }
            ApplyViewportInfo(e);
        }

        /// <summary>
        /// Applies viewport information to the control's dependency properties and raises change events.
        /// </summary>
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

            // Critical fix: Trigger rendering refresh
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

        #region Input Routing
        /// <summary>
        /// Gets the effective input router from the facade, or a no-op router if none is available.
        /// </summary>
        IInputRouter GetEffectiveRouter()
        {
            return _facade?.InputRouter ?? (_defaultRouter ??= NoopRouter);
        }

        /// <summary>
        /// A no-operation input router that handles no events.
        /// </summary>
        private sealed class NoopInputRouter : IInputRouter
        {
            public bool OnWheel(object s, PointerEvent p) => false;
            public bool OnMove(object s, PointerEvent p) => false;
            public bool OnMouseDown(object s, PointerEvent p) => false;
            public bool OnMouseUp(object s, PointerEvent p) => false;
        }
        #endregion

        #region Input Handling

        /// <summary>
        /// Builds a pointer event from mouse event arguments and position.
        /// </summary>
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
                // Fallback: manual calculation (theoretically should not reach here)
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

        /// <summary>
        /// Handles the mouse wheel event.
        /// </summary>
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

        /// <summary>
        /// Handles the mouse move event.
        /// </summary>
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
        /// Handles the left mouse button down event.
        /// </summary>
        /// <param name="e">The event data.</param>
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
        /// Handles the left mouse button up event.
        /// </summary>
        /// <param name="e">The event data.</param>
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

        #region Right-Click Special Handling (Context Menu)

        const double ContextClickMoveThreshold = 4.0;   // pixels
        static readonly TimeSpan ContextClickTimeThreshold = TimeSpan.FromMilliseconds(600);

        Point _rightDownPos;
        DateTime _rightDownTime;
        bool _rightDown;

        /// <summary>
        /// Handles the right mouse button down event.
        /// </summary>
        /// <param name="e">The event data.</param>
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
        /// Handles the right mouse button up event and context menu display logic.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            if (_host is null) return;

            var p = e.GetPosition(_host);
            var pe = BuildPointerEvent(e, p);
            var router = GetEffectiveRouter();
            bool handled = router.OnRightUp(this, pe) || router.OnMouseUp(this, pe);

            // If router declares context menu suppression, skip menu logic
            if (!handled && !pe.SuppressContextMenu)
            {
                // Retain "click threshold" check to distinguish click from drag
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
        /// Handles the general mouse button down event (supports all three buttons).
        /// </summary>
        /// <param name="e">The event data.</param>
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
        /// Handles the general mouse button up event (supports all three buttons).
        /// </summary>
        /// <param name="e">The event data.</param>
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