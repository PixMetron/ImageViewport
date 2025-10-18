using System.Windows.Controls;

namespace PixMetron.Controls.ImageViewport.Contracts.Input
{
    public interface IContextMenuProvider
    {
        ContextMenu? BuildContextMenu(PointerEvent p);
    }
}