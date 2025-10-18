using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Facade;
using PixMetron.Controls.ImageViewport.Handlers.Contracts;
using PixMetron.Controls.ImageViewport.Handlers.Routers;
using PixMetron.Controls.ImageViewport.Surfaces.Primitives;

namespace ImageViewportDemoApp
{
    public partial class MainWindow : Window
    {
        private ImageViewportFacade _facade;
        private RulerSurfaceRenderer _ruler = new();
        private CrosshairSurfaceRenderer _hud = new();
        private GridSurfaceRenderer _measure = new();
        private PanZoomHandler _panZoom;
        private PanZoomOptions _options = new();
        private bool _isRulerVisible = true;
        private bool _isHUDVisible = true;
        private bool _isMeasureVisible = true;
        readonly ImageSource _image;

        public MainWindow()
        {
            InitializeComponent();

            _options.PanButton = PanButton.Left;
            _panZoom = new PanZoomHandler(_options);
            _facade = new ImageViewportFacade();
            _facade.SetInputRouter(_panZoom);
            _image = new BitmapImage(new System.Uri("pack://application:,,,/Assets/demo_checker.png"));
            var imageRectPx = new PxRect(0, 0, _image.Width, _image.Height);
            _facade.AddSurface(new ImageSurfaceRenderer
            {
                Source = _image,
                ImageRectPx = imageRectPx
            });
            _facade.AddSurface(_ruler);
            _facade.AddSurface(_hud);
            _facade.AddSurface(_measure);
            Viewport.Facade = _facade;
            UpdateImageFit();
            Viewport.WindowSizeChanged += (s, e) => UpdateImageFit();
        }

        private void ToggleRuler_Click(object sender, RoutedEventArgs e)
        {
            _isRulerVisible = !_isRulerVisible;
            _facade.SetSurfaceVisible(_ruler, _isRulerVisible);
            RulerMenuItem.IsChecked = _isRulerVisible;
            Viewport.InvilidateViewport();
        }

        private void ToggleHUD_Click(object sender, RoutedEventArgs e)
        {
            _isHUDVisible = !_isHUDVisible;
            _facade.SetSurfaceVisible(_hud, _isHUDVisible);
            HUDMenuItem.IsChecked = _isHUDVisible;
            Viewport.InvilidateViewport();
        }

        private void ToggleMeasure_Click(object sender, RoutedEventArgs e)
        {
            _isMeasureVisible = !_isMeasureVisible;
            _facade.SetSurfaceVisible(_measure, _isMeasureVisible);
            MeasureMenuItem.IsChecked = _isMeasureVisible;
            Viewport.InvilidateViewport();
        }

        private void UseLeftPan_Click(object sender, RoutedEventArgs e)
        {
            _options.PanButton = PanButton.Left;
            UseLeftPanItem.IsChecked = true;
            UseMiddlePanItem.IsChecked = false;
            UseRightPanItem.IsChecked = true;
        }

        private void UseMiddlePan_Click(object sender, RoutedEventArgs e)
        {
            _options.PanButton = PanButton.Middle;
            UseLeftPanItem.IsChecked = false;
            UseMiddlePanItem.IsChecked = true;
            UseRightPanItem.IsChecked = false;
        }

        private void UseRightPan_Click(object sender, RoutedEventArgs e)
        {
            _options.PanButton = PanButton.Right;
            UseLeftPanItem.IsChecked = false;
            UseMiddlePanItem.IsChecked = false;
            UseRightPanItem.IsChecked = true;
        }

        // Automatically scale image to fit viewport when DPI or window size changes
        private void UpdateImageFit()
        {
            var newRect = new PxRect(0, 0, _image.Width, _image.Height);
            _facade.Service.FitImageRect(newRect);
        }

    }
}
