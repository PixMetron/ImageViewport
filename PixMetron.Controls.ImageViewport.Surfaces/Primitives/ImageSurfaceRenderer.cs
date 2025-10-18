using System.Windows;
using System.Windows.Media;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;

namespace PixMetron.Controls.ImageViewport.Surfaces.Primitives
{
    /// <summary>
    /// 极简图像渲染层：将 ImageSource 按图像像素空间(0,0,w,h)绘制到场景坐标系。
    /// </summary>
    public sealed class ImageSurfaceRenderer : ISurfaceRenderer
    {
        public ImageSource? Source { get; set; }
        public PxRect ImageRectPx { get; set; } // 若宽高<=0且Source为BitmapSource则自动取像素尺寸

        public SurfaceMode TransformMode => SurfaceMode.Follow;

        public void Render(DrawingContext dc, in SurfaceRenderContext ctx)
        {
            if ((ImageRectPx.Width <= 0 || ImageRectPx.Height <= 0) && Source is System.Windows.Media.Imaging.BitmapSource bs)
            {
                ImageRectPx = new PxRect(0, 0, bs.PixelWidth, bs.PixelHeight);
            }
            if (Source is null || ImageRectPx.Width <= 0 || ImageRectPx.Height <= 0) return;

            var dest = new Rect(ImageRectPx.X, ImageRectPx.Y, ImageRectPx.Width, ImageRectPx.Height);
            dc.DrawImage(Source, dest);
        }
    }
}