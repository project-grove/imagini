using System;
using System.Drawing;

namespace Imagini.Fonts
{
    public interface ITextRenderer : IDisposable
    {
        SpriteFont Font { get; }
        void Draw(string text, PointF position, 
            FontDrawingOptions options = new FontDrawingOptions());
        void Draw(string text, PointF position, Color color,
            FontDrawingOptions options = new FontDrawingOptions());
        Size Measure(string text, FontDrawingOptions options = new FontDrawingOptions());
    }
}