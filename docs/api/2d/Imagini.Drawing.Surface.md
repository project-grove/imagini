# Surface

**Class**

**Namespace:** [Imagini.Drawing](Imagini.Drawing.md)

**Base types:**

* [IDisposable](#.md)
* [Resource](#.md)


**Declared in:** [Imagini.Drawing](Imagini.Drawing.md)

------



Defines a surface which stores it's pixel data in
the RAM.


## Members

### Property
* [PixelInfo](Imagini.Drawing.Surface.PixelInfo.md)
* [Width](Imagini.Drawing.Texture.Width.md)
* [Height](Imagini.Drawing.Texture.Height.md)
* [PixelCount](Imagini.Drawing.Texture.PixelCount.md)
* [Stride](Imagini.Drawing.Surface.Stride.md)
* [SizeInBytes](Imagini.Drawing.Surface.SizeInBytes.md)
* [MustBeLocked](Imagini.Drawing.Surface.MustBeLocked.md)
* [Locked](Imagini.Drawing.Texture.Locked.md)
* [RLEEnabled](Imagini.Drawing.Surface.RLEEnabled.md)
* [ClipRect](Imagini.Drawing.Surface.ClipRect.md)
* [ColorMod](Imagini.Drawing.Texture.ColorMod.md)
* [AlphaMod](Imagini.Drawing.Texture.AlphaMod.md)
* [BlendMode](Imagini.Drawing.BlendMode.md)
* [ColorKey](Imagini.Drawing.Surface.ColorKey.md)

### Method
* [BlitTo(Surface, Rectangle?, Rectangle?)](Imagini.Drawing.Surface.BlitTo(Surface,Rectangle?,Rectangle?).md)
* [ConvertTo(PixelFormat)](Imagini.Drawing.Surface.ConvertTo(PixelFormat).md)
* [ConvertTo(PixelFormatInfo)](Imagini.Drawing.Surface.ConvertTo(PixelFormatInfo).md)
* [Create(int, int, int, int, int, int, int)](Imagini.Drawing.Surface.Create(int,int,int,int,int,int,int).md)
* [Create(int, int, PixelFormat)](Imagini.Drawing.Surface.Create(int,int,PixelFormat).md)
* [CreateFrom(byte[], int, int, int, int, int, int, int, int)](Imagini.Drawing.Surface.CreateFrom(byte[],int,int,int,int,int,int,int,int).md)
* [CreateFrom(byte[], int, int, PixelFormat)](Imagini.Drawing.Surface.CreateFrom(byte[],int,int,PixelFormat).md)
* [Dispose()](Imagini.Drawing.Texture.Dispose().md)
* [Fill(Color, Rectangle?)](Imagini.Drawing.Surface.Fill(Color,Rectangle?).md)
* [Fill(Color, Rectangle[])](Imagini.Drawing.Surface.Fill(Color,Rectangle[]).md)
* [Lock()](Imagini.Drawing.Surface.Lock().md)
* [OptimizeFor(PixelFormat)](Imagini.Drawing.Surface.OptimizeFor(PixelFormat).md)
* [OptimizeFor(PixelFormatInfo)](Imagini.Drawing.Surface.OptimizeFor(PixelFormatInfo).md)
* [ReadPixels(byte[])](Imagini.Drawing.Surface.ReadPixels(byte[]).md)
* [ReadPixels<T>(T[])](Imagini.Drawing.Surface.ReadPixels{T}(T[]).md)
* [SetPixelData(byte[])](Imagini.Drawing.Surface.SetPixelData(byte[]).md)
* [SetPixelData<T>(T[])](Imagini.Drawing.Surface.SetPixelData{T}(T[]).md)
* [SetRLEAcceleration(bool)](Imagini.Drawing.Surface.SetRLEAcceleration(bool).md)
* [Unlock()](Imagini.Drawing.Texture.Unlock().md)

------

[Back to index](index.md)