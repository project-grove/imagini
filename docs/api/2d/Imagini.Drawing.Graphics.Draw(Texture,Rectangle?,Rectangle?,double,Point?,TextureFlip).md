# Draw(Texture, Rectangle?, Rectangle?, double, Point?, TextureFlip)

**Method**

**Namespace:** [Imagini.Drawing](Imagini.Drawing.md)

**Declared in:** [Imagini.Drawing.Graphics](Imagini.Drawing.Graphics.md)

------



Copies a portion of the texture to the current rendering target.


## Syntax

```csharp
public void Draw(
	Texture texture,
	Rectangle? srcRect,
	Rectangle? dstRect,
	double angle,
	Point? center,
	TextureFlip flip
)
```

### Parameters

`texture`

Texture to copy

`srcRect`

Source rectangle (null for copying whole texture)

`dstRect`

Destination rectangle (null to fill the entire render target)

`angle`


Angle in degrees that indicates the rotation that will be applied
to dstRect, rotating it in a clockwise direction


`center`


Point around which dstRect will be rotated (if null, rotation will be
done around dstRect's center)


`flip`


Flipping actions that should be performed on the texture


------

[Back to index](index.md)