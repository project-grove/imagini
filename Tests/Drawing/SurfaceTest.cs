using System.Drawing;
using System.Linq;
using FluentAssertions;
using Imagini;
using Imagini.Drawing;
using Imagini.ImageSharp;
using Xunit;

namespace Tests.Drawing
{
#if !HEADLESS
    public class SurfaceTest : TestBase
    {
        Color TestColor = Color.FromArgb(unchecked((int)0xDEADBEEF));

        [Fact]
        public void ShouldCreateRGBASurfaceWithDefaultParameters()
        {
            PrintTestName();
            var surface = Surface.Create(200, 100);
            surface.Width.Should().Be(200);
            surface.Height.Should().Be(100);
            surface.PixelInfo.BitsPerPixel.Should().Be(32);

            surface.Dispose();
        }

        [Fact]
        public void ShouldCreateSurfaceWhenMaskIsSpecified()
        {
            PrintTestName();
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
            PrintTestName();
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
            PrintTestName();
            Assert.Throws<ImaginiException>(() =>
            {
                var invalidSurface = Surface.Create(0, 0, depth: 0);
            });
        }

        [Fact]
        public void ShouldConvertToSpecifiedFormat()
        {
            PrintTestName();
            var width = 10;
            var height = 10;
            var source = Surface.Create(width, height, PixelFormat.Format_RGBA8888);
            var pixels = Enumerable.Repeat<ColorRGBA8888>(
                new ColorRGBA8888(TestColor), source.PixelCount
            ).ToArray();
            // Set the pixel data and check it
            source.SetPixelData(ref pixels);
            var currentPixels = new ColorRGBA8888[source.PixelCount];
            source.GetPixelData(ref currentPixels);
            currentPixels.Should().BeEquivalentTo(pixels);
            // Convert the surface and check it again
            var converted = source.ConvertTo(PixelFormat.Format_ARGB8888);
            source.GetPixelData(ref currentPixels);
            currentPixels.Should().BeEquivalentTo(pixels);

            source.Dispose();
            converted.Dispose();
        }

        [Fact]
        public void ShouldOptimizeForSpecifiedFormat()
        {
            PrintTestName();
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
            PrintTestName();
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
            PrintTestName();
            var surface = Surface.Create(10, 10, PixelFormat.Format_ARGB8888);
            surface.Stride.Should().Be(4 * surface.Width);
            surface.SizeInBytes.Should().Be(surface.Stride * surface.Height);

            var data = new byte[surface.SizeInBytes];
            // Read the surface data
            surface.GetPixelData(ref data);
            Assert.All(data, b => b.Should().Be(0));
            // Set all pixels to white
            for (int i = 0; i < data.Length; i++)
                data[i] = 255;
            surface.SetPixelData(ref data);
            // Read the data again and compare
            var sameData = new byte[surface.SizeInBytes];
            surface.GetPixelData(ref sameData);
            data.Should().BeEquivalentTo(sameData);

            surface.Dispose();
        }

        [Fact]
        public void ShouldCreateSurfaceFromExistingData()
        {
            PrintTestName();
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
            surface.GetPixelData(ref sameData);
            data.Should().BeEquivalentTo(sameData);

            surface.Dispose();
        }

        [Fact]
        public void ShouldNotConvertPixelDataIfTheFormatIsSame()
        {
            PrintTestName();
            var width = 10;
            var height = 10;
            var surface = Surface.Create(width, height, PixelFormat.Format_RGBA8888);
            var pixels = Enumerable.Repeat<ColorRGBA8888>(
                new ColorRGBA8888(TestColor), surface.PixelCount).ToArray();

            // Read and convert the current pixel data
            var currentPixels = new ColorRGBA8888[surface.PixelCount];
            surface.GetPixelData(ref currentPixels);
            AllPixelsShouldBeEqualTo(currentPixels, new Color());

            // Convert and set the pixel data
            surface.SetPixelData(ref pixels);
            surface.GetPixelData(ref currentPixels);
            currentPixels.Should().BeEquivalentTo(pixels);

            surface.Dispose();
        }

        [Fact]
        public void ShouldConvertPixelDataIfFormatsAreDifferent()
        {
            PrintTestName();
            var surface = Surface.Create(10, 10, PixelFormat.Format_RGBA8888);
            var pixels = Enumerable.Repeat<ColorARGB8888>(
                new ColorARGB8888(TestColor), surface.PixelCount).ToArray();

            // Read and convert the current pixel data
            var currentPixels = new ColorARGB8888[surface.PixelCount];
            surface.GetPixelData(ref currentPixels);
            AllPixelsShouldBeEqualTo(currentPixels, new Color());

            // Convert and set the pixel data
            surface.SetPixelData(ref pixels);
            surface.GetPixelData(ref currentPixels);
            currentPixels.Should().BeEquivalentTo(pixels);

            surface.Dispose();
        }

        [Fact]
        public void ShouldFillWholeSurfaceWhenNoRectangleIsSpecified()
        {
            PrintTestName();
            var surface = Surface.Create(10, 10);
            AllPixelsShouldBeEqualTo(surface, new Color());
            surface.Fill(TestColor);
            AllPixelsShouldBeEqualTo(surface, TestColor);

            surface.Dispose();
        }

        [Fact]
        public void ShouldFillSpecifiedRectangle()
        {
            PrintTestName();
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
            surface.GetPixelData(ref actual);
            actual.Should().BeEquivalentTo(expected);
            
            surface.Dispose();
        }

        [Fact]
        public void ShouldFillSpecifiedRectangles()
        {
            PrintTestName();
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

            surface.SaveAsPng("ShouldFillSpecifiedRectangles.png");
            var actual = new ColorARGB8888[expected.Count()];
            surface.GetPixelData(ref actual);
            actual.Should().BeEquivalentTo(expected);

            surface.Dispose();
        }

        [Fact]
        public void ShouldBlitToWholeSurface()
        {
            PrintTestName();
            var source = Surface.Create(5, 5);
            var destination = Surface.Create(5, 5);
            source.Fill(TestColor);
            AllPixelsShouldBeEqualTo(source, TestColor);
            source.BlitTo(destination);
            AllPixelsShouldBeEqualTo(destination, TestColor);

            source.Dispose();
            destination.Dispose();
        }

        [Fact]
        public void ShouldBlitInSpecifiedRectangles()
        {
            PrintTestName();
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
            }.Select(v => v > 0 ? TestColor : new Color())
                .Select(c => new ColorARGB8888(c));

            source.Fill(TestColor, new Rectangle(0, 0, 1, 1));
            source.BlitTo(destination, 
                srcRect: new Rectangle(0, 0, 1, 1),
                dstRect: new Rectangle(2, 1, 5, 4));
            
            var actual = new ColorARGB8888[destination.PixelCount];
            destination.GetPixelData(ref actual);
            actual.Should().BeEquivalentTo(expected);
            
            source.Dispose();
            destination.Dispose();
        }

        private void AllPixelsShouldBeEqualTo(Surface surface, Color color)
        {
            var pixels = new ColorARGB8888[surface.PixelCount];
            surface.GetPixelData(ref pixels);
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