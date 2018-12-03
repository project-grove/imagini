using System;
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

        public void Draw(string text, PointF position, Color color, 
            FontDrawingOptions options = new FontDrawingOptions())
        {
            if (options.Equals(default(FontDrawingOptions)))
                options = FontDrawingOptions.Default;

            if (options.Scale <= 0.0f)
                return;

            foreach (var tex in _pages)
                tex.ColorMod = color;

            var scale = options.Scale;
            var scaledSpacing = (int)(options.LetterSpacing * scale);

            var pageIndex = 0;
            var page = Font.Pages[0];
            var x = (int)position.X;
            var y = (int)position.Y;
            var scaledFontSize = (int)(Font.Font.Size * scale);
            foreach(var symbol in text)
            {
                if (_pages.Count > 1)
                {
                    if (symbol < page.Start || symbol > page.End)
                    {
                        pageIndex = Font.GetPageIndex(symbol);
                        if (pageIndex < 0) pageIndex = 0;
                        page = Font.Pages[pageIndex];
                    }
                }
                var srcRect = page.GetGlyph(symbol);
                if (char.IsWhiteSpace(symbol))
                {
                    x += srcRect.IsEmpty ? scaledFontSize : (int)(srcRect.Width * scale);
                    x += scaledSpacing;
                    continue;
                }
                if (srcRect.IsEmpty)
                {
                    // symbol not found, replace with a rectangle
                    _graphics.DrawRect(new Rectangle(x, y, scaledFontSize, scaledFontSize));
                    x += scaledFontSize + 1 + scaledSpacing;
                    continue;
                }

                var scaledWidth = scale == 1.0f ? srcRect.Width : (int)(srcRect.Width * scale);
                var scaledHeight = scale == 1.0f ? srcRect.Height: (int)(srcRect.Height * scale);
                var dstRect = new Rectangle(x, y, scaledWidth, scaledHeight);
                _graphics.Draw(_pages[pageIndex], srcRect, dstRect);
                x += scaledWidth + scaledSpacing;
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

        public Size Measure(string text, FontDrawingOptions options = new FontDrawingOptions())
        {
            if (options.Equals(default(FontDrawingOptions)))
                options = FontDrawingOptions.Default;
            if (options.Scale <= 0.0f)
                return Size.Empty;
            
            var scale = options.Scale;
            var scaledSpacing = (int)(options.LetterSpacing * scale);

            var pageIndex = 0;
            var page = Font.Pages[0];
            int x = 0;
            var scaledFontSize = (int)(Font.Font.Size * scale);
            foreach(var symbol in text)
            {
                if (_pages.Count > 1)
                {
                    if (symbol < page.Start || symbol > page.End)
                    {
                        pageIndex = Font.GetPageIndex(symbol);
                        if (pageIndex < 0) pageIndex = 0;
                        page = Font.Pages[pageIndex];
                    }
                }
                var srcRect = page.GetGlyph(symbol);
                if (char.IsWhiteSpace(symbol))
                {
                    x += srcRect.IsEmpty ? scaledFontSize : (int)(srcRect.Width * scale);
                    x += scaledSpacing;
                    continue;
                }
                if (srcRect.IsEmpty)
                {
                    // symbol not found, replace with a rectangle
                    x += scaledFontSize + 1 + scaledSpacing;
                    continue;
                }
                x += (int)(srcRect.Width * scale) + scaledSpacing;
            }
            return new Size(x, scaledFontSize);
        }
    }
}