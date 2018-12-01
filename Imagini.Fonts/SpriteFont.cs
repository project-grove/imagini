using System.Collections.Generic;

/// <summary>
/// Sprite font generation and drawing module.
/// </summary>
namespace Imagini.Fonts
{
    /// <summary>
    /// Represents a sprite font
    /// </summary>
    public class SpriteFont
    {
        /// <summary>
        /// Returns the pages of this sprite font.
        /// </summary>
        public IReadOnlyCollection<SpriteFontPage> Pages { get; private set; }

    }
}