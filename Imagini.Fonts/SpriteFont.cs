using System.Collections.Generic;
using SixLabors.Fonts;
using System.Linq;

/// <summary>
/// Sprite font generation and drawing module.
/// </summary>
namespace Imagini.Fonts
{
    /// <summary>
    /// Represents a sprite font.
    /// </summary>
    public class SpriteFont
    {
        /// <summary>
        /// Returns the pages of this sprite font.
        /// </summary>
        public IReadOnlyList<SpriteFontPage> Pages { get; private set; }

        private List<(char start, char end)> _rangeLookup = new List<(char start, char end)>();

        public SpriteFont(Font font, IEnumerable<char> symbols,
            int textureSize = 512, int padding = 1)
        {
            ISet<char> chars = new HashSet<char>(symbols);
            var pages = new List<SpriteFontPage>();
            do
            {
                var before = chars.Count;
                var page = new SpriteFontPage(font, ref chars, textureSize, padding);
                var after = chars.Count;
                if (after == before)
                    throw new ImaginiException("Texture size is too small");
                pages.Add(page);
                _rangeLookup.Add((page.Start, page.End));
            } while (chars.Any());
            Pages = pages;
        }

        /// <summary>
        /// Returns index of the page that can contain the specified glyph.
        /// Returns -1 if the glyph code point is out of range.
        /// </summary>
        public int GetPageIndex(char glyph)
        {
            return _rangeLookup.FindIndex(pair => 
                glyph >= pair.start &&
                glyph <= pair.end);
        }
    }
}