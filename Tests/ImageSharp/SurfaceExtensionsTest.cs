using System.Drawing;
using System.IO;
using FluentAssertions;
using Imagini.Drawing;
using Imagini.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using Xunit;

namespace Tests.ImageSharp
{
    [DisplayTestMethodName]
    public class SurfaceExtensionsTest 
    {
        Color TestColor = Color.FromArgb(unchecked((int)0xDEADBEEF));

        [Fact]
        public void ShouldSaveAndLoadAsPNG()
        {
            
            var surface = Surface.Create(10, 10);
            surface.Fill(TestColor);
            
            var ms = new MemoryStream();
            surface.SaveAsPng(ms);
            surface.SaveAsPng("ShouldSaveAndLoadAsPNG.png");
            ms.Seek(0, SeekOrigin.Begin);
            var sameSurface = SurfaceFactory.FromStream(ms, new PngDecoder());
            ms.Dispose();

            var pixels = new ColorRGBA8888[surface.PixelCount];
            var samePixels = new ColorRGBA8888[surface.PixelCount];
            surface.GetPixelData(ref pixels);
            sameSurface.GetPixelData(ref samePixels);
            samePixels.Should().BeEquivalentTo(pixels);
        }
    }
}