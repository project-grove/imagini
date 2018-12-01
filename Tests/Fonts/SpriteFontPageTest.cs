using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Imagini.Fonts;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using Xunit;

using static Tests.Util;

namespace Tests.Fonts
{
    [DisplayTestMethodName]
    public class SpriteFontPageTest
    {
        private ISet<char> GetChars(string chars) => new HashSet<char>(chars);
        const string FontName = "Actor-Regular.ttf";

        private FontCollection fonts = new FontCollection();
        private FontFamily FontFamily;

        public SpriteFontPageTest()
        {
            FontFamily = fonts.Install(NearAssembly(FontName));
        }

        [Fact]
        public void ShouldHaveSpecifiedSymbols()
        {
            var font = new Font(FontFamily, 16, FontStyle.Regular);
            var chars = GetChars("abcd");
            ISet<char> charsToProcess = new HashSet<char>(chars);

            var page = new SpriteFontPage(font, ref charsToProcess, 64, 1);
            SaveImage(page.Texture, "ShouldHaveSpecifiedSymbols.png");
            page.Start.Should().Be('a');
            page.End.Should().Be('d');
            foreach (var character in chars)
            {
                page.HasGlyph(character).Should().BeTrue();
                var rectangle = page.GetGlyph(character);
                rectangle.IsEmpty.Should().BeFalse();
            }
            page.GetGlyph('Z').IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void ShouldStopIfNotFitting()
        {
            var font = new Font(FontFamily, 16, FontStyle.Regular);
            var chars = GetChars("abcde");
            ISet<char> charsToProcess = new HashSet<char>(chars);

            var page = new SpriteFontPage(font, ref charsToProcess, 35, 1);
            SaveImage(page.Texture, "ShouldStopIfNotFitting.png");
            page.Start.Should().Be('a');
            page.End.Should().Be('c');
            charsToProcess.Should().BeEquivalentTo('d', 'e');
        }
    }
}