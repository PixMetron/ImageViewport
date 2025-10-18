using System.Windows;
using System.Windows.Media;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;

namespace PixMetron.Controls.ImageViewport.Surfaces.Primitives
{
    /// <summary>
    /// A minimalist image rendering layer that draws an <see cref="ImageSource"/> 
    /// in image pixel space (0,0,w,h) to the scene coordinate system.
    /// </summary>
    /// <remarks>
    /// This renderer provides basic image rendering functionality with automatic size detection
    /// for bitmap sources. It operates in the Follow transform mode, meaning the viewport
    /// transformation is automatically applied by the control.
    /// </remarks>
    public sealed class ImageSurfaceRenderer : ISurfaceRenderer
    {
        /// <summary>
        /// Gets or sets the image source to be rendered.
        /// </summary>
        /// <value>The <see cref="ImageSource"/> to render, or null if no image is available.</value>
        public ImageSource? Source { get; set; }

        /// <summary>
        /// Gets or sets the rectangle defining the image's extent in pixel coordinates.
        /// </summary>
        /// <value>
        /// A <see cref="PxRect"/> representing the image bounds. If width or height is less than or equal to 0
        /// and <see cref="Source"/> is a <see cref="System.Windows.Media.Imaging.BitmapSource"/>, 
        /// the pixel dimensions will be automatically retrieved during rendering.
        /// </value>
        public PxRect ImageRectPx { get; set; }

        /// <summary>
        /// Gets the transform mode for this renderer, which follows the viewport transformation.
        /// </summary>
        /// <value>Always returns <see cref="SurfaceMode.Follow"/>.</value>
        public SurfaceMode TransformMode => SurfaceMode.Follow;

        /// <summary>
        /// Renders the image to the drawing context.
        /// </summary>
        /// <param name="dc">The drawing context to render into.</param>
        /// <param name="ctx">The render context containing viewport information and transforms.</param>
        /// <remarks>
        /// The rendering process:
        /// <list type="number">
        /// <item>If <see cref="ImageRectPx"/> has invalid dimensions and <see cref="Source"/> is a <see cref="System.Windows.Media.Imaging.BitmapSource"/>,
        /// the rectangle is automatically set to (0, 0, PixelWidth, PixelHeight).</item>
        /// <item>If the source is null or dimensions are invalid, rendering is skipped.</item>
        /// <item>The image is drawn to the specified rectangle in image coordinate space.</item>
        /// </list>
        /// </remarks>
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