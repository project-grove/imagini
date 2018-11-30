# Save(Graphics, Action<Image<Rgba32>>)

**Method**

**Namespace:** [Imagini.ImageSharp](Imagini.ImageSharp.md)

**Declared in:** [Imagini.ImageSharp.GraphicsExtensions](Imagini.ImageSharp.GraphicsExtensions.md)

------



Saves the graphics using the specified save action.


## Syntax

```csharp
public static void Save(
	Graphics graphics,
	Action<Image<Rgba32>> onSave
)
```

## Examples


graphics.Save(image => image.SaveAsJpeg("file.jpg"))


------

[Back to index](index.md)