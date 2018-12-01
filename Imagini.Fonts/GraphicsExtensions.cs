using Imagini.Drawing;
using Imagini.Fonts;
using Imagini.Fonts.Renderers;

namespace Imagini.Fonts
{
    /// <summary>
    /// Contains Graphics class related extension methods.
    /// </summary>
    public static class GraphicsExtensions
    {
        /// <summary>
        /// Creates a text renderer for the specified font.
        /// </summary>
        public static ITextRenderer CreateTextRenderer(
            this Graphics graphics, SpriteFont font, 
            TextureScalingQuality quality = TextureScalingQuality.Linear) =>
            new GraphicsTextRenderer(graphics, font, quality);
    }
}