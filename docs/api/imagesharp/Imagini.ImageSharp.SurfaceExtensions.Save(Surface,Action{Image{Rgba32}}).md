# Save(Surface, Action<Image<Rgba32>>)

**Method**

**Namespace:** [Imagini.ImageSharp](Imagini.ImageSharp.md)

**Declared in:** [Imagini.ImageSharp.SurfaceExtensions](Imagini.ImageSharp.SurfaceExtensions.md)

------



Saves the surface using the specified save action.


## Syntax

```csharp
public static void Save(
	Surface surface,
	Action<Image<Rgba32>> onSave
)
```

## Examples


surface.Save(image => image.SaveAsJpeg("file.jpg"))


------

[Back to index](index.md)