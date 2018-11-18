using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Imagini.Drawing;
using Xunit;

namespace Tests.Drawing
{
    [DisplayTestMethodName]
    public class PixelFormatInfoTest 
    {
        Func<EquivalencyAssertionOptions<PixelFormatInfo>, EquivalencyAssertionOptions<PixelFormatInfo>> options =
            o => o
                .IncludingAllRuntimeProperties()
                .Excluding(m => m.SelectedMemberPath.EndsWith("ResourceID"));

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
            format.Should().BeEquivalentTo(sameFormat, options);
            format.Dispose();
        }

        [Fact]
        public void ShouldCreateFormatsWithPalette()
        {
            
            var colors = Enumerable.Repeat(Color.Red.WithoutName(), 256);
            var format = new PixelFormatInfo(PixelFormat.Format_INDEX8);
            var palette = new Palette(colors);
            format.Palette = palette;

            format.Format.Should().Be(PixelFormat.Format_INDEX8);
            format.Palette.Should().Be(palette);

            var sameFormat = new PixelFormatInfo(format.Handle);
            format.Should().BeEquivalentTo(sameFormat, options);
            format.Dispose();
            // sameFormat.Palette.Dispose();
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