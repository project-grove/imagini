using FluentAssertions;
using Imagini;
using Imagini.Fonts;
using SixLabors.Fonts;
using Xunit;

using static Tests.Util;
using static Tests.Fonts.TestFont;

namespace Tests.Fonts
{
    [DisplayTestMethodName]
    public class SpriteFontTest
    {

        [Fact]
        public void ShouldProduceOnePageIfFits()
        {
            var chars = "abcd";
            var spriteFont = CreateFont(size: 16, characters: chars, textureSize: 128);

            spriteFont.Pages.Should().ContainSingle();
            foreach (var character in chars)
            {
                var pageIndex = spriteFont.GetPageIndex(character);
                pageIndex.Should().Be(0);
                spriteFont.Pages[pageIndex].HasGlyph(character).Should().BeTrue();
            }

            spriteFont.Font.Size.Should().Be(16);
            spriteFont.Font.Family.Should().Be(TestFont.FontFamily);
            spriteFont.Dispose();
        }

        [Fact]
        public void ShouldProduceSeveralPagesIfTooManyPerOne()
        {
            var chars = "abcdefghABCDEFGH";
            var spriteFont = CreateFont(size: 16, characters: chars, textureSize: 48);
            spriteFont.Pages.Count.Should().BeGreaterThan(1);
            spriteFont.Dispose();
        }

        [Fact]
        public void ShouldThrowExceptionIfTextureIsTooSmall()
        {
            var font = new Font(TestFont.FontFamily, 16, FontStyle.Regular);
            var chars = "abcd";
            Assert.ThrowsAny<ImaginiException>(() =>
                new SpriteFont(font, chars, textureSize: 10));
        }
    }
}