# CreateTextureFromSurface(Surface, TextureScalingQuality)

**Method**

**Namespace:** [Imagini.Drawing](Imagini.Drawing.md)

**Declared in:** [Imagini.Drawing.Graphics](Imagini.Drawing.Graphics.md)

------



Creates a static texture from an existing surface.


## Syntax

```csharp
public Texture CreateTextureFromSurface(
	Surface surface,
	TextureScalingQuality quality
)
```

### Parameters

`surface`

Surface to create texture from

`quality`

Filtering quality when texture is scaled

## Remarks

The surface is not modified or disposed by this function.
[TextureAccess](Imagini.Drawing.TextureAccess.md) is static.
The pixel format of the created texture may be different from the
pixel format of the surface.

------

[Back to index](index.md)