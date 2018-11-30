# SetPixels(byte[], Rectangle?)

**Method**

**Namespace:** [Imagini.Drawing](Imagini.Drawing.md)

**Declared in:** [Imagini.Drawing.Texture](Imagini.Drawing.Texture.md)

------



Use this function to update the given texture rectangle with new
pixel data.


## Syntax

```csharp
public void SetPixels(
	byte[] pixelData,
	Rectangle? rect
)
```

### Parameters

`pixelData`

Pixel data array

## Remarks

The pixel data must be in the pixel format of the texture.
This is a fairly slow function, intended for use with static textures that do not change often.
If the texture is intended to be updated often, it is preferred to
create the texture as streaming and use the locking functions.

------

[Back to index](index.md)