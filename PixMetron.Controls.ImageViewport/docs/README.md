# PixMetron.Controls.ImageViewport

A lightweight, extensible WPF image viewport control for .NET 8.  
It focuses on core state/infra; behaviors and rendering are plugged in via a Facade + Handlers + Surfaces architecture.

Key scenarios:
- Smooth pan/zoom (wheel/mouse), zoom-at-cursor or custom pivot
- DPI-aware; exposes DPI scale changes
- Coordinate conversions (window px ↔ image px)
- Layered rendering pipeline (surfaces)
- Extensible input routing and context menu
- Minimal core control; business logic lives in the facade

## Package & Requirements
- Target framework: net8.0-windows (WPF)
- PackageId: PixMetron.Controls.ImageViewport
- Install from NuGet (if published) or add a project reference

## Quick Start (XAML-first)

The attached properties in `Defaults.Viewport` will auto-create a sensible default facade (image layer + pan/zoom handler + auto-fit).

```XAML
<Window x:Class="Demo.MainWindow" 
  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" 
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" 
  xmlns:iv="clr-namespace:PixMetron.Controls.ImageViewport;assembly=PixMetron.Controls.ImageViewport" 
  xmlns:ivd="clr-namespace:PixMetron.Controls.ImageViewport.Defaults;assembly=PixMetron.Controls.ImageViewport"> 
  <Grid> 
  <iv:ImageViewport 
    x:Name="Viewport" 
    ivd:Viewport.Source="{Binding ImageSource}" 
    ivd:Viewport.AutoFitMode="Always" 
    ivd:Viewport.PanButton="Middle" 
    ivd:Viewport.RequireCtrlForWheelZoom="False" 
    ivd:Viewport.UseImageCoordinateZoom="True" 
    ivd:Viewport.WheelPivot="Mouse" 
    ivd:Viewport.ScaleFactor="1.1" 
    ivd:Viewport.MinScale="0.02" 
    ivd:Viewport.MaxScale="40" /> 
  </Grid> 
</Window>
```

Attached property defaults (can be changed at runtime):
- PanButton: Middle
- RequireCtrlForWheelZoom: False
- UseImageCoordinateZoom: True
- WheelPivot: Mouse
- ScaleFactor: 1.1
- MinScale: 0.02, MaxScale: 40
- AutoFitMode: Always
- CustomPivotWindowPxProvider: optional Func<PxPoint> to override zoom pivot

## Programmatic Use

Namespaces

```csharp
using PixMetron.Controls.ImageViewport; 
using PixMetron.Controls.ImageViewport.Contracts.Abstractions; // PxPoint, PxRect, PxSize 
using PixMetron.Controls.ImageViewport.Defaults;               // DefaultImageViewportFacade, Viewport attached props
```

Common operations

```csharp
// Zoom at a window pixel or at an image pixel anchor 
Viewport.ZoomAtWindowPx(1.2, new PxPoint(120, 80)); 
Viewport.ZoomAtImagePx(0.9, new PxPoint(1024, 768));

// Pan by window pixels 
Viewport.PanWindowPx(dx: 30, dy: -20);

// Fit a given image rectangle to the viewport 
Viewport.FitImageRect(new PxRect(0, 0, imageWidth, imageHeight));

// Coordinate conversions 
PxPoint imgPx = Viewport.WindowToImage(new PxPoint(50, 50)); 
PxPoint winPx = Viewport.ImageToWindow(new PxPoint(1000, 800));
```

Events (subscribe as needed; avoid feedback loops if you auto-fit on change)

```csharp
Viewport.ViewportChanged += (s, view) => { /* view.Scale, view.ViewportRectInImage, view.WindowPixelSize, ... */ }; 
Viewport.ScaleChanged += (s, e) => { /* e.Scale */ }; 
Viewport.PanChanged += (s, e) => { /* e.TopLeft */ }; 
Viewport.WindowSizeChanged += (s, e) => { /* e.Size */ }; 
Viewport.DpiScaleChanged += (s, e) => { /* e.DpiScaleX, e.DpiScaleY */ }; 
Viewport.ContextMenuRequested += (s, p) => { /* build/open your menu if not provided by facade */ };
```

## Architecture Overview

- ImageViewport (Control): core state and input plumbing only (no business logic).  
  Template part: `PART_SurfaceHost` (internal layered host).
- IViewportFacade: supplies
  - IViewportService: pan/zoom/fit + ViewportInfo snapshots
  - IEnumerable<ISurfaceRenderer>: layered renderers (image, overlays, etc.)
  - IInputRouter: input handling (wheel/move/down/up/left/right/middle)
  - IContextMenuProvider: context menu on right-click
  - IViewportTransforms: coordinate conversions for the current `ViewportInfo`

Built-ins:
- BuiltInViewportService: default pan/zoom/fit behavior
- BuiltInViewportTransforms: window↔image conversions
- DefaultImageViewportFacade: image surface + pan/zoom handler + auto-fit
- Attached `Defaults.Viewport`: auto-wires the default facade and keeps it in sync

Coordinate types:
- PxPoint, PxRect, PxSize are pixel-based (image/window space)

## Advanced: Custom Facade / Surfaces / Input

Implement your own `IViewportFacade` to:
- Provide custom `IViewportService` (e.g., constraints, snapping, tiling)
- Supply additional `ISurfaceRenderer` overlays
- Plug your `IInputRouter` or `IContextMenuProvider`

Using the default facade programmatically:

```csharp
var facade = new DefaultImageViewportFacade(Viewport, options: /* IPanZoomOptions (from Defaults.Viewport at runtime) */, source: null); 
Viewport.Facade = facade; 
facade.SetSource(myImageSource); 
facade.SetAutoFitMode(AutoFitMode.Always);
```

Note: If you only need the defaults, prefer the XAML attached properties; they create and manage the facade automatically.

## Notes
- DPI-aware: `DpiScaleX/Y` exposed on the control and raised via `DpiScaleChanged`.
- `Viewport.ViewportMatrix` is a read-only `MatrixTransform` (image→window) you can use in bindings.
- The control invalidates its layered host on state updates to avoid a one-frame lag.
- API intentionally small; extend via facades and surfaces.

## License
See the repository for license information.
