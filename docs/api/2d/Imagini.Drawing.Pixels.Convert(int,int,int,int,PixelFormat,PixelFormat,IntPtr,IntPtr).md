# Convert(int, int, int, int, PixelFormat, PixelFormat, IntPtr, IntPtr)

**Method**

**Namespace:** [Imagini.Drawing](Imagini.Drawing.md)

**Declared in:** [Imagini.Drawing.Pixels](Imagini.Drawing.Pixels.md)

------



Copies a block of pixels of one format to another format.


## Syntax

```csharp
public static void Convert(
	int width,
	int height,
	int srcStride,
	int dstStride,
	PixelFormat srcFormat,
	PixelFormat dstFormat,
	IntPtr src,
	IntPtr dst
)
```

### Parameters

`width`

The width of the block to copy, in pixels

`height`

The height of the block to copy, in pixels

`srcStride`

The stride (pitch) of the block to copy

`dstStride`

The stride (pitch) of the destination pixels

`srcFormat`

Source pixel format

`dstFormat`

Destination pixel format

`src`

Source pixel data

`dst`

Destination pixel data

------

[Back to index](index.md)