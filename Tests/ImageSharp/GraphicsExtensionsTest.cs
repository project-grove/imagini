using System;
using System.Drawing;
using System.IO;
using System.Linq;
using FluentAssertions;
using Imagini.Drawing;
using Imagini.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using Tests.Drawing;
using Xunit;

namespace Tests.ImageSharp
{
    [DisplayTestMethodName]
    public class GraphicsExtensionsTest : IDisposable
    {
        SampleApp app = new SampleApp(visible: true);
        Graphics graphics => app.Graphics;

        public void Dispose() => app.Dispose();

        [Fact]
        public void ShouldSaveFramebufferData()
        {
            var color = Color.CornflowerBlue.WithoutName();
            graphics.Clear(color);
            
            var ms = new MemoryStream();
            graphics.SaveAsPng(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var surface = SurfaceFactory.FromStream(ms, new PngDecoder());
            
            var pixels = new ColorRGB888[surface.PixelCount];
            surface.ReadPixels(ref pixels);
            pixels.Distinct().Select(c => c.AsColor())
                .Should().BeEquivalentTo(color);
        }
    }
}