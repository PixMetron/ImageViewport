using System;
using System.Windows;
using System.Windows.Input;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Contracts.Input;
using PixMetron.Controls.ImageViewport.Handlers.Contracts;

namespace PixMetron.Controls.ImageViewport.Handlers.Routers
{
    /// <summary>
    /// 纯拖拽平移处理器。
    /// 支持配置拖拽按键（左键/中键/右键）。
    /// </summary>
    public class PanHandler : IMouseButtonHandler, IMoveHandler
    {
        /// <summary>
        /// 静态配置的拖拽按键（当 PanButtonProvider 为 null 时使用）。
        /// </summary>
        public PanButton PanButton { get; set; } = PanButton.Left;

        /// <summary>
        /// 动态获取 PanButton 的委托（优先级高于 PanButton 属性）。
        /// </summary>
        public Func<PanButton>? PanButtonProvider { get; set; }

        // 业务状态：是否正在拖拽
        private bool _isDragging;
        private PxPoint _lastWindowPos;

        public bool OnMouseDown(object sender, PointerEvent p)
        {
            if (!IsPanButtonPressed(p))
                return false;

            _isDragging = true;
            _lastWindowPos = p.WindowPx;

            // 如果配置的拖拽键是 Right，就禁止弹菜单
            var button = PanButtonProvider?.Invoke() ?? PanButton;
            if (button == PanButton.Right)
            {
                p.SuppressContextMenu = true;
            }

            if (sender is IInputElement el)
            {
                el.CaptureMouse();
            }

            return true;
        }

        public bool OnMove(object sender, PointerEvent p)
        {
            if (!_isDragging)
                return false;

            // 如果配置的拖拽键是 Right，就禁止弹菜单
            var button = PanButtonProvider?.Invoke() ?? PanButton;
            if (button == PanButton.Right)
            {
                p.SuppressContextMenu = true;
            }

            // 检查物理按键状态（防止异步问题：如鼠标移出窗口后释放）
            if (!IsPanButtonPressed(p))
            {
                _isDragging = false;

                if (sender is IInputElement el && Mouse.Captured == el)
                {
                    el.ReleaseMouseCapture(); // 释放
                }

                return false;
            }

            if (sender is not ImageViewport vp)
                return false;

            var dx = p.WindowPx.X - _lastWindowPos.X;
            var dy = p.WindowPx.Y - _lastWindowPos.Y;

            if (Math.Abs(dx) > double.Epsilon || Math.Abs(dy) > double.Epsilon)
            {
                vp.PanWindowPx(dx, dy);
                _lastWindowPos = p.WindowPx;
                return true;
            }

            return false;
        }

        public bool OnMouseUp(object sender, PointerEvent p)
        {
            if (!_isDragging)
                return false;

            // 如果配置的拖拽键是 Right，就禁止弹菜单
            var button = PanButtonProvider?.Invoke() ?? PanButton;
            if (button == PanButton.Right)
            {
                p.SuppressContextMenu = true;
            }

            _isDragging = false;
            if (sender is IInputElement el && Mouse.Captured == el)
            {
                el.ReleaseMouseCapture(); // 释放
            }

            return true;
        }

        /// <summary>
        /// 检查配置的拖拽按键是否被按下（物理状态）。
        /// </summary>
        private bool IsPanButtonPressed(PointerEvent p)
        {
            // 优先使用委托,动态获取最新配置
            var button = PanButtonProvider?.Invoke() ?? PanButton;

            return button switch
            {
                PanButton.Left => p.CurrentLeftPressed,
                PanButton.Middle => p.CurrentMiddlePressed,
                PanButton.Right => p.CurrentRightPressed,
                _ => false
            };
        }
    }
}