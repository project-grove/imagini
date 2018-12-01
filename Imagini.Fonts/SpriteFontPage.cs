using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Shapes.Temp;

using SPointF = SixLabors.Primitives.PointF;

namespace Imagini.Fonts
{
    /// <summary>
    /// Represents a sprite font page with the texture atlas and glyph bounds data.
    /// </summary>
    public class SpriteFontPage
    {
        /// <summary>
        /// Returns the starting code point of this page.
        /// </summary>
        public char Start { get; private set; }
        /// <summary>
        /// Returns the ending code point of this page.
        /// </summary>
        public char End { get; private set; }

        private Dictionary<char, Rectangle> _glyphMap = 
            new Dictionary<char, Rectangle>();

        /// <summary>
        /// Returns the Image containing the glyphs of this page.
        /// </summary>
        /// <remarks>
        /// Intended for use by text renderers.
        /// Can be disposed and nullified by the renderer if it
        /// uses other surface type (GPU texture, for example).
        /// </remarks>
        public Image<Rgba32> Texture { get; set; }

        internal SpriteFontPage(Font font, 
            ref ISet<char> requestedSymbols, int textureSize, int padding)
        {
            Texture = new Image<Rgba32>(textureSize, textureSize);
            Texture.Mutate(x => x.Fill(Rgba32.Transparent));
            RenderFontToTexture(font, ref requestedSymbols, textureSize, padding);            
        }

        private void RenderFontToTexture(Font font, 
            ref ISet<char> requestedSymbols, int textureSize, int padding)
        {
            var options = new RendererOptions(font);
            int x = 0, y = 0, maxHeight = 0;
            do
            {
                var glyphStr = $"{requestedSymbols.First()}";
                var glyph = glyphStr[0];
                var sizeF = TextMeasurer.Measure(glyphStr, options);
                var size = new Size((int)Math.Ceiling(sizeF.Width), (int)Math.Ceiling(sizeF.Height));
                maxHeight = Math.Max(maxHeight, size.Height);

                var dstRect = new Rectangle(x, y, size.Width, size.Height);
                if (dstRect.Right >= textureSize)
                {
                    // jump onto the next line
                    x = 0;
                    y += maxHeight + padding;
                    maxHeight = 0;
                    dstRect.X = x;
                    dstRect.Y = y;
                }
                if (dstRect.Bottom >= textureSize)
                {
                    // we can't fit letters anymore
                    break;
                }

                var shapes = TextBuilder.GenerateGlyphs(glyphStr, new SPointF(x, y), options);
                Texture.Mutate(m => m.Fill(Rgba32.White, shapes)); 
                _glyphMap.Add(glyph, dstRect);
                requestedSymbols.Remove(glyph);
                if (Start == default(char)) Start = glyph;
                End = glyph;

                x += size.Width + padding;
            } while (requestedSymbols.Any());
        }

        /// <summary>
        /// Checks if this page contains the specified symbol.
        /// </summary>
        public bool HasGlyph(char symbol) => _glyphMap.ContainsKey(symbol);

        /// <summary>
        /// Returns the rectangle defining pixel region where the specified
        /// glyph is located. Returns empty rectangle if the glyph is not present.
        /// </summary>
        public Rectangle GetGlyph(char symbol)
        {
            if (!HasGlyph(symbol)) return Rectangle.Empty;
            return _glyphMap[symbol];
        }
    }
}