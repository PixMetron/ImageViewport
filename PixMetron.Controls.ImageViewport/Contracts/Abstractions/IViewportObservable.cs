namespace PixMetron.Controls.ImageViewport.Contracts.Abstractions
{
    /// <summary>
    /// 可选的视口事件源接口（若服务实现该接口，则可推送视口变化事件）。
    /// </summary>
    public interface IViewportObservable
    {
        /// <summary>
        /// 视口状态发生变化时触发的事件。
        /// </summary>
        event EventHandler<ViewportInfo> ViewportChanged;
    }
}