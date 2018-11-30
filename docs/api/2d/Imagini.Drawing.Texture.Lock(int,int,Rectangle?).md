# Lock(int, int, Rectangle?)

**Method**

**Namespace:** [Imagini.Drawing](Imagini.Drawing.md)

**Declared in:** [Imagini.Drawing.Texture](Imagini.Drawing.Texture.md)

------



Locks a portion or the whole texture for write-only access and
returns a pointer to it.


## Syntax

```csharp
public IntPtr Lock(
	int length,
	int stride,
	Rectangle? rect
)
```

### Parameters

`rect`

Rectangle to be locked, or null to lock entire texture

`length`

Length of the byte array

`stride`

Stride of the pixel data (bytes per row)

------

[Back to index](index.md)