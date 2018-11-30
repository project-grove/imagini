# ReadPixels(ColorRGB888[], Rectangle?)

**Method**

**Namespace:** [Imagini.Drawing](Imagini.Drawing.md)

**Declared in:** [Imagini.Drawing.Graphics](Imagini.Drawing.Graphics.md)

------



Reads the pixel data from current render target to the specified pixel data buffer.


## Syntax

```csharp
public void ReadPixels(
	ColorRGB888[] pixelData,
	Rectangle? rectangle
)
```

### Parameters

`pixelData`



`rect`

Rectangle to read data from, or null to read entire texture

## Remarks
This function is pretty slow and shouldn't be used often.
------

[Back to index](index.md)