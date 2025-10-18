using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Contracts.Input;
using PixMetron.Controls.ImageViewport.Handlers.Contracts;

namespace PixMetron.Controls.ImageViewport.Handlers.Routers
{
    /// <summary>
    /// 自动适配处理器(仅负责状态管理和策略判断)。
    /// </summary>
    public sealed class AutoFitHandler : IWindowSizeHandler
    {
        /// <summary>
        /// 自动适配模式。
        /// </summary>
        public AutoFitMode Mode { get; set; } = AutoFitMode.Disabled;

        private PxRect? _cachedRect;
        private PxSize _lastWindowSize;

        /// <summary>
        /// 设置要适配的图像矩形(仅缓存)。
        /// </summary>
        public void SetCachedRect(PxRect rect)
        {
            _cachedRect = rect;
        }

        /// <summary>
        /// 获取当前缓存的适配矩形。
        /// </summary>
        public PxRect? GetCachedRect() => _cachedRect;

        /// <summary>
        /// 清除缓存的矩形。
        /// </summary>
        public void ClearCache()
        {
            _cachedRect = null;
        }

        /// <summary>
        /// 窗口尺寸变化时的处理(IWindowSizeHandler 实现)。
        /// </summary>
        /// <param name="sender">发送者(通常是 ImageViewport 控件)。</param>
        /// <param name="newSize">新的窗口尺寸。</param>
        /// <returns>是否处理了该事件。</returns>
        public bool OnWindowSizeChanged(object sender, PxSize newSize)
        {
            if (!_cachedRect.HasValue || Mode == AutoFitMode.Disabled)
            {
                _lastWindowSize = newSize;
                return false;
            }

            bool shouldFit = Mode switch
            {
                AutoFitMode.Always => true,
                AutoFitMode.OnWindowGrow => newSize.Width > _lastWindowSize.Width ||
                                           newSize.Height > _lastWindowSize.Height,
                AutoFitMode.OnWindowShrink => newSize.Width < _lastWindowSize.Width ||
                                              newSize.Height < _lastWindowSize.Height,
                _ => false
            };

            _lastWindowSize = newSize;

            // 返回是否需要重新适配
            return shouldFit;
        }

        /// <summary>
        /// 判断窗口尺寸变化时是否应该重新适配。
        /// </summary>
        /// <param name="newSize">新的窗口尺寸。</param>
        /// <param name="rectToFit">如果需要适配,返回要适配的矩形;否则返回 null。</param>
        /// <returns>是否应该执行适配。</returns>
        public bool ShouldRefitOnWindowSizeChange(PxSize newSize, out PxRect? rectToFit)
        {
            rectToFit = null;

            bool shouldFit = OnWindowSizeChanged(this, newSize);

            if (shouldFit && _cachedRect.HasValue)
            {
                rectToFit = _cachedRect.Value;
                return true;
            }

            return false;
        }
    }
}