using System.Windows.Input;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Contracts.Input;
using PixMetron.Controls.ImageViewport.Handlers.Contracts;

namespace PixMetron.Controls.ImageViewport.Handlers.Routers
{
    /// <summary>
    /// 组合式处理器：拖拽平移 + 滚轮缩放。
    /// 内部组合 PanHandler 和 ZoomHandler。
    /// </summary>
    public sealed class PanZoomHandler : IInputRouter
    {
        private readonly PanHandler _panHandler;
        private readonly ZoomHandler _zoomHandler;
        private readonly IPanZoomOptions _options;

        public PanZoomHandler(IPanZoomOptions? options = null)
        {
            _options = options ?? new PanZoomOptions();

            // 初始化子处理器
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

        public bool OnWheel(object sender, PointerEvent p)
        {
            // 检查 Ctrl 修饰键要求
            if (_options.RequireCtrlForWheelZoom && !p.Modifiers.HasFlag(ModifierKeys.Control))
                return false;

            return _zoomHandler.OnWheel(sender, p);
        }

        public bool OnMouseDown(object sender, PointerEvent p)
        {
            // 检查 Shift 修饰键（阻止拖拽）
            if (p.Modifiers.HasFlag(ModifierKeys.Shift))
                return false;

            return _panHandler.OnMouseDown(sender, p);
        }

        public bool OnMove(object sender, PointerEvent p)
        {
            // 优先处理平移，然后更新缩放锚点
            var handled = _panHandler.OnMove(sender, p);
            _zoomHandler.OnMove(sender, p); // 不拦截，仅更新位置
            return handled;
        }

        public bool OnMouseUp(object sender, PointerEvent p)
        {
            return _panHandler.OnMouseUp(sender, p);
        }

        // IInputRouter 的默认实现（已由接口提供）
        // OnLeftDown, OnLeftUp, OnRightDown, OnRightUp, OnMiddleDown, OnMiddleUp
        // 都使用默认实现（返回 false）
    }
}