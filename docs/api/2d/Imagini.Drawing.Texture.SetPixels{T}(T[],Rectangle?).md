# SetPixels<T>(T[], Rectangle?)

**Method**

**Namespace:** [Imagini.Drawing](Imagini.Drawing.md)

**Declared in:** [Imagini.Drawing.Texture](Imagini.Drawing.Texture.md)

------



Use this function to update the given texture rectangle with new
pixel data.


## Syntax

```csharp
public void SetPixels<T>(
	T[] pixelData,
	Rectangle? rect
)
```

### Parameters

`rect`

Area to update, or null to update entire texture

`pixelData`

Pixel data array

## Remarks

This is a fairly slow function, intended for use with static textures that do not change often.
If the texture is intended to be updated often, it is preferred to
create the texture as streaming and use the locking functions.

------

[Back to index](index.md)