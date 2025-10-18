using System.Windows;
using System.Windows.Media;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;

namespace PixMetron.Controls.ImageViewport.Contracts.Surfaces
{
    /// <summary>
    /// 表示图像视口渲染过程中的上下文信息，包含坐标变换、窗口区域、DPI缩放等渲染所需的所有数据。
    /// </summary>
    /// <remarks>
    /// 此结构体是不可变的，用于在渲染管道中传递上下文信息，确保线程安全和性能优化。
    /// </remarks>
    public readonly struct SurfaceRenderContext
    {
        /// <summary>
        /// 获取窗口的矩形区域。
        /// </summary>
        /// <value>表示窗口边界的 <see cref="Rect"/> 对象。</value>
        public Rect WindowRect { get; }

        /// <summary>
        /// 获取视口信息。
        /// </summary>
        /// <value>包含视口详细信息的 <see cref="ViewportInfo"/> 对象。</value>
        public ViewportInfo View { get; }

        /// <summary>
        /// 获取视口变换操作的接口。
        /// </summary>
        /// <value>提供坐标变换功能的 <see cref="IViewportTransforms"/> 接口实例。</value>
        public IViewportTransforms Transforms { get; }

        /// <summary>
        /// 获取从图像坐标系到窗口坐标系的变换矩阵。
        /// </summary>
        /// <value>用于将图像坐标转换为窗口坐标的 <see cref="Matrix"/> 对象。</value>
        public Matrix ViewportMatrix { get; }   // image -> window

        /// <summary>
        /// 获取从窗口坐标系到图像坐标系的逆变换矩阵。
        /// </summary>
        /// <value>用于将窗口坐标转换为图像坐标的 <see cref="Matrix"/> 对象。</value>
        public Matrix ViewportMatrixInverse { get; }   // window -> image

        /// <summary>
        /// 获取水平方向的 DPI 缩放因子。
        /// </summary>
        /// <value>水平方向的 DPI 缩放倍数。</value>
        public double DpiScaleX { get; }

        /// <summary>
        /// 获取垂直方向的 DPI 缩放因子。
        /// </summary>
        /// <value>垂直方向的 DPI 缩放倍数。</value>
        public double DpiScaleY { get; }

        /// <summary>
        /// 获取附加的载荷数据集合。
        /// </summary>
        /// <value>包含额外数据的只读列表，如果没有提供数据则为空集合。</value>
        public IReadOnlyList<object> Payload { get; }

        /// <summary>
        /// 获取服务提供者，用于依赖注入或服务定位。
        /// </summary>
        /// <value>服务提供者实例，可能为 null。</value>
        public IServiceProvider? Services { get; }

        /// <summary>
        /// 获取通用标签对象，用于存储用户自定义数据。
        /// </summary>
        /// <value>用户定义的标签对象，可能为 null。</value>
        public object? Tag { get; }

        /// <summary>
        /// 初始化 <see cref="SurfaceRenderContext"/> 结构体的新实例。
        /// </summary>
        /// <param name="windowRect">窗口的矩形区域。</param>
        /// <param name="view">视口信息。</param>
        /// <param name="transforms">视口变换操作接口。</param>
        /// <param name="viewportMatrix">从图像坐标系到窗口坐标系的变换矩阵。</param>
        /// <param name="viewportMatrixInverse">从窗口坐标系到图像坐标系的逆变换矩阵。</param>
        /// <param name="dpiX">水平方向的 DPI 缩放因子。</param>
        /// <param name="dpiY">垂直方向的 DPI 缩放因子。</param>
        /// <param name="payload">可选的附加载荷数据集合。</param>
        /// <param name="services">可选的服务提供者。</param>
        /// <param name="tag">可选的通用标签对象。</param>
        public SurfaceRenderContext(
            Rect windowRect,
            ViewportInfo view,
            IViewportTransforms transforms,
            Matrix viewportMatrix,
            Matrix viewportMatrixInverse,
            double dpiX,
            double dpiY,
            IReadOnlyList<object>? payload = null,
            IServiceProvider? services = null,
            object? tag = null)
        {
            WindowRect = windowRect;
            View = view;
            Transforms = transforms;
            ViewportMatrix = viewportMatrix;
            ViewportMatrixInverse = viewportMatrixInverse;
            DpiScaleX = dpiX;
            DpiScaleY = dpiY;
            Payload = payload ?? Array.Empty<object>();
            Services = services;
            Tag = tag;
        }
    }
}