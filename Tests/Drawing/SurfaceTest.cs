using System.Drawing;
using System.Linq;
using FluentAssertions;
using Imagini;
using Imagini.Drawing;
using Xunit;

namespace Tests.Drawing
{
#if !HEADLESS
    [DisplayTestMethodName]
    public class SurfaceTest 
    {
        Color TestColor = Color.FromArgb(unchecked((int)0xDEADBEEF));

        [Fact]
        public void ShouldCreateRGBASurfaceWithDefaultParameters()
        {
            
            var surface = Surface.Create(200, 100);
            surface.Width.Should().Be(200);
            surface.Height.Should().Be(100);
            surface.PixelInfo.BitsPerPixel.Should().Be(32);

            surface.Dispose();
        }

        [Fact]
        public void ShouldCreateSurfaceWhenMaskIsSpecified()
        {
            
            var surface = Surface.Create(200, 100, depth: 32,
                Rmask: 0x000000FF,
                Gmask: 0x0000FF00,
                Bmask: 0x00FF0000,
                Amask: unchecked((int)0xFF000000));
            surface.Width.Should().Be(200);
            surface.Height.Should().Be(100);
            surface.PixelInfo.BitsPerPixel.Should().Be(32);

            surface.Dispose();
        }

        [Fact]
        public void ShouldCreateSurfaceWithTheSpecifiedFormat()
        {
            
            var targetFormat = PixelFormat.Format_ARGB8888;
            var surface = Surface.Create(200, 100, targetFormat);
            surface.Width.Should().Be(200);
            surface.Height.Should().Be(100);
            surface.PixelInfo.Format.Should().Be(targetFormat);

            surface.Dispose();
        }

        [Fact]
        public void ShouldFailWhenInvalidSurfaceParametersArePassed()
        {
            
            Assert.Throws<ImaginiException>(() =>
            {
                var invalidSurface = Surface.Create(0, 0, depth: 0);
            });
        }

        [Fact]
        public void ShouldConvertToSpecifiedFormat()
        {
            
            var width = 10;
            var height = 10;
            var source = Surface.Create(width, height, PixelFormat.Format_RGBA8888);
            var pixels = Enumerable.Repeat<ColorRGBA8888>(
                new ColorRGBA8888(TestColor), source.PixelCount
            ).ToArray();
            // Set the pixel data and check it
            source.SetPixelData(ref pixels);
            var currentPixels = new ColorRGBA8888[source.PixelCount];
            source.ReadPixels(ref currentPixels);
            currentPixels.Should().BeEquivalentTo(pixels);
            // Convert the surface and check it again
            var converted = source.ConvertTo(PixelFormat.Format_ARGB8888);
            source.ReadPixels(ref currentPixels);
            currentPixels.Should().BeEquivalentTo(pixels);

            source.Dispose();
            converted.Dispose();
        }

        [Fact]
        public void ShouldOptimizeForSpecifiedFormat()
        {
            
            var targetFormat = PixelFormat.Format_BGRA8888;
            var source = Surface.Create(200, 100);
            source.PixelInfo.Format.Should().NotBe(targetFormat);
            var destination = source.OptimizeFor(targetFormat);
            destination.PixelInfo.Format.Should().Be(targetFormat);

            source.Dispose();
            destination.Dispose();
        }

        [Fact]
        public void ShouldToggleRLEAndSupportLocking()
        {
            
            var surface = Surface.Create(200, 100);
            surface.RLEEnabled.Should().BeFalse();
            surface.SetRLEAcceleration(enable: true);
            surface.RLEEnabled.Should().BeTrue();

            surface.Locked.Should().BeFalse();
            surface.Lock();
            surface.Locked.Should().BeTrue();
            surface.Unlock();
            surface.Locked.Should().BeFalse();

            surface.SetRLEAcceleration(enable: false);
            surface.RLEEnabled.Should().BeFalse();

            surface.Dispose();
        }

        [Fact]
        public void ShouldAllowModifyingThePixelData()
        {
            
            var surface = Surface.Create(10, 10, PixelFormat.Format_ARGB8888);
            surface.Stride.Should().Be(4 * surface.Width);
            surface.SizeInBytes.Should().Be(surface.Stride * surface.Height);

            var data = new byte[surface.SizeInBytes];
            // Read the surface data
            surface.ReadPixels(ref data);
            Assert.All(data, b => b.Should().Be(0));
            // Set all pixels to white
            for (int i = 0; i < data.Length; i++)
                data[i] = 255;
            surface.SetPixelData(ref data);
            // Read the data again and compare
            var sameData = new byte[surface.SizeInBytes];
            surface.ReadPixels(ref sameData);
            data.Should().BeEquivalentTo(sameData);

            surface.Dispose();
        }

        [Fact]
        public void ShouldCreateSurfaceFromExistingData()
        {
            
            var format = PixelFormat.Format_ABGR8888;
            var width = 10;
            var height = 10;
            var data = new byte[width * height * format.GetBytesPerPixel()];
            // Set pixel data
            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)(128 + (i % 4));
            var surface = Surface.CreateFrom(data, width, height, format);
            // Read the pixel data and compare
            var sameData = new byte[data.Length];
            surface.ReadPixels(ref sameData);
            data.Should().BeEquivalentTo(sameData);

            surface.Dispose();
        }

        [Fact]
        public void ShouldNotConvertPixelDataIfTheFormatIsSame()
        {
            
            var width = 10;
            var height = 10;
            var surface = Surface.Create(width, height, PixelFormat.Format_RGBA8888);
            var pixels = Enumerable.Repeat<ColorRGBA8888>(
                new ColorRGBA8888(TestColor), surface.PixelCount).ToArray();

            // Read and convert the current pixel data
            var currentPixels = new ColorRGBA8888[surface.PixelCount];
            surface.ReadPixels(ref currentPixels);
            AllPixelsShouldBeEqualTo(currentPixels, new Color());

            // Convert and set the pixel data
            surface.SetPixelData(ref pixels);
            surface.ReadPixels(ref currentPixels);
            currentPixels.Should().BeEquivalentTo(pixels);

            surface.Dispose();
        }

        [Fact]
        public void ShouldConvertPixelDataIfFormatsAreDifferent()
        {
            
            var surface = Surface.Create(10, 10, PixelFormat.Format_RGBA8888);
            var pixels = Enumerable.Repeat<ColorARGB8888>(
                new ColorARGB8888(TestColor), surface.PixelCount).ToArray();

            // Read and convert the current pixel data
            var currentPixels = new ColorARGB8888[surface.PixelCount];
            surface.ReadPixels(ref currentPixels);
            AllPixelsShouldBeEqualTo(currentPixels, new Color());

            // Convert and set the pixel data
            surface.SetPixelData(ref pixels);
            surface.ReadPixels(ref currentPixels);
            currentPixels.Should().BeEquivalentTo(pixels);

            surface.Dispose();
        }

        [Fact]
        public void ShouldFillWholeSurfaceWhenNoRectangleIsSpecified()
        {
            
            var surface = Surface.Create(10, 10);
            AllPixelsShouldBeEqualTo(surface, new Color());
            surface.Fill(TestColor);
            AllPixelsShouldBeEqualTo(surface, TestColor);

            surface.Dispose();
        }

        [Fact]
        public void ShouldFillSpecifiedRectangle()
        {
            
            var surface = Surface.Create(10, 10);
            var expected = new[] {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 1, 1, 1, 1, 1, 0, 0, 0,
                0, 0, 1, 1, 1, 1, 1, 0, 0, 0,
                0, 0, 1, 1, 1, 1, 1, 0, 0, 0,
                0, 0, 1, 1, 1, 1, 1, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            }.Select(v => v > 0 ? TestColor : new Color())
                .Select(c => new ColorARGB8888(c));

            surface.Fill(TestColor, new Rectangle(2, 1, 5, 4));
            var actual = new ColorARGB8888[expected.Count()];
            surface.ReadPixels(ref actual);
            actual.Should().BeEquivalentTo(expected);

            surface.Dispose();
        }

        [Fact]
        public void ShouldFillSpecifiedRectangles()
        {
            
            var surface = Surface.Create(10, 10);
            var expected = new[] {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 1, 1, 1, 1, 1, 0, 0, 0,
                0, 0, 1, 1, 1, 1, 1, 0, 0, 0,
                0, 0, 1, 1, 1, 1, 1, 0, 0, 0,
                0, 0, 1, 1, 1, 1, 1, 1, 0, 0,
                0, 0, 0, 1, 1, 1, 1, 1, 0, 0,
                0, 0, 0, 1, 1, 1, 1, 1, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            }.Select(v => v > 0 ? TestColor : new Color())
                .Select(c => new ColorARGB8888(c));

            surface.Fill(TestColor,
                new Rectangle(2, 1, 5, 4),
                new Rectangle(3, 4, 5, 3));

            var actual = new ColorARGB8888[expected.Count()];
            surface.ReadPixels(ref actual);
            actual.Should().BeEquivalentTo(expected);

            surface.Dispose();
        }

        [Fact]
        public void ShouldBlitToWholeSurface()
        {
            
            var color = Color.Red;
            var source = Surface.Create(5, 5);
            var destination = Surface.Create(5, 5);
            source.Fill(color);
            AllPixelsShouldBeEqualTo(source, color);
            source.BlitTo(destination);
            AllPixelsShouldBeEqualTo(destination, color);

            source.Dispose();
            destination.Dispose();
        }

        [Fact]
        public void ShouldBlitInSpecifiedRectangles()
        {
            
            var color = Color.Red;
            var source = Surface.Create(2, 2);
            var destination = Surface.Create(10, 10);
            var expected = new[] {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 1, 1, 1, 1, 1, 0, 0, 0,
                0, 0, 1, 1, 1, 1, 1, 0, 0, 0,
                0, 0, 1, 1, 1, 1, 1, 0, 0, 0,
                0, 0, 1, 1, 1, 1, 1, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            }.Select(v => v > 0 ? color : new Color())
                .Select(c => new ColorARGB8888(c));

            source.Fill(color, new Rectangle(0, 0, 1, 1));
            source.BlitTo(destination,
                srcRect: new Rectangle(0, 0, 1, 1),
                dstRect: new Rectangle(2, 1, 5, 4));

            var actual = new ColorARGB8888[destination.PixelCount];
            destination.ReadPixels(ref actual);
            actual.Should().BeEquivalentTo(expected);

            source.Dispose();
            destination.Dispose();
        }

        [Fact]
        public void ShouldGetAndSetClipRectangle()
        {
            var surface = Surface.Create(10, 10);
            var expected = new Rectangle(1, 2, 3, 4);
            surface.ClipRect.Should().BeNull();
            surface.ClipRect = expected;
            surface.ClipRect.Should().BeEquivalentTo(expected);
            surface.ClipRect = null;
            surface.ClipRect.Should().BeNull();
        }

        [Fact]
        public void ShouldGetAndSetColorMod()
        {
            var surface = Surface.Create(1, 1);
            var expected = Color.CornflowerBlue.WithoutName();
            surface.ColorMod.Should().Be(Color.White.WithoutName());
            surface.ColorMod = expected;
            surface.ColorMod.Should().Be(expected);
            surface.Dispose();
        }

        [Fact]
        public void ShouldGetAndSetAlphaMod()
        {
            var surface = Surface.Create(1, 1);
            byte expected = 123;
            surface.AlphaMod.Should().Be(byte.MaxValue);
            surface.AlphaMod = expected;
            surface.AlphaMod.Should().Be(expected);
            surface.Dispose();
        }

        [Fact]
        public void ShouldGetAndSetBlendMode()
        {
            var surface = Surface.Create(1, 1);
            var expected = BlendMode.Modulate;
            surface.BlendMode.Should().Be(BlendMode.AlphaBlend);
            surface.BlendMode = expected;
            surface.BlendMode.Should().Be(expected);
            surface.Dispose();
        }

        [Fact]
        public void ShouldGetAndSetColorKey()
        {
            var surface = Surface.Create(1, 1);
            var expected = Color.CornflowerBlue.WithoutName();
            surface.ColorKey.Should().BeNull();
            surface.ColorKey = expected;
            surface.ColorKey.Should().Be(expected);
            surface.ColorKey = null;
            surface.ColorKey.Should().BeNull();
            surface.Dispose();
        }

        private void AllPixelsShouldBeEqualTo(Surface surface, Color color)
        {
            var pixels = new ColorARGB8888[surface.PixelCount];
            surface.ReadPixels(ref pixels);
            AllPixelsShouldBeEqualTo(pixels, color);
        }

        private void AllPixelsShouldBeEqualTo<T>(T[] pixels, Color color)
            where T : IColor
        {
            pixels.Distinct().Select(c => c.AsColor()).Should().BeEquivalentTo(
                color.WithoutName()
            );
        }
    }
#endif
}