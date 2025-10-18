using PixMetron.Controls.ImageViewport.Contracts.Input;
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;

namespace PixMetron.Controls.ImageViewport.Facade
{
    public interface IInteractiveSurface : ISurfaceRenderer
    {
        IInputRouter? Router { get; }
    }

    public interface IInputPrioritizable
    {
        int Priority { get; }
    }
}