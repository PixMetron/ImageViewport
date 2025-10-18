using System;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Contracts.Input;

namespace PixMetron.Controls.ImageViewport.Handlers.Routers
{
    /// <summary>
    /// 纯滚轮缩放处理器。
    /// 支持不同的缩放锚点模式和缩放范围限制。
    /// </summary>
    public class ZoomHandler : IWheelHandler, IMoveHandler
    {
        // 静态属性(当对应的 Provider 为 null 时使用)
        public double ScaleFactor { get; set; } = 1.1;
        public double MinScale { get; set; } = 0.01;
        public double MaxScale { get; set; } = 100.0;
        public ZoomPivotMode PivotMode { get; set; } = ZoomPivotMode.Mouse;
        public PxPoint CustomPivot { get; set; }
        public bool UseImageCoordinate { get; set; } = true;

        // 动态配置委托(优先级高于静态属性)
        public Func<double>? ScaleFactorProvider { get; set; }
        public Func<double>? MinScaleProvider { get; set; }
        public Func<double>? MaxScaleProvider { get; set; }
        public Func<ZoomPivotMode>? PivotModeProvider { get; set; }
        public Func<PxPoint>? CustomPivotProvider { get; set; }
        public Func<bool>? UseImageCoordinateProvider { get; set; }

        private PxPoint _lastMousePosWindow;
        private PxPoint _lastMousePosImage;

        public virtual bool OnWheel(object sender, PointerEvent p)
        {
            if (sender is not ImageViewport vp) return false;

            // 动态获取配置
            var scaleFactor = ScaleFactorProvider?.Invoke() ?? ScaleFactor;
            var minScale = MinScaleProvider?.Invoke() ?? MinScale;
            var maxScale = MaxScaleProvider?.Invoke() ?? MaxScale;
            var useImageCoordinate = UseImageCoordinateProvider?.Invoke() ?? UseImageCoordinate;

            // 计算缩放因子
            double factor = p.WheelDelta > 0 ? scaleFactor : 1.0 / scaleFactor;

            // 获取当前缩放并应用范围限制
            var current = vp.Scale <= 0 ? 1.0 : vp.Scale;
            var desired = current * factor;

            var minS = minScale > 0 ? minScale : 1e-6;
            var maxS = maxScale > minS ? maxScale : minS;
            desired = Math.Max(minS, Math.Min(maxS, desired));

            var actualFactor = desired / current;
            if (Math.Abs(actualFactor - 1.0) < 1e-9) return false; // 无变化

            // 根据模式选择使用窗口坐标或图像坐标
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

        public virtual bool OnMove(object sender, PointerEvent p)
        {
            // 追踪鼠标位置
            _lastMousePosWindow = p.WindowPx;
            _lastMousePosImage = p.ImagePx;
            return false; // 不拦截移动事件
        }
    }
}