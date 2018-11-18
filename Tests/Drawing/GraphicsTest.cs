using System;
using System.Drawing;
using System.Linq;
using FluentAssertions;
using Imagini;
using Imagini.Drawing;
using Imagini.ImageSharp;
using Xunit;

namespace Tests.Drawing
{
    [DisplayTestMethodName]
    public class GraphicsTest : IDisposable
    {
        Color TestColor = Color.FromArgb(unchecked((int)0xDEADBEEF));
        SampleApp app = new SampleApp(visible: true);
        Graphics graphics => app.Graphics;

        public void Dispose()
        {
            app.Dispose();
        }

        [Theory]
        [InlineData(TextureAccess.Static, PixelFormat.Format_ARGB8888)]
        [InlineData(TextureAccess.Streaming, PixelFormat.Format_RGBA8888)]
        [InlineData(TextureAccess.Target, PixelFormat.Format_ABGR8888)]
        public void ShouldCreateTexturesWithSpecifiedParameters(
            TextureAccess access,
            PixelFormat format
        )
        {
            var width = 20;
            var height = 10;
            var texture = graphics.CreateTexture(width, height,
                format: format, access: access);
            
            texture.Width.Should().Be(width);
            texture.Height.Should().Be(height);
            texture.Format.Should().Be(format);
            texture.Access.Should().Be(access);
        }

        [Fact]
        public void ShouldCreateTextureFromSurface()
        {
            var width = 10;
            var height = 10;
            var surface = Surface.Create(width, height);
            var texture = graphics.CreateTextureFromSurface(surface);
            texture.Width.Should().Be(width);
            texture.Height.Should().Be(height);
        }

        [Theory]
        [InlineData(unchecked((int)0xFFC0FFEE), 123, BlendMode.AlphaBlend)]
        public void ShouldGetAndSetModifiers(int colorModValue, byte alphaMod, BlendMode blendMode)
        {
            var colorMod = Color.FromArgb(colorModValue);
            var texture = graphics.CreateTexture(10, 10);

            texture.ColorMod.Should().Be(Color.White.WithoutName());
            texture.AlphaMod.Should().Be(byte.MaxValue);
            texture.BlendMode.Should().Be(BlendMode.None);

            texture.ColorMod = colorMod;
            texture.AlphaMod = alphaMod;
            texture.BlendMode = blendMode;

            texture.ColorMod.Should().Be(colorMod);
            texture.AlphaMod.Should().Be(alphaMod);
            texture.BlendMode.Should().Be(blendMode);
        }

        [Fact]
        public void ShouldLockAllTexture()
        {
            var texture = graphics.CreateTexture(6, 6, access: TextureAccess.Streaming);
            var bpp = texture.Format.GetBytesPerPixel();
            var expectedLength = texture.Width * texture.Height * bpp;
            texture.Locked.Should().BeFalse();
            var ptr = texture.Lock(out int length, out int stride);
            texture.Locked.Should().BeTrue();
            ptr.Should().NotBe(IntPtr.Zero);
            stride.Should().Be(texture.Width * bpp);
            length.Should().Be(expectedLength);
            texture.Unlock();            
            texture.Locked.Should().BeFalse();
        }

        [Fact]
        public void ShouldLockPortionOfTexture()
        {
            var texture = graphics.CreateTexture(6, 6, access: TextureAccess.Streaming);
            var bpp = texture.Format.GetBytesPerPixel();
            var rectangle = new Rectangle(2, 1, 3, 3);
            var expectedLength = new[] {
                0, 0, 0, 0, 0, 0,
                0, 0, 1, 1, 1, 0,
                0, 0, 1, 1, 1, 0,
                0, 0, 1, 1, 1, 0,
                0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0,
            }.Count(locked => locked == 1) * bpp;

            texture.Locked.Should().BeFalse();
            var ptr = texture.Lock(out int length, out int stride, rectangle);
            texture.Locked.Should().BeTrue();
            Assert.ThrowsAny<ImaginiException>(() => texture.Lock(out _, out _));
            ptr.Should().NotBe(IntPtr.Zero);
            stride.Should().Be(texture.Width * bpp);
            length.Should().Be(expectedLength);
            texture.Unlock();            
            texture.Locked.Should().BeFalse();
        }

        [Theory]
        [InlineData(TextureAccess.Static)]
        [InlineData(TextureAccess.Target)]
        public void ShouldNotAllowLockingNonStreamingTextures(TextureAccess access)
        {
            var texture = graphics.CreateTexture(1, 1, access: access);
            Assert.ThrowsAny<ImaginiException>(() => texture.Lock(out _, out _));
        }

        [Fact]
        public void ShouldGetAndSetRenderTarget()
        {
            graphics.GetRenderTarget().Should().BeNull();
            var size = graphics.OutputSize;
            var renderTarget = graphics.CreateTexture(size.Width, size.Height,
                access: TextureAccess.Target);
            graphics.SetRenderTarget(renderTarget);
            graphics.GetRenderTarget().Should().Be(renderTarget);
            graphics.SetRenderTarget(null);
            graphics.GetRenderTarget().Should().BeNull();
        }

        [Fact]
        public void ShouldReadPixelsFromFramebuffer()
        {
            var color = Color.CornflowerBlue;
            var readBuffer = new ColorRGB888[graphics.PixelCount];
            graphics.ReadPixels(ref readBuffer);
            readBuffer.Distinct().Should().BeEquivalentTo(new ColorRGB888());

            graphics.Clear(color);
            graphics.ReadPixels(ref readBuffer);
            readBuffer.Distinct().Should().BeEquivalentTo(new ColorRGB888(color));
        }

        [Fact]
        public void ShouldReadPixelsFromFramebufferInRectangle()
        {
            var color = Color.CornflowerBlue;
            var rectangle = new Rectangle(5, 5, 10, 10);
            var readBuffer = new ColorRGB888[graphics.GetPixelBufferSize(rectangle)];
            graphics.ReadPixels(ref readBuffer, rectangle);
            readBuffer.Distinct().Should().BeEquivalentTo(new ColorRGB888());

            var texture = graphics.CreateTexture(rectangle.Width, rectangle.Height);
            var textureData = Enumerable.Repeat(new ColorRGB888(color),
                texture.PixelCount).ToArray();
            texture.SetPixels(ref textureData);
            graphics.Draw(texture, dstRect: rectangle);

            graphics.ReadPixels(ref readBuffer, rectangle);
            readBuffer.Distinct().Should().BeEquivalentTo(new ColorRGB888(color));
        }

        [Fact]
        public void ShouldSetAllTexturePixels()
        {
            var color = Color.CornflowerBlue;
            var size = graphics.OutputSize;
            var texture = graphics.CreateTexture(size.Width, size.Height);
            var pixels = Enumerable.Repeat(new ColorRGB888(color), 
                texture.PixelCount)
                .ToArray();
            texture.SetPixels(ref pixels);
            graphics.Draw(texture);
            var actual = new ColorRGB888[texture.PixelCount];
            graphics.ReadPixels(ref actual);
            actual.Should().BeEquivalentTo(pixels);
        }

        [Theory]
        [InlineData(PixelFormat.Format_ARGB8888)]
        [InlineData(PixelFormat.Format_RGB888)]
        public void ShouldSetTexturePixelsInRectangle(PixelFormat format)
        {
            var screenSize = graphics.OutputSize;
            var portion = new Rectangle(5, 10, 15, 20);
            var texture = graphics.CreateTexture(screenSize.Width, screenSize.Height,
                format: format);
            var pixelCount = texture.GetPixelBufferSize<ColorRGB888>(portion);
            var pixels = new ColorRGB888[pixelCount];
            for (int i = 0; i < pixelCount; i++)
                pixels[i] = new ColorRGB888(
                    Color.FromArgb(i % 200 + 55, i % 200 + 55, i % 200 + 55));
            
            texture.SetPixels(ref pixels, portion);
            graphics.Draw(texture);
            var actual = new ColorRGB888[pixelCount];
            graphics.ReadPixels(ref actual, portion);
            actual.Should().BeEquivalentTo(pixels);
        }

        [Fact]
        public void ShouldSetAllBytes()
        {
            var width = 10;
            var height = 10;
            var format = PixelFormat.Format_RGB888;
            var texture = graphics.CreateTexture(width, height, 
                format: format);
            var pixels = new byte[width * height * format.GetBytesPerPixel()];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = 255;
            texture.SetPixels(ref pixels);
            graphics.Draw(texture);

            var actual = new ColorRGB888[texture.PixelCount];
            graphics.ReadPixels(ref actual, new Rectangle(0, 0, width, height));
            actual.Distinct().Should().BeEquivalentTo(
                new ColorRGB888(Color.White)
            );
        }

        [Fact]
        public void ShouldSetBytesInRectangle()
        {
            var width = 10;
            var height = 10;
            var rectangle = new Rectangle(5, 5, width, height);
            var format = PixelFormat.Format_RGB888;
            var texture = graphics.CreateTexture(
                graphics.OutputSize.Width,
                graphics.OutputSize.Height, 
                format: format);
            var pixels = new byte[width * height * format.GetBytesPerPixel()];

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = 255;
            texture.SetPixels(ref pixels, rectangle);
            graphics.Draw(texture);

            var actual = new ColorRGB888[width * height];
            graphics.ReadPixels(ref actual, rectangle);
            actual.Distinct().Should().BeEquivalentTo(
                new ColorRGB888(Color.White)
            );
        }
    }
}