using System;
using System.Drawing;
using System.Linq;
using FluentAssertions;
using Imagini;
using Imagini.Drawing;
using Xunit;

namespace Tests.Drawing
{
    [DisplayTestMethodName]
    public class PaletteTest 
    {
        [Fact]
        public void ShouldSetColors()
        {
            
            var expected = new[] {
                Color.Red,
                Color.Lime,
                Color.Blue,
                Color.Black
            };
            var palette = new Palette(expected);

            palette.Colors.Should().BeEquivalentTo(expected);

            palette.Dispose();
        }

        [Fact]
        public void ShouldThrowIfInvalidColorCount()
        {
            
            var zeroColors = new Color[] { };
            var tooManyColors = Enumerable.Repeat(Color.Black, Palette.MaximumColors + 1);
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new Palette(zeroColors));
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new Palette(tooManyColors));
        }
    }
}