using System.Drawing;
using FluentAssertions;
using Imagini.Drawing;
using Xunit;

namespace Tests.Drawing
{
    public class PaletteTest
    {
        [Fact]
        public void ShouldSetColors()
        {
            var expected = new [] {
                Color.Red,
                Color.Green,
                Color.Blue,
                Color.Black
            };
            var palette = new Palette(expected);

            palette.Colors.Should().BeEquivalentTo(expected);

            palette.Dispose();
        }        
    }
}