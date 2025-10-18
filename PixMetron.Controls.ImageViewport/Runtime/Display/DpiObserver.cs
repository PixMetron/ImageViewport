using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

using PixMetron.Controls.ImageViewport.Contracts.Abstractions;

namespace PixMetron.Controls.ImageViewport.Runtime.Display
{
    /// <summary>
    /// Observes DPI changes for a visual element and updates the viewport service accordingly.
    /// </summary>
    internal static class DpiObserver
    {
        /// <summary>
        /// Attaches DPI monitoring to a visual element.
        /// </summary>
        /// <param name="visual">The visual element to monitor.</param>
        /// <param name="service">The viewport service to update when DPI changes.</param>
        /// <returns>A disposable subscription, or null if DPI monitoring is not available.</returns>
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

        /// <summary>
        /// Represents a subscription to DPI change events.
        /// </summary>
        sealed class DpiSubscription : IDisposable
        {
            readonly HwndSource _source;
            readonly System.Reflection.EventInfo _event;
            readonly HwndDpiChangedEventHandler _handler;
            bool _disposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="DpiSubscription"/> class.
            /// </summary>
            /// <param name="source">The HWND source.</param>
            /// <param name="evt">The event information.</param>
            /// <param name="handler">The event handler.</param>
            public DpiSubscription(HwndSource source, System.Reflection.EventInfo evt, HwndDpiChangedEventHandler handler)
            {
                _source = source;
                _event = evt;
                _handler = handler;
            }

            /// <summary>
            /// Disposes the subscription and removes the event handler.
            /// </summary>
            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;

                try
                {
                    _event.RemoveEventHandler(_source, _handler);
                } catch
                {
                    // Already disposed object, ignore
                }
            }
        }
    }
}