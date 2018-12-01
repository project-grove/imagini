using System.Drawing;

namespace Imagini.Fonts
{
    public interface ITextRenderer
    {
        SpriteFont Font { get; set; }
        void Draw(string text, PointF position, 
            FontDrawingOptions options = new FontDrawingOptions());
        void Draw(string text, PointF position, Color color,
            FontDrawingOptions options = new FontDrawingOptions());
    }
}