using System.Collections.Generic;
using Imagini.Fonts;
using SixLabors.Fonts;
using static Tests.Util;

namespace Tests.Fonts
{
    public static class TestFont
    {
        const string FontName = "Actor-Regular.ttf";

        private static FontCollection s_fonts = new FontCollection();
        public static FontFamily FontFamily { get; private set; }

        static TestFont()
        {
            FontFamily = s_fonts.Install(NearAssembly(FontName));
        }

        public static SpriteFont CreateFont(float size, IEnumerable<char> characters,
            int textureSize = 128) =>
            new SpriteFont(new Font(FontFamily, size, FontStyle.Regular),
                characters, textureSize);
    }
}