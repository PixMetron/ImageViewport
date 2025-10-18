namespace PixMetron.Controls.ImageViewport.Contracts.Abstractions
{
    /// <summary>
    /// 表示一个以像素为单位的矩形区域。
    /// </summary>
    public readonly record struct PxRect(double X, double Y, double Width, double Height)
    {
        /// <summary>
        /// 获取一个值，指示该矩形是否为空（宽度或高度小于等于0）。
        /// </summary>
        public bool IsEmpty => Width <= 0 || Height <= 0;

        /// <summary>
        /// 获取矩形的左上角点。
        /// </summary>
        public PxPoint TopLeft => new(X, Y);

        /// <summary>
        /// 获取矩形的中心点。
        /// </summary>
        public PxPoint Center => new(X + Width / 2, Y + Height / 2);
    }
}