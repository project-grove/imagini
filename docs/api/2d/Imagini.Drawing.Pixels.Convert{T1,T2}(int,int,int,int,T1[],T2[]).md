# Convert<T1, T2>(int, int, int, int, T1[], T2[])

**Method**

**Namespace:** [Imagini.Drawing](Imagini.Drawing.md)

**Declared in:** [Imagini.Drawing.Pixels](Imagini.Drawing.Pixels.md)

------



Copies a block of pixels of one format to another format.


## Syntax

```csharp
public static void Convert<T1, T2>(
	int width,
	int height,
	int srcStride,
	int dstStride,
	T1[] src,
	T2[] dst
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

`src`

Source pixel data

`dst`

Destination pixel data

### Returns



------

[Back to index](index.md)