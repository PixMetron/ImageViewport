namespace PixMetron.Controls.ImageViewport.Contracts.Input
{
    /// <summary>
    /// 鼠标移动事件处理接口。
    /// </summary>
    public interface IMoveHandler
    {
        /// <summary>
        /// 处理鼠标移动事件。
        /// </summary>
        bool OnMove(object sender, PointerEvent p);
    }
}