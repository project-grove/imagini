using System.Drawing;
using System.IO;
using System.Linq;
using Imagini.Drawing;
using Xunit;
using Imagini.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using FluentAssertions;
using System;

namespace Tests.ImageSharp
{
    [DisplayTestMethodName]
    public class TextureExtensionsTest : IDisposable
    {
        SampleApp app = new SampleApp(visible: true);
        Graphics graphics => app.Graphics;

        public void Dispose() => app.Dispose();

        [Fact]
        public void ShouldSaveAndLoadTextureFromPng()
        {
            var color = new ColorRGB888(
                Color.FromArgb(unchecked((int)0xFFC0FFEE))
            );
            var size = graphics.OutputSize;
            // Prepare a texture, draw it and save framebuffer contents
            var texture = graphics.CreateTexture(size.Width, size.Height);
            var pixels = Enumerable.Repeat(color, texture.PixelCount).ToArray();
            texture.SetPixels(ref pixels);
            graphics.Clear(Color.Black);
            graphics.Draw(texture);
            var ms = new MemoryStream();
            graphics.SaveAsPng(ms);
            ms.Seek(0, SeekOrigin.Begin);


            // Recreate same texture from framebuffer data, draw and compare
            var sameTexture = TextureFactory.FromStream(graphics, ms,
                TextureScalingQuality.Nearest, new PngDecoder());
            var samePixels = new ColorRGB888[pixels.Length];
            graphics.Clear(Color.Black);
            graphics.Draw(texture);
            graphics.ReadPixels(ref samePixels);

            samePixels.Should().Equal(pixels);
        }

    }
}