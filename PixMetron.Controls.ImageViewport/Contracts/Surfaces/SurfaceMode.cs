namespace PixMetron.Controls.ImageViewport.Contracts.Surfaces
{
    public enum SurfaceMode
    {
        Follow,      // 由控件统一 Push(ViewportMatrix)，用"图像像素"绘制
        Independent  // 控件不做矩阵操作，Surface 自己处理（窗口坐标/自推矩阵）
    }
}