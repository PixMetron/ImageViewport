# PixMetron.Controls.ImageViewport.Facade

**Extensible Facade Implementation for Advanced Viewport Customization**

A flexible, composable facade for [PixMetron.Controls.ImageViewport](../../PixMetron.Controls.ImageViewport/docs/README.md) that provides advanced control over surface rendering, input routing, and viewport behavior through the `IEditableViewportFacade` interface.

---

## 📦 Installation

```powershell
Install-Package PixMetron.Controls.ImageViewport.Facade
```

or via .NET CLI:

```bash
dotnet add package PixMetron.Controls.ImageViewport.Facade
```

**Requirements:**
- .NET 8.0 or later
- Windows (WPF)

---

## ✨ Key Features

- **🎨 Dynamic Surface Management**: Add, remove, reorder, and configure surface renderers at runtime
- **🔧 Extensible Architecture**: Implements `IEditableViewportFacade` for full programmatic control
- **📐 Z-Index Ordering**: Precise control over layer rendering order with Z-index support
- **👥 Group Management**: Organize surfaces into logical groups for batch operations
- **🎯 Input Router Composition**: Flexible input routing with priority-based handling
- **🚀 Batch Updates**: Optimize performance with batch update operations
- **🔄 Transform Caching**: Efficient coordinate transform caching with version tracking

---

## 🚀 Quick Start

### Basic Setup
```csharp
using PixMetron.Controls.ImageViewport; 
using PixMetron.Controls.ImageViewport.Facade; 
using PixMetron.Controls.ImageViewport.Surfaces; 
using PixMetron.Controls.ImageViewport.Handlers;

// Create facade with default service 
var facade = new ImageViewportFacade();

// Add an image surface 
var imageSurface = new ImageSurfaceRenderer { Source = myBitmapSource }; facade.AddSurface(imageSurface);

// Add pan/zoom handler 
var panZoomHandler = new PanZoomHandler(facade.Service); 
facade.PrependInputRouter(panZoomHandler);

// Attach to viewport 
viewport.Facade = facade;
```

### Advanced Configuration
```csharp
using PixMetron.Controls.ImageViewport.Runtime.Services; 
using PixMetron.Controls.ImageViewport.Runtime.Transforms;

// Create facade with custom components 
var service = new BuiltInViewportService(); 
var facade = new ImageViewportFacade( service: service, transformsFactory: info => new BuiltInViewportTransforms(info), contextMenu: myContextMenuProvider, initialRouter: myCustomRouter );

// Configure surface with metadata 
var surface = new ImageSurfaceRenderer { Source = image }; 
facade.AddSurface(surface); 
facade.SetSurfaceGroup(surface, "ImageLayer");

// Add overlay with higher Z-index 
var overlay = new MyCustomOverlay(); 
facade.AddSurface(overlay); 
facade.BringToFront(overlay);
```

---

## 📚 Core Concepts

### Surface Management

`ImageViewportFacade` provides comprehensive surface lifecycle management:

```csharp
// Add surfaces 
facade.AddSurface(renderer); 

// Insert at specific index
facade.InsertSurface(0, renderer); 

// Remove surfaces 
facade.RemoveSurface(renderer);

// Reorder surfaces 
facade.BringToFront(renderer); 
facade.SendToBack(renderer); 
facade.MoveTo(renderer, targetIndex);

// Control visibility 
facade.SetSurfaceVisible(renderer, false);

// Access mutable collection IList<ISurfaceRenderer> 
surfaces = facade.SurfacesMutable;
```

### Group-Based Operations

Organize surfaces into logical groups for efficient batch operations:
```csharp
// Assign surfaces to groups 
facade.SetSurfaceGroup(imageSurface, "Content"); 
facade.SetSurfaceGroup(overlayA, "Overlays"); 
facade.SetSurfaceGroup(overlayB, "Overlays");

// Batch operations 
facade.HideGroup("Overlays");  

// Hide all surfaces in group 
facade.ShowGroup("Overlays");   

// Show all surfaces in group 
facade.ClearGroup("Overlays");  

// Remove all surfaces in group
```


### Input Router Management

Compose multiple input handlers with priority-based routing:
```csharp
// Replace entire router 
facade.SetInputRouter(newRouter);

// Add router at the beginning (higher priority) 
facade.PrependInputRouter(highPriorityRouter);

// Add router at the end (lower priority) 
facade.AppendInputRouter(lowPriorityRouter);

// Routers implementing IInputPrioritizable are automatically ordered by priority 
// Higher priority values are processed first
```

### Batch Updates

Optimize performance by deferring cache rebuilds:
```csharp
using (facade.BatchUpdate()) 
{ 
    // Multiple operations within batch 
    facade.AddSurface(surface1); 
    facade.AddSurface(surface2); 
    facade.SetSurfaceVisible(surface3, false); 
    facade.BringToFront(surface4);
    // Cache rebuild happens only once when batch disposes
}
```

---

## 🔧 API Reference

### Constructor
```csharp
public ImageViewportFacade( 
    IViewportService? service = null, 
    Func<ViewportInfo, IViewportTransforms>? transformsFactory = null, 
    IContextMenuProvider? contextMenu = null, 
    IInputRouter? initialRouter = null)
```

**Parameters:**
- `service`: Viewport state service (defaults to `BuiltInViewportService`)
- `transformsFactory`: Custom transform factory for coordinate conversions
- `contextMenu`: Right-click context menu provider
- `initialRouter`: Initial input router (can be extended with Prepend/Append)

### IEditableViewportFacade Methods

#### Surface Management
- `bool AddSurface(ISurfaceRenderer surface)` - Add surface at highest Z-index
- `bool InsertSurface(int index, ISurfaceRenderer surface)` - Insert at specific index
- `bool RemoveSurface(ISurfaceRenderer surface)` - Remove surface
- `bool SetSurfaceVisible(ISurfaceRenderer surface, bool visible)` - Toggle visibility
- `bool SetSurfaceGroup(ISurfaceRenderer surface, string? group)` - Assign to group
- `bool BringToFront(ISurfaceRenderer surface)` - Move to highest Z-index
- `bool SendToBack(ISurfaceRenderer surface)` - Move to lowest Z-index
- `bool MoveTo(ISurfaceRenderer surface, int index)` - Move to specific index

#### Group Operations
- `int HideGroup(string group)` - Hide all surfaces in group
- `int ShowGroup(string group)` - Show all surfaces in group
- `int ClearGroup(string group)` - Remove all surfaces in group

#### Input & Context Menu
- `void SetInputRouter(IInputRouter? router)` - Replace input router
- `void PrependInputRouter(IInputRouter router)` - Add router with higher priority
- `void AppendInputRouter(IInputRouter router)` - Add router with lower priority
- `void SetContextMenu(IContextMenuProvider? provider)` - Set context menu provider

#### Batch Operations
- `IDisposable BatchUpdate()` - Create batch update scope

---

## 🎯 Usage Scenarios

### Multi-Layer Image Viewer
```csharp
var facade = new ImageViewportFacade();

// Base image layer 
var baseImage = new ImageSurfaceRenderer { Source = backgroundImage }; 
facade.AddSurface(baseImage); 
facade.SetSurfaceGroup(baseImage, "Base");

// Annotation layer 
var annotations = new AnnotationSurfaceRenderer(); 
facade.AddSurface(annotations); 
facade.SetSurfaceGroup(annotations, "Annotations");

// Overlay layer 
var overlay = new GridOverlaySurfaceRenderer(); 
facade.AddSurface(overlay); 
facade.SetSurfaceGroup(overlay, "Overlays");

// Toggle overlays 
facade.HideGroup("Overlays");
```

### Custom Input Handling
```csharp
var facade = new ImageViewportFacade();

// Add pan/zoom as base handler 
var panZoom = new PanZoomHandler(facade.Service); 
facade.AppendInputRouter(panZoom);

// Add custom annotation handler with higher priority 
var annotationHandler = new AnnotationInputHandler(); 
facade.PrependInputRouter(annotationHandler);

// Annotation handler processes input first; 
// if not handled, falls through to pan/zoom
```

### Dynamic Surface Management
```csharp
var facade = new ImageViewportFacade();

// Load initial content 
facade.AddSurface(backgroundSurface);

// Add/remove surfaces based on user actions 
void OnAddLayer() 
{ 
    var newLayer = CreateLayerSurface(); 
    facade.AddSurface(newLayer); 
    facade.SetSurfaceGroup(newLayer, "UserLayers"); 
}

void OnClearAllLayers() 
{ 
    facade.ClearGroup("UserLayers"); 
}

void OnReorderLayer(ISurfaceRenderer surface, int newIndex) 
{ 
    facade.MoveTo(surface, newIndex); 
}
```

---

## 🆚 Comparison with Other Packages

| Feature | Facade | Defaults |
|---------|--------|----------|
| **Target Audience** | Advanced customization | Quick setup |
| **Configuration** | Programmatic API | XAML attached properties |
| **Surface Management** | Full runtime control | Auto-managed |
| **Input Routing** | Composable routers | Built-in pan/zoom |
| **Learning Curve** | Moderate | Low |
| **Use Case** | Custom viewers, complex UIs | Standard image display |

---

## 📖 Related Documentation

- [Core Viewport Control](../../PixMetron.Controls.ImageViewport/docs/README.md)
- [Default Facade with XAML](../../PixMetron.Controls.ImageViewport.Defaults/docs/README.md)
- [Surface Renderers](../../PixMetron.Controls.ImageViewport.Surfaces/docs/README.md)
- [Input Handlers](../../PixMetron.Controls.ImageViewport.Handlers/docs/README.md)

---

## 🤝 Contributing

Contributions are welcome! Please visit the [repository](https://github.com/PixMetron/ImageViewport) for guidelines.

---

## 📄 License

This project is licensed under the Apache-2.0 License.