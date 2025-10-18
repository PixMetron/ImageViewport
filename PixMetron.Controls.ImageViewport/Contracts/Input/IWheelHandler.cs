namespace PixMetron.Controls.ImageViewport.Contracts.Input
{
    /// <summary>
    /// 鼠标滚轮事件处理接口。
    /// </summary>
    public interface IWheelHandler
    {
        /// <summary>
        /// 处理鼠标滚轮事件。
        /// </summary>
        /// <param name="sender">发送者（通常是 ImageViewport）。</param>
        /// <param name="p">指针事件信息。</param>
        /// <returns>如果事件被处理则返回 true，否则返回 false。</returns>
        bool OnWheel(object sender, PointerEvent p);
    }
}