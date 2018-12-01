using System.Collections.Generic;
using System.Drawing;
using Imagini.Drawing;
using Imagini.ImageSharp;

namespace Imagini.Fonts.Renderers
{
    public class GraphicsTextRenderer : Resource, ITextRenderer
    {
        public SpriteFont Font { get; private set; }
        private Graphics _graphics;

        private List<Texture> _pages = new List<Texture>();

        public GraphicsTextRenderer(Graphics graphics, SpriteFont font,
            TextureScalingQuality quality = TextureScalingQuality.Linear)
        {
            Font = font;
            _graphics = graphics;
            foreach(var page in font.Pages)
            {
                var texture = TextureFactory.FromImage(_graphics, quality,
                    page.Texture);
                texture.BlendMode = BlendMode.AlphaBlend;
                page.Texture.Dispose(); // dispose and unset the original image
                page.Texture = null;
                _pages.Add(texture);
            }
        }

        public void Draw(string text, PointF position, Color color, FontDrawingOptions options = default(FontDrawingOptions))
        {
            foreach (var tex in _pages)
                tex.ColorMod = color;

            var pageIndex = 0;
            var page = Font.Pages[0];
            var x = (int)position.X;
            var y = (int)position.Y;
            var fontSize = (int)Font.Font.Size;
            foreach(var symbol in text)
            {
                if (_pages.Count > 1)
                {
                    if (symbol < page.Start || symbol > page.End)
                    {
                        pageIndex = Font.GetPageIndex(symbol);
                        page = Font.Pages[pageIndex];
                    }
                }
                var srcRect = page.GetGlyph(symbol);
                if (char.IsWhiteSpace(symbol))
                {
                    x += srcRect.IsEmpty ? fontSize : srcRect.Width;
                    x += options.LetterSpacing;
                    continue;
                }
                if (srcRect.IsEmpty)
                {
                    // symbol not found, replace with a rectangle
                    _graphics.DrawRect(new Rectangle(x, y, fontSize, fontSize));
                    x += fontSize + 1 + options.LetterSpacing;
                    continue;
                }

                var dstRect = new Rectangle(x, y, srcRect.Width, srcRect.Height);
                _graphics.Draw(_pages[pageIndex], srcRect, dstRect);
                x += srcRect.Width + options.LetterSpacing;
            }
        }

        public void Draw(string text, PointF position, FontDrawingOptions options = default(FontDrawingOptions))
        {
            Draw(text, position, _graphics.GetDrawingColor(), options);
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            base.Destroy();
        }
    }
}