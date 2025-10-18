using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
using PixMetron.Controls.ImageViewport.Contracts.Input;
using PixMetron.Controls.ImageViewport.Handlers.Contracts;

namespace PixMetron.Controls.ImageViewport.Handlers.Routers
{
    /// <summary>
    /// An automatic fitting handler responsible for state management and strategy evaluation.
    /// </summary>
    /// <remarks>
    /// This handler monitors window size changes and determines when the viewport should
    /// automatically refit its content based on the configured <see cref="Mode"/>.
    /// It maintains a cached rectangle representing the target fit area and tracks
    /// window size history to evaluate fitting conditions.
    /// </remarks>
    public sealed class AutoFitHandler : IWindowSizeHandler
    {
        /// <summary>
        /// Gets or sets the automatic fitting mode that determines when refitting occurs.
        /// </summary>
        /// <value>
        /// An <see cref="AutoFitMode"/> value specifying the fitting strategy.
        /// Default is <see cref="AutoFitMode.Disabled"/>.
        /// </value>
        public AutoFitMode Mode { get; set; } = AutoFitMode.Disabled;

        private PxRect? _cachedRect;
        private PxSize _lastWindowSize;

        /// <summary>
        /// Sets the image rectangle to be fitted (cached only).
        /// </summary>
        /// <param name="rect">The rectangle in image coordinates that should be fitted to the viewport.</param>
        /// <remarks>
        /// This method stores the target rectangle for future fitting operations but does not
        /// trigger an immediate fit. The actual fitting occurs during window size change events
        /// based on the configured <see cref="Mode"/>.
        /// </remarks>
        public void SetCachedRect(PxRect rect)
        {
            _cachedRect = rect;
        }

        /// <summary>
        /// Gets the currently cached fitting rectangle.
        /// </summary>
        /// <returns>
        /// The cached <see cref="PxRect"/> if available; otherwise, <c>null</c>.
        /// </returns>
        public PxRect? GetCachedRect() => _cachedRect;

        /// <summary>
        /// Clears the cached rectangle.
        /// </summary>
        /// <remarks>
        /// After calling this method, no automatic fitting will occur until a new rectangle
        /// is set via <see cref="SetCachedRect"/>, even if the <see cref="Mode"/> is enabled.
        /// </remarks>
        public void ClearCache()
        {
            _cachedRect = null;
        }

        /// <summary>
        /// Handles window size change events (implementation of <see cref="IWindowSizeHandler"/>).
        /// </summary>
        /// <param name="sender">The event sender (typically the <see cref="ImageViewport"/> control).</param>
        /// <param name="newSize">The new window size in pixels.</param>
        /// <returns>
        /// <c>true</c> if the event was handled and fitting should occur; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method evaluates whether refitting should occur based on:
        /// </para>
        /// <list type="bullet">
        /// <item>The presence of a cached rectangle.</item>
        /// <item>The current <see cref="Mode"/> setting.</item>
        /// <item>The relationship between the new and previous window sizes.</item>
        /// </list>
        /// <para>
        /// The previous window size is always updated to the new size after evaluation.
        /// </para>
        /// </remarks>
        public bool OnWindowSizeChanged(object sender, PxSize newSize)
        {
            if (!_cachedRect.HasValue || Mode == AutoFitMode.Disabled)
            {
                _lastWindowSize = newSize;
                return false;
            }

            bool shouldFit = Mode switch
            {
                AutoFitMode.Always => true,
                AutoFitMode.OnWindowGrow => newSize.Width > _lastWindowSize.Width ||
                                           newSize.Height > _lastWindowSize.Height,
                AutoFitMode.OnWindowShrink => newSize.Width < _lastWindowSize.Width ||
                                              newSize.Height < _lastWindowSize.Height,
                _ => false
            };

            _lastWindowSize = newSize;

            // Return whether refitting is needed
            return shouldFit;
        }

        /// <summary>
        /// Determines whether refitting should occur on window size change and provides the target rectangle.
        /// </summary>
        /// <param name="newSize">The new window size in pixels.</param>
        /// <param name="rectToFit">
        /// When this method returns <c>true</c>, contains the rectangle to fit; otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if refitting should be performed; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This is a convenience method that combines the window size change evaluation with
        /// retrieval of the cached rectangle. It internally calls <see cref="OnWindowSizeChanged"/>
        /// and returns the cached rectangle only if fitting is determined to be necessary.
        /// </remarks>
        public bool ShouldRefitOnWindowSizeChange(PxSize newSize, out PxRect? rectToFit)
        {
            rectToFit = null;

            bool shouldFit = OnWindowSizeChanged(this, newSize);

            if (shouldFit && _cachedRect.HasValue)
            {
                rectToFit = _cachedRect.Value;
                return true;
            }

            return false;
        }
    }
}