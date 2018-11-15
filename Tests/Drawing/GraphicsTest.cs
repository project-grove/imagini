using System;
using System.Drawing;
using System.Linq;
using FluentAssertions;
using Imagini;
using Imagini.Drawing;
using Xunit;

namespace Tests.Drawing
{
    public class GraphicsTest : IDisposable
    {
        Color TestColor = Color.FromArgb(unchecked((int)0xDEADBEEF));
        SampleApp app = new SampleApp(visible: false);
        Graphics graphics => app.Graphics;

        public void Dispose()
        {
            app.Dispose();
        }

        [Theory]
        [InlineData(TextureAccess.Static, PixelFormat.Format_ARGB8888)]
        [InlineData(TextureAccess.Streaming, PixelFormat.Format_RGBA8888)]
        [InlineData(TextureAccess.Streaming, PixelFormat.Format_ABGR8888)]
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
                0, 0, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 0,
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
        public void ShouldWriteAndReadPixels()
        {
            var color = Color.CornflowerBlue.WithoutName();
            var size = graphics.OutputSize;
            var renderTarget = graphics.CreateTexture(size.Width, size.Height,
                access: TextureAccess.Target);
            graphics.SetRenderTarget(renderTarget);

            var readBuffer = new ColorRGB888[graphics.PixelCount];
            graphics.ReadPixels(ref readBuffer);
            readBuffer.Distinct().Should().BeEquivalentTo(new ColorRGB888());

            graphics.Clear(color);
            graphics.ReadPixels(ref readBuffer);
            readBuffer.Distinct().Should().BeEquivalentTo(new ColorRGB888(color));
        }
    }
}