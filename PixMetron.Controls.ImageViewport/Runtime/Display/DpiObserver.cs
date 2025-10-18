using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;

namespace PixMetron.Controls.ImageViewport.Runtime.Display
{
    internal static class DpiObserver
    {
        public static IDisposable? Attach(Visual visual, IViewportService service)
        {
            if (visual == null) return null;
            try
            {
                var dpi = VisualTreeHelper.GetDpi(visual);
                service.SetDpi(dpi.DpiScaleX, dpi.DpiScaleY);

                var src = PresentationSource.FromVisual(visual);
                if (src is HwndSource hwnd)
                {
                    // DpiChanged is available on newer WPF; fall back silently if not present.
                    var evt = hwnd.GetType().GetEvent("DpiChanged");
                    if (evt != null)
                    {
                        HwndDpiChangedEventHandler handler = (s, e) =>
                        {
                            if (visual.Dispatcher.CheckAccess())
                            {
                                var d = VisualTreeHelper.GetDpi(visual);
                                service.SetDpi(d.DpiScaleX, d.DpiScaleY);
                            } else
                            {
                                visual.Dispatcher.Invoke(() =>
                                {
                                    var d = VisualTreeHelper.GetDpi(visual);
                                    service.SetDpi(d.DpiScaleX, d.DpiScaleY);
                                });
                            }
                        };

                        evt.AddEventHandler(hwnd, handler);

                        return new DpiSubscription(hwnd, evt, handler);
                    }
                }
            } catch
            {
                // Best-effort; ignore if environment does not support DPI query
            }

            return null;
        }

        sealed class DpiSubscription : IDisposable
        {
            readonly HwndSource _source;
            readonly System.Reflection.EventInfo _event;
            readonly HwndDpiChangedEventHandler _handler;
            bool _disposed;

            public DpiSubscription(HwndSource source, System.Reflection.EventInfo evt, HwndDpiChangedEventHandler handler)
            {
                _source = source;
                _event = evt;
                _handler = handler;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;

                try
                {
                    _event.RemoveEventHandler(_source, _handler);
                } catch
                {
                    // 已释放的对象，忽略
                }
            }
        }
    }
}