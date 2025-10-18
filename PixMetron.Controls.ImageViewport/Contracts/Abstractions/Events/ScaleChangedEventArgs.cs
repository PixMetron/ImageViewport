namespace PixMetron.Controls.ImageViewport.Contracts.Abstractions.Events
{
    public sealed class ScaleChangedEventArgs : EventArgs
    {
        public ScaleChangedEventArgs(double newScale)
        {
            NewScale = newScale;
        }

        public double NewScale { get; }
    }
}