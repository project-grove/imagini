using System.Drawing;
using System.Linq;
using FluentAssertions;
using Imagini;
using Imagini.Drawing;
using Xunit;

namespace Tests.Drawing
{
#if !HEADLESS
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
                new ColorRGBA8888(TestColor), width * height
            ).ToArray();
            // Set the pixel data and check it
            source.SetPixelData(ref pixels);
            var currentPixels = new ColorRGBA8888[width * height];
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
            var width = 10;
            var height = 10;
            var pixels = Enumerable.Repeat<ColorRGBA8888>(
                new ColorRGBA8888(TestColor), width * height).ToArray();
            var surface = Surface.Create(width, height, PixelFormat.Format_RGBA8888);

            // Read and convert the current pixel data
            var currentPixels = new ColorRGBA8888[width * height];
            surface.GetPixelData(ref currentPixels);
            currentPixels.Distinct().Should().BeEquivalentTo(
                new ColorRGBA8888(new Color())
            );

            // Convert and set the pixel data
            surface.SetPixelData(ref pixels);
            surface.GetPixelData(ref currentPixels);
            currentPixels.Should().BeEquivalentTo(pixels);

            surface.Dispose();
        }

        [Fact]
        public void ShouldConvertPixelDataIfFormatsAreDifferent()
        {
            var width = 10;
            var height = 10;
            var pixels = Enumerable.Repeat<ColorRGBA8888>(
                new ColorRGBA8888(TestColor), width * height).ToArray();
            var surface = Surface.Create(width, height, PixelFormat.Format_ARGB8888);

            // Read and convert the current pixel data
            var currentPixels = new ColorRGBA8888[width * height];
            surface.GetPixelData(ref currentPixels);
            currentPixels.Distinct().Should().BeEquivalentTo(
                new ColorRGBA8888(new Color())
            );

            // Convert and set the pixel data
            surface.SetPixelData(ref pixels);
            surface.GetPixelData(ref currentPixels);
            currentPixels.Should().BeEquivalentTo(pixels);

            surface.Dispose();
        }
    }
#endif
}