# PixMetron.Controls.ImageViewport.Defaults

**Default Facade Implementation with Attached Properties for Simplified XAML Configuration**

This is a companion package for [PixMetron.Controls.ImageViewport](../../PixMetron.Controls.ImageViewport/docs/README.md), providing an out-of-the-box default Facade implementation and WPF attached properties for rapid image viewer configuration without writing code.

---

## 📦 Installation

```bash
Install-Package PixMetron.Controls.ImageViewport.Defaults
```
or via .NET CLI:
```bash
dotnet add package PixMetron.Controls.ImageViewport.Defaults
```

**Requirements:**
- .NET 8.0 or later
- Windows (WPF)

---

## ✨ Key Features

- **🎯 Zero-Code Configuration**: Configure Viewport directly through XAML attached properties
- **🚀 Default Facade**: Built-in `DefaultImageViewportFacade` automatically manages image rendering, pan/zoom, and auto-fit
- **🎨 Automatic Image Rendering**: Integrated `ImageSurfaceRenderer` handles image display
- **🖱️ Built-in Input Handling**: Integrated `PanZoomHandler` processes mouse/keyboard interactions
- **📐 Smart Auto-Fit**: Supports three auto-fit modes (Always/OnLoad/Never)

---

## 🚀 Quick Start

### XAML Declarative Configuration (Recommended)
```xml
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" 
  xmlns:iv="clr-namespace:PixMetron.Controls.ImageViewport;assembly=PixMetron.Controls.ImageViewport" 
  xmlns:ivd="clr-namespace:PixMetron.Controls.ImageViewport.Defaults;assembly=PixMetron.Controls.ImageViewport.Defaults">
<Grid>
    <iv:ImageViewport 
        x:Name="Viewport"
        ivd:Viewport.Source="{Binding ImageSource}"
        ivd:Viewport.AutoFitMode="Always"
        ivd:Viewport.PanButton="Middle"
        ivd:Viewport.WheelPivot="Mouse"
        ivd:Viewport.ScaleFactor="1.1"
        ivd:Viewport.MinScale="0.02"
        ivd:Viewport.MaxScale="40" />
</Grid>
</Window>
```
### Code-Behind Configuration
```csharp
using PixMetron.Controls.ImageViewport;
using PixMetron.Controls.ImageViewport.Defaults;

// Option 1: Using attached properties 
var viewport = new ImageViewport(); 
Viewport.SetSource(viewport, myImageSource); 
Viewport.SetAutoFitMode(viewport, AutoFitMode.Always); 
Viewport.SetPanButton(viewport, PanButton.Middle);

// Option 2: Directly creating DefaultImageViewportFacade 
var options = new PanZoomOptions { 
    PanButton = PanButton.Middle, 
    WheelPivot = ZoomPivotMode.Mouse, 
    ScaleFactor = 1.1, 
    MinScale = 0.02, 
    MaxScale = 40.0 }; 
viewport.Facade = new DefaultImageViewportFacade(viewport, options, myImageSource);
```


---

## 📖 Attached Properties Reference

### `Viewport` Attached Properties Class

All attached properties are defined in the `PixMetron.Controls.ImageViewport.Defaults.Viewport` class:

#### **Source** (ImageSource)
Sets the image source to display.

```csharp
ivd:Viewport.Source="{Binding ImageSource}"
```


---

#### **AutoFitMode** (AutoFitMode)
Auto-fit mode. Default: `Always`

| Value | Description |
|-------|-------------|
| `Always` | Auto-fit on image load and window resize |
| `OnLoad` | Auto-fit only when image is first loaded |
| `Never` | Manual control only, no auto-fit |


```csharp
ivd:Viewport.AutoFitMode="Always"
```

---

#### **PanButton** (PanButton)
Mouse button for panning. Default: `Middle`

| Value | Description |
|-------|-------------|
| `Left` | Drag with left button to pan |
| `Middle` | Drag with middle button to pan (recommended) |
| `Right` | Drag with right button to pan |
| `None` | Disable mouse panning |

```csharp
ivd:Viewport.PanButton="Middle"
```

---

#### **WheelPivot** (ZoomPivotMode)
Wheel zoom pivot point. Default: `Mouse`

| Value | Description |
|-------|-------------|
| `Mouse` | Zoom centered at mouse position (recommended) |
| `Center` | Zoom centered at viewport center |
| `Custom` | Custom zoom center (requires CustomPivotWindowPxProvider) |

```csharp
ivd:Viewport.WheelPivot="Mouse"
```

---

#### **ScaleFactor** (double)
Scale factor per wheel notch. Default: `1.1`

```csharp
ivd:Viewport.ScaleFactor="1.1"
```

---

#### **MinScale / MaxScale** (double)
Minimum/maximum zoom scale. Default: `0.02` / `40.0`
```csharp
ivd:Viewport.MinScale="0.01"
ivd:Viewport.MaxScale="1000"
```

---

#### **RequireCtrlForWheelZoom** (bool)
Whether Ctrl key is required for wheel zoom. Default: `False`

```csharp
ivd:Viewport.RequireCtrlForWheelZoom="False"
```

---

#### **UseImageCoordinateZoom** (bool)
Whether to use image coordinate space for zoom calculations. Default: `True`

```csharp
ivd:Viewport.UseImageCoordinateZoom="True"
```

---

#### **CustomPivotWindowPxProvider** (Func<PxPoint>)
Custom zoom pivot provider (only effective when `WheelPivot="Custom"`)

```csharp
ivd:Viewport.CustomPivotWindowPxProvider="{Binding GetCustomPivot}"
```

---

## 🏗️ DefaultImageViewportFacade

`DefaultImageViewportFacade` is the core default implementation that automatically integrates the following components:

### Component Architecture

```plaintext
DefaultImageViewportFacade 
├── IViewportService (BuiltInViewportService) 
│   └── Manages viewport state and transformations 
├── Surfaces   
│   └── ImageSurfaceRenderer       
├── Renders image content 
└── InputRouter  
    ├── PanZoomHandler       
    │   └── Handles pan/zoom input
    └── AutoFitHandler 
        └── Handles auto-fit logic
```

### Constructor

```csharp
public DefaultImageViewportFacade( 
    ImageViewport viewport,      // Associated ImageViewport control 
    IPanZoomOptions options,     // Pan/zoom configuration 
    ImageSource? source          // Initial image source (optional) 
    )
```

### Usage Example

```csharp
// Create configuration 
var options = new PanZoomOptions 
{ 
    PanButton = PanButton.Middle, 
    WheelPivot = ZoomPivotMode.Mouse, 
    ScaleFactor = 1.1, 
    MinScale = 0.02, 
    MaxScale = 40.0, 
    RequireCtrlForWheelZoom = false, 
    UseImageCoordinateZoom = true 
};

// Create Facade 
var facade = new DefaultImageViewportFacade(viewport, options, imageSource);

// Set auto-fit mode 
facade.AutoFitMode = AutoFitMode.OnLoad;

// Apply to Viewport 
viewport.Facade = facade;
```

---

## 🎯 Common Use Cases

### Scenario 1: Simple Image Viewer

```xml
<iv:ImageViewport 
  ivd:Viewport.Source="{Binding CurrentImage}" 
  ivd:Viewport.AutoFitMode="Always" 
  ivd:Viewport.PanButton="Middle" 
  ivd:Viewport.WheelPivot="Mouse" />
```

### Scenario 2: Zoom Control with Ctrl Key Required

```xml
<iv:ImageViewport 
  ivd:Viewport.Source="{Binding CurrentImage}" 
  ivd:Viewport.RequireCtrlForWheelZoom="True" 
  ivd:Viewport.PanButton="Left" />
```

### Scenario 3: Precision Image Measurement Tool

```xml
<iv:ImageViewport 
  ivd:Viewport.Source="{Binding MeasurementImage}" 
  ivd:Viewport.AutoFitMode="OnLoad" 
  ivd:Viewport.MinScale="0.1" 
  ivd:Viewport.MaxScale="200.0" 
  ivd:Viewport.UseImageCoordinateZoom="True" />
```

### Scenario 4: Runtime Dynamic Configuration Switching

```csharp
// Switch to edit mode: left button pan, disable auto-fit 
Viewport.SetPanButton(viewport, PanButton.Left); 
Viewport.SetAutoFitMode(viewport, AutoFitMode.Disabled);

// Switch to view mode: middle button pan, enable auto-fit 
Viewport.SetPanButton(viewport, PanButton.Middle); 
Viewport.SetAutoFitMode(viewport, AutoFitMode.Always);
```

---

## 🔧 Advanced Configuration

### Accessing the Facade

```csharp
// Get the default facade instance 
if (viewport.Facade is DefaultImageViewportFacade defaultFacade) 
{ 
    // Access image layer 
    var imageLayer = defaultFacade.ImageLayer;
    // Access auto-fit handler
    var autoFit = defaultFacade.AutoFit;
    autoFit.Mode = AutoFitMode.OnLoad;
    
    // Access viewport service
    var service = defaultFacade.Service;
    var currentState = service.Current;
}
```

### Changing Image Source at Runtime

```csharp
// Method 1: Using attached property 
Viewport.SetSource(viewport, newImageSource);

// Method 2: Through facade 
if (viewport.Facade is DefaultImageViewportFacade facade) 
{ 
facade.ImageLayer.Source = newImageSource; 
}
```

### Programmatic Pan and Zoom

```csharp
// Get viewport service 
var service = (viewport.Facade as DefaultImageViewportFacade)?.Service; 
if (service != null) { 
    // Pan to specific position 
    service.Pan(new PxVector(100, 50));

    // Zoom to specific scale
    service.ZoomTo(2.0, new PxPoint(viewport.ActualWidth / 2, viewport.ActualHeight / 2));
    
    // Reset viewport
    service.Reset();
}
```

---

## 🔗 Dependencies

This package depends on the following components:

- **PixMetron.Controls.ImageViewport** - Core Viewport control
- **PixMetron.Controls.ImageViewport.Handlers** - Input handlers
- **PixMetron.Controls.ImageViewport.Surfaces** - Image renderers

---

## 📚 Additional Resources

- [Core ImageViewport Documentation](../../PixMetron.Controls.ImageViewport/docs/README.md)
- [Custom Facade Guide](../../docs/CustomFacade.md)
- [Sample Applications](../../samples/DemoApp/)

---

## 🐛 Troubleshooting

### Issue: Image Not Displaying

**Solution**: Ensure the `Source` property is set correctly and the image source is valid.

```csharp
// Verify image source 
if (Viewport.GetSource(viewport) == null) 
{ 
    Viewport.SetSource(viewport, new BitmapImage(new Uri("path/to/image.jpg"))); 
}
```


### Issue: Pan/Zoom Not Working

**Solution**: Check that attached properties are set correctly and no custom facade is overriding defaults.
```csharp
// Verify pan button configuration 
var panButton = Viewport.GetPanButton(viewport); 
Console.WriteLine($"Pan button is set to: {panButton}");
```

### Issue: Auto-Fit Not Triggering

**Solution**: Ensure `AutoFitMode` is set to `Always` or `OnLoad`, and the image source has valid dimensions.

```csharp
ivd:Viewport.AutoFitMode=AutoFitMode.Always
```

---

## 🤝 Contributing

Contributions are welcome! Please submit Issues and Pull Requests.

- **GitHub**: [https://github.com/PixMetron/ImageViewport](https://github.com/PixMetron/ImageViewport)
- **Issues**: [https://github.com/PixMetron/ImageViewport/issues](https://github.com/PixMetron/ImageViewport/issues)

---

## 📄 License

This project is licensed under the **Apache-2.0** License - see the [LICENSE](../../LICENSE) file for details.

---

## 🏢 About PixMetron

**PixMetron** is a high-performance image processing toolkit designed for professional WPF applications.

- **Organization**: [https://github.com/PixMetron](https://github.com/PixMetron)
- **Repository**: [https://github.com/PixMetron/ImageViewport](https://github.com/PixMetron/ImageViewport)

---

**Built with ❤️ for the WPF community**