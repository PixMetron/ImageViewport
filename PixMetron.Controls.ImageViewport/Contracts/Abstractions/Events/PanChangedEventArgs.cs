namespace PixMetron.Controls.ImageViewport.Contracts.Abstractions.Events
{
    public sealed class PanChangedEventArgs : EventArgs
    {
        public PanChangedEventArgs(PxPoint newTopLeft)
        {
            NewTopLeftInImagePx = newTopLeft;
        }

        public PxPoint NewTopLeftInImagePx { get; }
    }
}