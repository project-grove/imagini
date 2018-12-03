namespace Imagini.Fonts
{
    /// <summary>
    /// Font drawing options which control the text rendering process.
    /// </summary>
    public struct FontDrawingOptions
    {
        /// <summary>
        /// Font scale.
        /// </summary>
        public float Scale { get; set; }
        /// <summary>
        /// Letter spacing (in pixels). Will be multiplied by the specified scale.
        /// </summary>
        public int LetterSpacing { get; set; }

        /// <summary>
        /// Default font drawing options.
        /// </summary>
        public static FontDrawingOptions Default =
            new FontDrawingOptions()
            {
                Scale = 1.0f,
                LetterSpacing = 0
            };

        /// <summary>
        /// Creates a FontDrawingOptions object with specified parameters.
        /// </summary>
        public FontDrawingOptions(float scale = 1.0f, int spacing = 0) =>
            (Scale, LetterSpacing) = (scale, spacing);
    }
}