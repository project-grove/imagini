using System;
using System.Drawing;
using System.Linq;
using FluentAssertions;
using Imagini;
using Imagini.Drawing;
using Xunit;

namespace Tests.Drawing
{
    public class PaletteTest : TestBase
    {
        [Fact]
        public void ShouldSetColors()
        {
            PrintTestName();
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
            PrintTestName();
            var zeroColors = new Color[] { };
            var tooManyColors = Enumerable.Repeat(Color.Black, Palette.MaximumColors + 1);
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new Palette(zeroColors));
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new Palette(tooManyColors));
        }
    }
}