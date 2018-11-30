# CreateFrom(byte[], int, int, int, int, int, int, int, int)

**Method**

**Namespace:** [Imagini.Drawing](Imagini.Drawing.md)

**Declared in:** [Imagini.Drawing.Surface](Imagini.Drawing.Surface.md)

------



Creates a new surface from existing data. Data is copied.


## Syntax

```csharp
public static Surface CreateFrom(
	byte[] data,
	int width,
	int height,
	int stride,
	int depth,
	int Rmask,
	int Gmask,
	int Bmask,
	int Amask
)
```

### Parameters

`data`

The pixel data to create surface from

`width`

Surface width

`height`

Surface height

`stride`

Length of pixel row in bytes. RGBA = 4 * width

`depth`

Surface depth in bits (defaults to 32)

## Remarks

The mask parameters are the bitmasks used to extract that
color from a pixel. For instance, Rmask being FF000000 means
the red data is stored in the most significant byte. Uzing zeros for
the RGB masks sets a default value, based on the depth. However,
using zero for the Amask results in an Amask of 0.

------

[Back to index](index.md)