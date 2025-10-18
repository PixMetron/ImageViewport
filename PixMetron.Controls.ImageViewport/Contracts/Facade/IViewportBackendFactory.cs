using PixMetron.Controls.ImageViewport.Contracts.Abstractions;

namespace PixMetron.Controls.ImageViewport.Contracts.Facade
{
    public interface IViewportBackendFactory
    {
        IViewportService CreateViewportService();
        IViewportTransforms CreateTransforms(ViewportInfo snapshot);
    }
}
