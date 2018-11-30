# Draw(Texture, Rectangle?, Rectangle?)

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
	Rectangle? dstRect
)
```

### Parameters

`texture`

Texture to copy

`srcRect`

Source rectangle (null for copying whole texture)

`dstRect`

Destination rectangle (null to fill the entire render target)

------

[Back to index](index.md)