namespace PixMetron.Controls.ImageViewport.Contracts.Abstractions
{
    public interface IViewportTransforms
    {
        PxPoint WindowToImage(PxPoint windowPx);
        PxPoint ImageToWindow(PxPoint imagePx);
        PxRect WindowToImage(PxRect windowRect);
        PxRect ImageToWindow(PxRect imageRect);
    }
}