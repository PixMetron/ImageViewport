using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;

namespace PixMetron.Controls.ImageViewport.Surfaces.Primitives
{
    /// <summary>
    /// A generic multi-image renderer that defines Local-to-Scene transformation matrices 
    /// and local extents for each image, rendering them uniformly to the window.
    /// </summary>
    /// <remarks>
    /// This renderer supports rendering multiple images with independent transformations.
    /// Each image can have its own local coordinate space, which is transformed to scene space
    /// using a matrix that supports scaling, translation, and rotation.
    /// </remarks>
    public sealed class MultiImageRenderer : ISurfaceRenderer
    {
        /// <summary>
        /// Represents a single image entry with its source, local extent, and transformation matrix.
        /// </summary>
        public sealed class ImageEntry
        {
            /// <summary>
            /// Gets or sets the image source to be rendered.
            /// </summary>
            /// <value>The <see cref="ImageSource"/> to render, or null if no image is available.</value>
            public ImageSource? Source { get; set; }

            /// <summary>
            /// Gets or sets the rectangle defining the image's extent in its local coordinate space.
            /// </summary>
            /// <value>A <see cref="PxRect"/> representing the local bounds (e.g., 0, 0, width, height).</value>
            public PxRect LocalExtent { get; set; }

            /// <summary>
            /// Gets or sets the transformation matrix from local coordinates to scene coordinates.
            /// </summary>
            /// <value>A <see cref="Matrix"/> that supports scaling, translation, and rotation operations.</value>
            public Matrix LocalToScene { get; set; }
        }

        /// <summary>
        /// Gets the collection of images to be rendered.
        /// </summary>
        /// <value>A mutable list of <see cref="ImageEntry"/> instances.</value>
        public List<ImageEntry> Images { get; } = new();

        /// <summary>
        /// Gets the transform mode for this renderer, which follows the viewport transformation.
        /// </summary>
        /// <value>Always returns <see cref="SurfaceMode.Follow"/>.</value>
        public SurfaceMode TransformMode => SurfaceMode.Follow;

        /// <summary>
        /// Renders all images in the collection to the drawing context.
        /// </summary>
        /// <param name="dc">The drawing context to render into.</param>
        /// <param name="ctx">The render context containing viewport information and transforms.</param>
        /// <remarks>
        /// For each image entry:
        /// <list type="number">
        /// <item>Validates the image source and extent.</item>
        /// <item>Transforms the local extent corners to scene space using the LocalToScene matrix.</item>
        /// <item>Converts scene coordinates to window coordinates using the viewport transforms.</item>
        /// <item>Draws the image in the calculated window rectangle.</item>
        /// </list>
        /// </remarks>
        public void Render(DrawingContext dc, in SurfaceRenderContext ctx)
        {
            foreach (var img in Images)
            {
                if (img?.Source is null || img.LocalExtent.Width <= 0 || img.LocalExtent.Height <= 0)
                    continue;

                // Map local rect corners -> scene -> window
                var p00 = Transform(img.LocalToScene, img.LocalExtent.TopLeft);
                var p11 = Transform(img.LocalToScene, new PxPoint(img.LocalExtent.X + img.LocalExtent.Width, img.LocalExtent.Y + img.LocalExtent.Height));
                var w0 = ctx.Transforms.ImageToWindow(p00);
                var w1 = ctx.Transforms.ImageToWindow(p11);
                var winRect = new Rect(new Point(w0.X, w0.Y), new Point(w1.X, w1.Y));

                dc.DrawImage(img.Source, winRect);
            }
        }

        /// <summary>
        /// Transforms a point using the specified transformation matrix.
        /// </summary>
        /// <param name="m">The transformation matrix to apply.</param>
        /// <param name="p">The point to transform.</param>
        /// <returns>The transformed point in the target coordinate space.</returns>
        /// <remarks>
        /// Applies the matrix transformation: x' = M11*x + M21*y + OffsetX, y' = M12*x + M22*y + OffsetY.
        /// </remarks>
        private static PxPoint Transform(Matrix m, PxPoint p)
            => new PxPoint(m.M11 * p.X + m.M21 * p.Y + m.OffsetX, m.M12 * p.X + m.M22 * p.Y + m.OffsetY);
    }
}