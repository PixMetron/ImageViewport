# PixMetron ImageViewport

A high-performance, extensible WPF image viewport control with layered surface rendering, pan/zoom capabilities, and a flexible facade architecture.

## Features

- **🚀 High Performance**: Optimized rendering with layered surface architecture
- **🎯 Flexible Pan/Zoom**: Configurable mouse and keyboard interactions with multiple pivot modes
- **📐 Auto-Fit Support**: Smart automatic fitting with multiple modes (Always, OnLoad, Never)
- **🔧 Extensible Design**: Facade pattern for custom viewport behaviors
- **🎨 Surface Rendering**: Multi-layer surface system for overlays and annotations
- **⚡ .NET 8 WPF**: Built on the latest .NET platform

## Packages

| Package | Description | Version |
|---------|-------------|---------|
| `PixMetron.Controls.ImageViewport` | Core viewport control and contracts | ![NuGet](https://img.shields.io/nuget/v/PixMetron.Controls.ImageViewport) |
| `PixMetron.Controls.ImageViewport.Defaults` | Default facade with attached properties | ![NuGet](https://img.shields.io/nuget/v/PixMetron.Controls.ImageViewport.Defaults) |
| `PixMetron.Controls.ImageViewport.Handlers` | Pan/Zoom input handlers | ![NuGet](https://img.shields.io/nuget/v/PixMetron.Controls.ImageViewport.Handlers) |
| `PixMetron.Controls.ImageViewport.Surfaces` | Built-in surface renderers | ![NuGet](https://img.shields.io/nuget/v/PixMetron.Controls.ImageViewport.Surfaces) |

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package PixMetron.Controls.ImageViewport.Defaults
```

Or via Package Manager Console:

```bash
Install-Package PixMetron.Controls.ImageViewport.Defaults
```

## Quick Start

### Basic Usage with Attached Properties

```xml
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:vp="clr-namespace:PixMetron.Controls.ImageViewport;assembly=PixMetron.Controls.ImageViewport" xmlns:def="clr-namespace:PixMetron.Controls.ImageViewport.Defaults;assembly=PixMetron.Controls.ImageViewport.Defaults">
<vp:ImageViewport def:Viewport.Source="{Binding ImageSource}"
                  def:Viewport.AutoFitMode="Always"
                  def:Viewport.PanButton="Middle"
                  def:Viewport.WheelPivot="Mouse"
                  def:Viewport.ScaleFactor="1.1"
                  def:Viewport.MinScale="0.02"
                  def:Viewport.MaxScale="40.0" />
</Window>
```

### Programmatic Configuration

```csharp
using PixMetron.Controls.ImageViewport; 
using PixMetron.Controls.ImageViewport.Defaults;
// Create viewport 
var viewport = new ImageViewport();
// Configure using attached properties 
Viewport.SetSource(viewport, myImageSource); 
Viewport.SetAutoFitMode(viewport, AutoFitMode.Always); 
Viewport.SetPanButton(viewport, PanButton.Middle); 
Viewport.SetWheelPivot(viewport, ZoomPivotMode.Mouse); 
Viewport.SetScaleFactor(viewport, 1.1);
```

## Configuration Options

### Pan/Zoom Settings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `PanButton` | `PanButton` | `Middle` | Mouse button for panning (Left, Middle, Right, None) |
| `WheelPivot` | `ZoomPivotMode` | `Mouse` | Zoom pivot point (Mouse, Center, Custom) |
| `ScaleFactor` | `double` | `1.1` | Zoom factor per wheel notch |
| `MinScale` | `double` | `0.02` | Minimum zoom scale |
| `MaxScale` | `double` | `40.0` | Maximum zoom scale |
| `RequireCtrlForWheelZoom` | `bool` | `false` | Require Ctrl key for wheel zoom |
| `UseImageCoordinateZoom` | `bool` | `true` | Use image coordinate space for zoom pivot |

### Auto-Fit Modes

| Mode | Description |
|------|-------------|
| `Always` | Auto-fit on image load and window resize |
| `OnLoad` | Auto-fit only when image is first loaded |
| `Never` | Manual control only |

## Advanced Usage

### Custom Facade Implementation

```csharp
using PixMetron.Controls.ImageViewport.Contracts.Facade;
public class CustomFacade : IViewportFacade 
{ 
    public IViewportService Service { get; } 
    public IEnumerable<ISurfaceRenderer> Surfaces { get; } 
    public IInputRouter? InputRouter { get; } 
    public IContextMenuProvider? ContextMenu { get; }
    public ViewportInfo Current => Service.Current;

    public IViewportTransforms GetTransforms(in ViewportInfo info)
    {
        // Custom transform logic
    }
}

// Apply custom facade 
viewport.Facade = new CustomFacade();
```

### Custom Surface Renderer

```csharp
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;
public class AnnotationRenderer : ISurfaceRenderer 
{ 
    public void Render(IDrawingSession session, in ViewportInfo viewport) 
    { 
        // Custom rendering logic 
        using DrawingContext var dc = session.Context; d
        c.DrawEllipse(/* ... */); 
    } 
}
```


## Architecture

The library follows a layered architecture:
```text
┌─────────────────────────────────┐ 
│     ImageViewport Control       │  
│        Core WPF control         │
├─────────────────────────────────┤ 
│      IViewportFacade            │  
│     Facade pattern for behavior │ 
├─────────────────────────────────┤ 
│  ┌────────────┬──────────────┐  │ 
│  │  Surfaces  │Input Handlers│  │  
│  │     Pluggable components  │  │
│  └────────────┴──────────────┘  │ 
├─────────────────────────────────┤ 
│    ViewportService + Transforms │  
│          Core services          │
└─────────────────────────────────┘
```


### Key Interfaces

- **`IViewportFacade`**: Main facade interface for viewport behavior
- **`ISurfaceRenderer`**: Renders content on specific layers
- **`IInputRouter`**: Handles mouse/keyboard input
- **`IViewportService`**: Manages viewport state
- **`IViewportTransforms`**: Coordinate transformations

## Examples

Check out the `PixMetron.ImageViewport.DemoApp` project in the repository for comprehensive examples including:

- Basic image viewing
- Custom pan/zoom configurations
- Layer rendering
- Custom facades

## Requirements

- **.NET 8.0** or later
- **Windows** platform (WPF)
- **Visual Studio 2022** or later (for development)

## Building from Source

```bash
git clone https://github.com/PixMetron/ImageViewport.git 
cd ImageViewport 
dotnet restore 
dotnet build
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the **Apache License 2.0** - see the LICENSE file for details.

## Repository

- **GitHub**: [https://github.com/PixMetron/ImageViewport](https://github.com/PixMetron/ImageViewport)
- **Organization**: [https://github.com/PixMetron](https://github.com/PixMetron)

## Support

For issues, questions, or feature requests, please use the [GitHub Issues](https://github.com/PixMetron/ImageViewport/issues) page.

---

Made with ❤️ by PixMetron
