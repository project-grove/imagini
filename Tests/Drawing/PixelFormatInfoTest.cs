using FluentAssertions;
using Imagini.Drawing;
using Xunit;

namespace Tests.Drawing
{
    public class PixelFormatInfoTest
    {
        [Fact]
        public void ShouldCreateFormatsWithoutPalette()
        {
            var format = new PixelFormatInfo(PixelFormat.Format_RGBA8888);
            Assert.Null(format.Palette);
            format.BitsPerPixel.Should().Be(32);
            format.BytesPerPixel.Should().Be(4);
            unchecked
            {
                format.MaskR.Should().Be((int)0xFF000000);
                format.MaskG.Should().Be((int)0x00FF0000);
                format.MaskB.Should().Be((int)0x0000FF00);
                format.MaskA.Should().Be((int)0x000000FF);
            }
            format.Dispose();
        }
    }
}