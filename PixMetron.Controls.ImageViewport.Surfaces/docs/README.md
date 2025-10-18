# PixMetron.Controls.ImageViewport.Surfaces

**Built-in Surface Renderers for ImageViewport**

A collection of ready-to-use surface renderer implementations for [PixMetron.Controls.ImageViewport](../../PixMetron.Controls.ImageViewport/docs/README.md), providing essential rendering capabilities for images and visual overlays in the layered viewport system.

---

## 📦 Installation
```powershell
Install-Package PixMetron.Controls.ImageViewport.Surfaces
```

or via .NET CLI:
```bash
dotnet add package PixMetron.Controls.ImageViewport.Surfaces
```

**Requirements:**
- .NET 8.0 or later
- Windows (WPF)

---

## ✨ Key Features

- **🖼️ Image Rendering**: High-performance image display with automatic size detection
- **📐 Primitive Shapes**: Built-in renderers for basic geometric shapes and graphics
- **🎨 Layered Architecture**: Seamlessly integrates with the viewport's multi-layer surface system
- **🔄 Transform Support**: Multiple transform modes (Follow, Fixed, etc.)
- **⚡ Optimized Performance**: Efficient WPF DrawingContext-based rendering
- **🛠️ Extensible Base**: Implement `ISurfaceRenderer` for custom surface types

---

## 🚀 Quick Start

### Basic Image Rendering
```csharp
using PixMetron.Controls.ImageViewport.Surfaces.Primitives; 
using System.Windows.Media.Imaging;
// Create an image surface renderer 
var imageSurface = new ImageSurfaceRenderer 
{ 
    Source = new BitmapImage(new Uri("image.png", UriKind.Relative)) 
};
// Add to viewport facade 
facade.AddSurface(imageSurface);
```

### Custom Positioned Image
```csharp
using PixMetron.Controls.ImageViewport.Contracts.Abstractions;
// Render image at specific pixel coordinates 
var imageSurface = new ImageSurfaceRenderer { 
    Source = myBitmapSource, 
    ImageRectPx = new PxRect(100, 50, 800, 600) // X, Y, Width, Height 
};
facade.AddSurface(imageSurface);
```

---

## 📚 Built-in Surface Renderers

### ImageSurfaceRenderer

Renders a WPF `ImageSource` in the viewport's image coordinate space.

**Namespace:** `PixMetron.Controls.ImageViewport.Surfaces.Primitives`

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Source` | `ImageSource?` | The image to render (BitmapSource, BitmapImage, etc.) |
| `ImageRectPx` | `PxRect` | Destination rectangle in pixel coordinates (auto-detected if not set) |
| `TransformMode` | `SurfaceMode` | Returns `SurfaceMode.Follow` (transforms with viewport) |

#### Features

- **Automatic Size Detection**: If `ImageRectPx` dimensions are ≤ 0 and `Source` is a `BitmapSource`, automatically uses pixel width/height
- **Null-Safe**: Gracefully handles null or invalid sources
- **Origin Anchored**: Renders at (0, 0) by default when dimensions are auto-detected

#### Example
```csharp
using PixMetron.Controls.ImageViewport.Surfaces.Primitives;
var renderer = new ImageSurfaceRenderer 
{ 
    Source = new BitmapImage(new Uri(@"C:\Images\photo.jpg")) // ImageRectPx is automatically set to (0, 0, pixelWidth, pixelHeight) 
};

// Or specify custom bounds 
var customRenderer = new ImageSurfaceRenderer { Source = myImage, ImageRectPx = new PxRect(x: 200, y: 100, width: 1024, height: 768) };
```

---

## 🔧 Core Concepts

### ISurfaceRenderer Interface

All surface renderers implement the `ISurfaceRenderer` interface:
```csharp
public interface ISurfaceRenderer
{
    SurfaceMode TransformMode { get; }
    void Render(DrawingContext dc, ViewportTransform transform);
}
```

**Transform Modes:**
- `SurfaceMode.Follow`: Surface transforms with viewport pan/zoom (standard for image content)
- `SurfaceMode.Fixed`: Surface remains fixed in screen coordinates (useful for UI overlays)

### Rendering Pipeline

1. Viewport calls `Render()` on each surface in Z-index order
2. Provides `DrawingContext` for WPF drawing operations
3. `SurfaceRenderContext` contains viewport state and transforms
4. Surfaces render using standard WPF drawing commands

---

## 🎯 Usage Scenarios

### Multi-Image Display
```csharp
using PixMetron.Controls.ImageViewport.Facade; 
using PixMetron.Controls.ImageViewport.Surfaces.Primitives;
var facade = new ImageViewportFacade();

// Background image 
var background = new ImageSurfaceRenderer { Source = LoadImage("background.png") }; facade.AddSurface(background); 
facade.SetSurfaceGroup(background, "Images");

// Overlay image at offset position 
var overlay = new ImageSurfaceRenderer 
{ 
    Source = LoadImage("overlay.png"), 
    ImageRectPx = new PxRect(500, 300, 400, 300) 
}; 
facade.AddSurface(overlay); 
facade.SetSurfaceGroup(overlay, "Images");
```

### Dynamic Image Updates
```csharp
var imageSurface = new ImageSurfaceRenderer(); 
facade.AddSurface(imageSurface);
// Update image dynamically 
void LoadNewImage(string path) 
{ 
    imageSurface.Source = new BitmapImage(new Uri(path)); 
    // Trigger viewport refresh 
    viewport.InvalidateVisual(); 
}
```

### Image Tiling
```csharp
// Create a grid of images 
for (int row = 0; row < 3; row++) 
{ 
    for (int col = 0; col < 3; col++) 
    { 
        var tile = new ImageSurfaceRenderer 
        { 
            Source = tileImage, 
            ImageRectPx = new PxRect( col * 512, row * 512, 512, 512 ) 
        }; 
        facade.AddSurface(tile); 
    } 
}
```

---

## 🛠️ Creating Custom Surface Renderers

Extend the surface system by implementing `ISurfaceRenderer`:
```csharp
using System.Windows; 
using System.Windows.Media; 
using PixMetron.Controls.ImageViewport.Contracts.Surfaces;
public class GridOverlaySurfaceRenderer : ISurfaceRenderer 
{ 
    public int GridSize { get; set; } = 50; 
    public Brush GridBrush { get; set; } = Brushes.LightGray;
    public SurfaceMode TransformMode => SurfaceMode.Follow;

    public void Render(DrawingContext dc, in SurfaceRenderContext ctx)
    {
        var pen = new Pen(GridBrush, 1.0);
        var bounds = ctx.ImageBounds; // Get visible image area
    
        // Draw vertical lines
        for (double x = 0; x < bounds.Width; x += GridSize)
        {
            dc.DrawLine(pen, 
                new Point(x, 0), 
                new Point(x, bounds.Height));
        }
    
        // Draw horizontal lines
        for (double y = 0; y < bounds.Height; y += GridSize)
        {
            dc.DrawLine(pen, 
                new Point(0, y), 
                new Point(bounds.Width, y));
        }
    }
}
```

---

## 📖 API Reference

### ImageSurfaceRenderer

#### Constructor
```csharp
public ImageSurfaceRenderer()
```

#### Properties
```csharp
public ImageSource? Source { get; set; }
public PxRect ImageRectPx { get; set; }
public SurfaceMode TransformMode { get; }
```

#### Methods
```csharp
public void Render(DrawingContext dc, in SurfaceRenderContext ctx)
```
Renders the image to the provided drawing context.

**Behavior:**
- If `ImageRectPx.Width` or `ImageRectPx.Height` ≤ 0 and `Source` is `BitmapSource`, auto-sets dimensions
- Returns early if `Source` is null or dimensions are invalid
- Draws image using `DrawingContext.DrawImage()`

---

## 🆚 Comparison with Other Packages

| Feature | Surfaces | Handlers | Facade |
|---------|----------|----------|--------|
| **Purpose** | Rendering content | Input processing | Integration layer |
| **Key Classes** | `ImageSurfaceRenderer` | `PanZoomHandler` | `ImageViewportFacade` |
| **Usage** | Display images/graphics | Handle mouse/keyboard | Manage viewport lifecycle |
| **Extensibility** | Implement `ISurfaceRenderer` | Implement `IInputRouter` | Implement `IViewportFacade` |

---

## 🔗 Integration with Facade

Surfaces are managed by the viewport's facade implementation:
```csharp
using PixMetron.Controls.ImageViewport.Facade; 
using PixMetron.Controls.ImageViewport.Surfaces.Primitives;
var facade = new ImageViewportFacade();

// Add image surface 
var image = new ImageSurfaceRenderer { Source = myImage }; 
facade.AddSurface(image);

// Organize with groups 
facade.SetSurfaceGroup(image, "ContentLayer");

// Control visibility 
facade.SetSurfaceVisible(image, false);

// Adjust Z-order 
facade.BringToFront(image);
```

---

## 📖 Related Documentation

- [Core Viewport Control](../../PixMetron.Controls.ImageViewport/docs/README.md)
- [Default Facade with XAML](../../PixMetron.Controls.ImageViewport.Defaults/docs/README.md)
- [Facade Implementation](../../PixMetron.Controls.ImageViewport.Facade/docs/README.md)
- [Input Handlers](../../PixMetron.Controls.ImageViewport.Handlers/docs/README.md)

---

## 🤝 Contributing

Contributions are welcome! Please visit the [repository](https://github.com/PixMetron/ImageViewport) for guidelines.

---

## 📄 License

This project is licensed under the Apache-2.0 License.