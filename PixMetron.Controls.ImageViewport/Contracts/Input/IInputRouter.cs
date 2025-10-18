namespace PixMetron.Controls.ImageViewport.Contracts.Input
{
    /// <summary>
    /// 完整的输入路由器接口，组合所有输入处理能力。
    /// 用于 Facade 的输入分发。
    /// </summary>
    public interface IInputRouter : IWheelHandler, IMoveHandler, IMouseButtonHandler
    {
        /// <summary>左键按下事件。</summary>
        bool OnLeftDown(object sender, PointerEvent p) => false;

        /// <summary>左键释放事件。</summary>
        bool OnLeftUp(object sender, PointerEvent p) => false;

        /// <summary>右键按下事件。</summary>
        bool OnRightDown(object sender, PointerEvent p) => false;

        /// <summary>右键释放事件。</summary>
        bool OnRightUp(object sender, PointerEvent p) => false;

        /// <summary>中键按下事件。</summary>
        bool OnMiddleDown(object sender, PointerEvent p) => false;

        /// <summary>中键释放事件。</summary>
        bool OnMiddleUp(object sender, PointerEvent p) => false;
    }
}