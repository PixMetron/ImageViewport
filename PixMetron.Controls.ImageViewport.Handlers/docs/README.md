# PixMetron.Controls.ImageViewport.Handlers

Input handlers for the PixMetron ImageViewport control, providing extensible pan/zoom interactions and automatic fitting capabilities for WPF applications.

## Overview

**PixMetron.Controls.ImageViewport.Handlers** is a companion package to the core ImageViewport control that implements user interaction logic. It follows the separation of concerns principle by decoupling input handling from the core viewport control, making it easy to customize or replace interaction behaviors.

## Features

- **🎯 Pan/Zoom Handlers**: Configurable mouse-based panning and zooming with multiple pivot modes
- **📐 Auto-Fit Handler**: Smart automatic viewport fitting with multiple modes
- **🔧 Extensible Architecture**: Implement custom handlers via clean interfaces
- **⚙️ Flexible Configuration**: Fine-grained control over interaction behaviors
- **🖱️ Multi-Button Support**: Configure different mouse buttons for panning
- **🎚️ Zoom Control**: Scale factor, min/max scale limits, and pivot modes

## Installation

Install via NuGet Package Manager:
```powershell
Install-Package PixMetron.Controls.ImageViewport.Handlers
```
Or via .NET CLI:
```bash
dotnet add package PixMetron.Controls.ImageViewport.Handlers
```

## Package Requirements

- **Target Framework**: .NET 8.0 Windows (WPF)
- **Dependencies**: `PixMetron.Controls.ImageViewport`

## Key Components

### 1. Pan/Zoom Handler

Handles mouse-based panning and wheel-based zooming interactions.

**Configuration Options** (`IPanZoomOptions`):

- `PanButton`: Which mouse button triggers panning (`Left`, `Middle`, `Right`)
- `RequireCtrlForWheelZoom`: Whether Ctrl key is required for mouse wheel zoom
- `UseImageCoordinateZoom`: Use image coordinate space for zoom operations
- `WheelPivot`: Zoom pivot mode (`Mouse`, `Center`, `Custom`)
- `ScaleFactor`: Zoom step multiplier (e.g., 1.1 = 10% per wheel tick)
- `MinScale`: Minimum allowed zoom scale
- `MaxScale`: Maximum allowed zoom scale
- `CustomPivotWindowPxProvider`: Custom pivot point provider function

**Example:**
```csharp
using PixMetron.Controls.ImageViewport.Handlers.Contracts;
var options = new PanZoomOptions 
{ 
    PanButton = PanButton.Middle, 
    RequireCtrlForWheelZoom = false, 
    UseImageCoordinateZoom = true, 
    WheelPivot = ZoomPivotMode.Mouse, 
    ScaleFactor = 1.1, 
    MinScale = 0.02, 
    MaxScale = 40.0 
};
```

### 2. Auto-Fit Handler

Automatically fits the viewport content based on window size changes.

**Auto-Fit Modes** (`AutoFitMode`):

- `Disabled`: No automatic fitting
- `Always`: Always fit content when window size changes
- `OnWindowGrow`: Fit content only when window grows
- `OnWindowShrink`: Fit content only when window shrinks

**Example:**
```csharp
using PixMetron.Controls.ImageViewport.Handlers.Routers; 
using PixMetron.Controls.ImageViewport.Handlers.Contracts;
var autoFitHandler = new AutoFitHandler { Mode = AutoFitMode.Always };
// Set the cached rect for auto-fit calculation 
autoFitHandler.SetCachedRect(new PxRect(0, 0, imageWidth, imageHeight));
```

### 3. Handler Interfaces

Implement these interfaces to create custom handlers:

**IWindowSizeHandler**

```csharp

public interface IWindowSizeHandler 
{ 
    bool OnWindowSizeChanged(object sender, PxSize newSize); 
}
```

**IMouseButtonHandler**
```csharp
public interface IMouseButtonHandler 
{ 
    bool OnMouseDown(object sender, MouseButtonEventArgs e); 
    bool OnMouseMove(object sender, MouseEventArgs e); 
    bool OnMouseUp(object sender, MouseButtonEventArgs e); 
}
```

**IMouseWheelHandler**
```csharp
public interface IMouseWheelHandler 
{ 
    bool OnMouseWheel(object sender, MouseWheelEventArgs e); 
}
```

## Usage with Defaults Package

The easiest way to use handlers is through the **PixMetron.Controls.ImageViewport.Defaults** package with attached properties:
```xml
<Window 
  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" 
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" 
  xmlns:vp="clr-namespace:PixMetron.Controls.ImageViewport;assembly=PixMetron.Controls.ImageViewport" 
  xmlns:def="clr-namespace:PixMetron.Controls.ImageViewport.Defaults;assembly=PixMetron.Controls.ImageViewport.Defaults">
<vp:ImageViewport def:Viewport.Source="{Binding ImageSource}"
                  def:Viewport.AutoFitMode="Always"
                  def:Viewport.PanButton="Middle"
                  def:Viewport.RequireCtrlForWheelZoom="False"
                  def:Viewport.WheelPivot="Mouse"
                  def:Viewport.ScaleFactor="1.1"
                  def:Viewport.MinScale="0.02"
                  def:Viewport.MaxScale="40" />
</Window>
```

## Programmatic Configuration

For programmatic setup, integrate handlers with your custom facade:
```csharp
using PixMetron.Controls.ImageViewport; 
using PixMetron.Controls.ImageViewport.Handlers.Routers; 
using PixMetron.Controls.ImageViewport.Handlers.Contracts;
var viewport = new ImageViewport();

// Create and configure pan/zoom options 
var panZoomOptions = new PanZoomOptions { PanButton = PanButton.Middle, WheelPivot = ZoomPivotMode.Mouse, ScaleFactor = 1.1, MinScale = 0.02, MaxScale = 40.0 };

// Create handlers (example integration - actual usage depends on your facade) 
var panHandler = new PanHandler { PanButton = panZoomOptions.PanButton }; 
var autoFitHandler = new AutoFitHandler { Mode = AutoFitMode.Always };

// Register handlers with your facade implementation 
// (specific registration depends on your IImageViewportFacade implementation)
```

## Zoom Pivot Modes

### Mouse Pivot (Default)
Zooms in/out with the mouse cursor as the center point, keeping the pixel under the cursor stationary.
```csharp
options.WheelPivot = ZoomPivotMode.Mouse;
```

### Center Pivot
Zooms in/out with the viewport center as the pivot point.
```csharp
options.WheelPivot = ZoomPivotMode.Center;
```

### Custom Pivot
Provides a custom pivot point for zoom operations.
```csharp
options.WheelPivot = ZoomPivotMode.Custom;
options.CustomPivotWindowPxProvider = (viewport) => new PxPoint(viewport.ActualWidth / 2, viewport.ActualHeight / 2);
```

## Advanced Scenarios

### Custom Handler Implementation

Create a custom input handler by implementing the appropriate interface:
```csharp
using PixMetron.Controls.ImageViewport.Contracts.Input; 
using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
public class CustomGestureHandler : IMouseButtonHandler 
{ 
    public bool OnMouseDown(object sender, MouseButtonEventArgs e) 
    { 
        // Your custom logic return true; 
        // Return true if handled 
    }

    public bool OnMouseMove(object sender, MouseEventArgs e)
    {
        // Your custom logic
        return false;
    }

    public bool OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        // Your custom logic
        return true;
    }
}
```

### Dynamic Handler Configuration

Update handler options at runtime:
```csharp
// Assuming you have access to the options instance 
panZoomOptions.PanButton = PanButton.Right; 
panZoomOptions.ScaleFactor = 1.2; 
autoFitHandler.Mode = AutoFitMode.OnWindowGrow;
```

## Related Packages

This package is part of the PixMetron ImageViewport ecosystem:

- **PixMetron.Controls.ImageViewport**: Core viewport control and contracts
- **PixMetron.Controls.ImageViewport.Handlers**: Input handlers (this package)
- **PixMetron.Controls.ImageViewport.Surfaces**: Surface rendering system
- **PixMetron.Controls.ImageViewport.Defaults**: Default facade with attached properties

## License

This project is licensed under the Apache-2.0 License.

## Links

- **Repository**: [https://github.com/PixMetron/ImageViewport](https://github.com/PixMetron/ImageViewport)
- **Project**: [https://github.com/PixMetron](https://github.com/PixMetron)
- **NuGet Package**: [PixMetron.Controls.ImageViewport.Handlers](https://www.nuget.org/packages/PixMetron.Controls.ImageViewport.Handlers)

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests on GitHub.

---

Built with ❤️ for the WPF community by PixMetron