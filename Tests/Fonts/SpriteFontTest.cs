using FluentAssertions;
using Imagini;
using Imagini.Fonts;
using SixLabors.Fonts;
using Xunit;

using static Tests.Util;

namespace Tests.Fonts
{
    public class SpriteFontTest
    {
        const string FontName = "Actor-Regular.ttf";

        private FontCollection fonts = new FontCollection();
        private FontFamily FontFamily;

        public SpriteFontTest()
        {
            FontFamily = fonts.Install(NearAssembly(FontName));
        }

        [Fact]
        public void ShouldProduceOnePageIfFits()
        {
            var font = new Font(FontFamily, 16, FontStyle.Regular);
            var chars = "abcd";
            var spriteFont = new SpriteFont(font, chars, textureSize: 128);

            spriteFont.Pages.Should().ContainSingle();
            foreach (var character in chars)
            {
                var pageIndex = spriteFont.GetPageIndex(character);
                pageIndex.Should().Be(0);
                spriteFont.Pages[pageIndex].HasGlyph(character).Should().BeTrue();
            }
        }

        [Fact]
        public void ShouldProduceSeveralPagesIfTooManyPerOne()
        {
            var font = new Font(FontFamily, 16, FontStyle.Regular);
            var chars = "abcdefghABCDEFGH";
            var spriteFont = new SpriteFont(font, chars, textureSize: 48);

            spriteFont.Pages.Count.Should().BeGreaterThan(1);
        }

        [Fact]
        public void ShouldThrowExceptionIfTextureIsTooSmall()
        {
            var font = new Font(FontFamily, 16, FontStyle.Regular);
            var chars = "abcd";
            Assert.ThrowsAny<ImaginiException>(() =>
                new SpriteFont(font, chars, textureSize: 10));
        }
    }
}