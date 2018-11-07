using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
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
            format.Format.Should().Be(PixelFormat.Format_RGBA8888);
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

            var sameFormat = new PixelFormatInfo(format.Handle);
            format.Should().BeEquivalentTo(sameFormat);
            format.Dispose(); // free it once because both point to the same ptr
        }

        [Fact]
        public void ShouldCreateFormatsWithPalette()
        {
            var colors = new [] {
                Color.Red,
                Color.Blue,
                Color.Lime,
                Color.Black
            }.Select(color => color.WithoutName());
            var format = new PixelFormatInfo(PixelFormat.Format_INDEX8);
            var palette = new Palette(colors);
            format.Palette = palette;

            format.Format.Should().Be(PixelFormat.Format_INDEX8);
            format.Palette.Should().Be(palette);

            var sameFormat = new PixelFormatInfo(format.Handle);
            format.Should().BeEquivalentTo(sameFormat);
            format.Dispose(); // free it once because both point to the same ptr
        }
    }

    internal static class ColorTestExtensions
    {
        /// <summary>
        /// Removes the color name so the equality test passes for known
        /// colors too
        /// </summary>
        public static Color WithoutName(this Color color) =>
            Color.FromArgb(color.ToArgb());
    }
}