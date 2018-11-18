using System.Drawing;
using System.Linq;
using FluentAssertions;
using Imagini.Drawing;
using Xunit;

namespace Tests.Drawing
{
    [DisplayTestMethodName]
    public class PixelsTest
    {
        [Fact]
        public void ShouldConvertPixels()
        {
            var width = 10;
            var height = 10;
            var count = width * height;
            var stride = width * 4; // 4 bytes per pixel
            var before = Enumerable.Repeat<ColorRGBA8888>(
                new ColorRGBA8888(Color.FromArgb(unchecked((int)0xDEADBEEF))),
                count
            ).ToArray();
            var after = new ColorARGB8888[count];
            Pixels.Convert(width, height, stride, stride, ref before, ref after);
            ShouldBeEquivalent(before, after);
        }

        private void ShouldBeEquivalent<T1, T2>(T1[] one, T2[] two)
            where T1 : IColor
            where T2 : IColor
        {
            var first = one.Select(c => c.AsColor());
            var second = two.Select(c => c.AsColor());
            first.Should().BeEquivalentTo(second);
        }
    }
}