namespace PixMetron.Controls.ImageViewport.Contracts.Input
{
    /// <summary>
    /// 鼠标按键事件处理接口。
    /// </summary>
    public interface IMouseButtonHandler
    {
        /// <summary>
        /// 处理鼠标按下事件（任意按键）。
        /// </summary>
        bool OnMouseDown(object sender, PointerEvent p);

        /// <summary>
        /// 处理鼠标释放事件（任意按键）。
        /// </summary>
        bool OnMouseUp(object sender, PointerEvent p);
    }
}