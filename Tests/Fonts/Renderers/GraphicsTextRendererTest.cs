using System;
using System.Drawing;
using FluentAssertions;
using Imagini;
using Imagini.Drawing;
using Imagini.Fonts;
using Xunit;
using static Tests.Fonts.TestFont;
using static Tests.Util;

namespace Tests.Fonts.Renderers
{
    public class GraphicsTextRendererTest : IDisposable
    {
        App2D app = new SampleApp();
        Graphics graphics => app.Graphics;

        public void Dispose() => app.Dispose();

        [Fact]
        public void ShouldDrawTextWithSpaces()
        {
            var textRenderer = graphics.CreateTextRenderer(
                CreateFont(12, Symbols.ASCII)
            );
            graphics.Clear(Color.DarkGray);
            textRenderer.Draw("Hello, world!", new PointF(10.0f, 10.0f),
                Color.AntiqueWhite);

            graphics.OutputIsCloseTo(
                NearAssembly("ShouldDrawTextWithSpaces.png")
            );

            textRenderer.Dispose();
        }

        [Fact]
        public void ShouldDrawMultiPagedSpriteFonts()
        {
            var textRenderer = graphics.CreateTextRenderer(
                CreateFont(12, Symbols.ASCII, textureSize: 64)
            );
            textRenderer.Font.Pages.Count.Should().BeGreaterThan(1);
            graphics.Clear(Color.DarkGray);
            graphics.SetDrawingColor(Color.AntiqueWhite);
            textRenderer.Draw("Hello, world!", new PointF(10.0f, 10.0f));

            graphics.OutputIsCloseTo(
                NearAssembly("ShouldDrawMultiPagedSpriteFonts.png")
            );

            textRenderer.Dispose();
        }

        [Fact]
        public void ShouldReplaceMissingCharsWithSquares()
        {
            var textRenderer = graphics.CreateTextRenderer(
                CreateFont(12, Symbols.ASCII)
            );
            graphics.Clear(Color.DarkGray);
            graphics.SetDrawingColor(Color.AntiqueWhite);
            textRenderer.Draw("Hello, мир!", new PointF(10.0f, 10.0f));

            graphics.OutputIsCloseTo(
                NearAssembly("ShouldReplaceMissingCharsWithSquares.png")
            );

            textRenderer.Dispose();
        }
    }
}