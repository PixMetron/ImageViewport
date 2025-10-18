namespace PixMetron.Controls.ImageViewport.Contracts.Abstractions.Events
{
    public sealed class DpiScaleChangedEventArgs : EventArgs
    {
        public DpiScaleChangedEventArgs(double x, double y)
        {
            DpiScaleX = x; DpiScaleY = y;
        }

        public double DpiScaleX { get; }
        public double DpiScaleY { get; }
    }
}
