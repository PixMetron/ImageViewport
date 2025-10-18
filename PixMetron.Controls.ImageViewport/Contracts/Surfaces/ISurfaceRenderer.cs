using System.Windows.Media;

namespace PixMetron.Controls.ImageViewport.Contracts.Surfaces
{
    public interface ISurfaceRenderer
    {
        /// <summary>该层的变换策略：Follow / Independent</summary>
        SurfaceMode TransformMode { get; }

        /// <summary>
        /// 渲染：
        /// - TransformMode=Follow：控件已 Push(ctx.ViewportMatrix)，此处按图像坐标绘制；
        /// - TransformMode=Independent：控件不做矩阵，此处按窗口坐标或自定义方式绘制。
        /// </summary>
        void Render(DrawingContext dc, in SurfaceRenderContext ctx);
    }
}