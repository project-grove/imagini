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
        FontDrawingOptions scale75Percent = new FontDrawingOptions(scale: 0.75f);

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
            textRenderer.Draw("Hello, world!", new PointF(10.0f, 30.0f),
                Color.AntiqueWhite, scale75Percent);

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
            textRenderer.Draw("Hello, world!", new PointF(10.0f, 30.0f), scale75Percent);

            graphics.OutputIsCloseTo(
                NearAssembly("ShouldDrawMultiPagedSpriteFonts.png")
            );

            textRenderer.Dispose();
        }

        [Theory]
        [InlineData(128)]
        [InlineData(64)]
        public void ShouldReplaceMissingCharsWithSquares(int textureSize)
        {
            var textRenderer = graphics.CreateTextRenderer(
                CreateFont(12, Symbols.ASCII, textureSize)
            );
            graphics.Clear(Color.DarkGray);
            graphics.SetDrawingColor(Color.AntiqueWhite);
            textRenderer.Draw("Hello, мир!", new PointF(10.0f, 10.0f));
            textRenderer.Draw("Hello, мир!", new PointF(10.0f, 30.0f), scale75Percent);

            graphics.OutputIsCloseTo(
                NearAssembly("ShouldReplaceMissingCharsWithSquares.png")
            );

            textRenderer.Dispose();
        }

        [Theory]
        [InlineData(128, "Hello, world!", 69, 47)]
        [InlineData(64, "Hello, мир!", 77, 56)]
        public void ShouldMeasureText(int textureSize, string text,
            int expectedWidth, int expectedScaledWidth)
        {
            var textRenderer = graphics.CreateTextRenderer(
                CreateFont(12, Symbols.ASCII, textureSize)
            );

            var size = textRenderer.Measure(text);
            size.Should().BeEquivalentTo(new Size(expectedWidth, 12));

            var scaledSize = textRenderer.Measure(text, scale75Percent);
            scaledSize.Should().BeEquivalentTo(new Size(expectedScaledWidth, 9));
            textRenderer.Dispose();
        }
    }
}