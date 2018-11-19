using System;
using System.Collections;
using System.Collections.Generic;
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
            graphics.Clear(Color.Black);
            graphics.ReadPixels(ref readBuffer);
            readBuffer.Distinct().Should().BeEquivalentTo(new ColorRGB888());

            graphics.Clear(color);
            app.Tick();
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
            graphics.Clear(Color.Black);
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

        [Theory]
        [ClassData(typeof(RectangleDataGenerator))]
        public void ShouldDrawInSpecifiedRectangles(Rectangle? src, Rectangle? dst)
        {
            var color = new ColorRGB888(Color.CornflowerBlue);
            var size = graphics.OutputSize;
            var texture = graphics.CreateTexture(
                dst?.Width ?? size.Width,
                dst?.Height ?? size.Height);
            var pixels = Enumerable.Repeat(
                color,
                texture.GetPixelBufferSize<ColorRGB888>(src)
            ).ToArray();
            texture.SetPixels(ref pixels, src);

            graphics.Clear(Color.Black);
            graphics.Draw(texture, src, dst);

            var portion = Project(texture, src, dst);
            var actual = new ColorRGB888[graphics.GetPixelBufferSize(portion)];
            graphics.ReadPixels(ref actual, portion);
            actual.Distinct().Should().BeEquivalentTo(color);
        }

        [Theory]
        [ClassData(typeof(RectangleAngleAndCenterDataGenerator))]
        public void ShouldRotateAndDraw(Rectangle? src, Rectangle? dst,
            int angleIn90DegIncrements, Point? center)
        {
            var color = new ColorRGB888(Color.CornflowerBlue);
            var size = graphics.OutputSize;
            var texture = graphics.CreateTexture(
                dst?.Width ?? size.Width,
                dst?.Height ?? size.Height);
            var pixels = new ColorRGB888[
                texture.GetPixelBufferSize<ColorRGB888>(src)
            ];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new ColorRGB888(Color.FromArgb(
                    i % 255, i % 255, i % 255
                ));
            texture.SetPixels(ref pixels, src);

            for (int i = 0; i < angleIn90DegIncrements; i++)
                pixels = RotateArrayClockwise(pixels,
                    i % 2 == 0 ? texture.Width : texture.Height,
                    i % 2 == 0 ? texture.Height : texture.Width);

            graphics.Clear(Color.Black);
            graphics.Draw(texture, src, dst, angleIn90DegIncrements * 90, center);

            var portion = Project(texture, src, dst, angleIn90DegIncrements, center);
            var actual = new ColorRGB888[graphics.GetPixelBufferSize(portion)];
            graphics.ReadPixels(ref actual, portion);

            actual.Should().Equal(pixels);
        }

        [Theory]
        [InlineData(BlendMode.Add)]
        [InlineData(BlendMode.AlphaBlend)]
        [InlineData(BlendMode.Modulate)]
        public void ShouldGetAndSetGlobalBlendMode(BlendMode mode)
        {
            graphics.SetBlendMode(BlendMode.None);
            graphics.GetBlendMode().Should().Be(BlendMode.None);
            graphics.SetBlendMode(mode);
            graphics.GetBlendMode().Should().Be(mode);
        }

        [Fact]
        public void ShouldGetAndSetClipRectangle()
        {
            graphics.GetClipRectangle().Should().BeNull();
            var rectangle = new Rectangle(5, 10, 15, 20);
            graphics.SetClipRectangle(rectangle);
            graphics.GetClipRectangle().Should().Be(rectangle);
            graphics.SetClipRectangle(null);
            graphics.GetClipRectangle().Should().BeNull();
        }

        [Fact]
        public void ShouldGetAndSetViewport()
        {
            var wholeTarget = new Rectangle(Point.Empty, graphics.OutputSize);
            graphics.GetViewport().Should().Be(wholeTarget);
            var rectangle = new Rectangle(5, 10, 15, 20);
            graphics.SetViewport(rectangle);
            graphics.GetViewport().Should().Be(rectangle);
            graphics.SetViewport(null);
            graphics.GetViewport().Should().Be(wholeTarget);
        }

        [Fact]
        public void ShouldGetAndSetScale()
        {
            graphics.GetScale().Should().Be(new SizeF(1.0f, 1.0f));
            var expected = new SizeF(2.0f, 2.0f);
            graphics.SetScale(expected);
            graphics.GetScale().Should().Be(expected);
        }

        [Fact]
        public void ShouldGetAndSetLogicalSize()
        {
            graphics.GetLogicalSize().Should().Be(Size.Empty);
            var expected = graphics.OutputSize / 2;
            graphics.SetLogicalSize(expected);
            graphics.GetLogicalSize().Should().Be(expected);
        }

        [Fact]
        public void ShouldDrawLine()
        {
            var color = Color.CornflowerBlue;
            var rect = new Rectangle(0, 0, 5, 3);
            var expected = new []
            {
                0, 0, 0, 0, 0,
                0, 1, 1, 1, 0,
                0, 0, 0, 0, 0,
            }.Select(v => v > 0 ? color : Color.Black)
                .Select(c => new ColorRGB888(c))
                .ToArray();

            graphics.Clear(Color.Black);
            graphics.SetDrawingColor(color);
            graphics.DrawLine(new Point(1, 1), new Point(3, 1));

            var pixels = new ColorRGB888[graphics.GetPixelBufferSize(rect)];
            graphics.ReadPixels(ref pixels, rect);
            pixels.Should().Equal(expected);
        }
        [Fact]
        public void ShouldDrawLines()
        {
            var color = Color.CornflowerBlue;
            var rect = new Rectangle(0, 0, 5, 5);
            var expected = new []
            {
                1, 1, 1, 1, 1,
                0, 0, 0, 0, 1,
                0, 0, 0, 0, 1,
                0, 0, 0, 0, 1,
                0, 0, 0, 0, 1,
            }.Select(v => v > 0 ? color : Color.Black)
                .Select(c => new ColorRGB888(c))
                .ToArray();

            graphics.Clear(Color.Black);
            graphics.SetDrawingColor(color);
            graphics.DrawLines(
                new Point(0, 0),
                new Point(4, 0),
                new Point(4, 4)
            );

            var pixels = new ColorRGB888[graphics.GetPixelBufferSize(rect)];
            graphics.ReadPixels(ref pixels, rect);
            pixels.Should().Equal(expected);
        }

        [Fact]
        public void ShouldDrawPoint()
        {
            var color = Color.CornflowerBlue;
            var rect = new Rectangle(0, 0, 5, 5);
            var expected = new []
            {
                0, 0, 0, 0, 0,
                0, 0, 0, 0, 0,
                0, 1, 0, 0, 0,
                0, 0, 0, 0, 0,
                0, 0, 0, 0, 0,
            }.Select(v => v > 0 ? color : Color.Black)
                .Select(c => new ColorRGB888(c))
                .ToArray();

            graphics.Clear(Color.Black);
            graphics.SetDrawingColor(color);
            graphics.DrawPoint(new Point(1, 2));

            var pixels = new ColorRGB888[graphics.GetPixelBufferSize(rect)];
            graphics.ReadPixels(ref pixels, rect);
            pixels.Should().Equal(expected);
        }

        [Fact]
        public void ShouldDrawPoints()
        {
            var color = Color.CornflowerBlue;
            var rect = new Rectangle(0, 0, 5, 5);
            var expected = new []
            {
                0, 0, 0, 0, 0,
                0, 0, 1, 0, 0,
                0, 0, 0, 0, 0,
                0, 1, 0, 1, 0,
                0, 0, 0, 0, 0,
            }.Select(v => v > 0 ? color : Color.Black)
                .Select(c => new ColorRGB888(c))
                .ToArray();

            graphics.Clear(Color.Black);
            graphics.SetDrawingColor(color);
            graphics.DrawPoints(
                new Point(1, 3),
                new Point(2, 1),
                new Point(3, 3)
            );

            var pixels = new ColorRGB888[graphics.GetPixelBufferSize(rect)];
            graphics.ReadPixels(ref pixels, rect);
            pixels.Should().Equal(expected);
        }

        [Fact]
        public void ShouldDrawRect()
        {
            var color = Color.CornflowerBlue;
            var rect = new Rectangle(0, 0, 5, 4);
            // Ignoring the bottom line due to rendering inconsistency bug
            // https://bugzilla.libsdl.org/show_bug.cgi?id=3182
            var expected = new []
            {
                1, 1, 1, 1, 1,
                1, 0, 0, 0, 1,
                1, 0, 0, 0, 1,
                1, 0, 0, 0, 1,
            }.Select(v => v > 0 ? color : Color.Black)
                .Select(c => new ColorRGB888(c))
                .ToArray();

            graphics.Clear(Color.Black);
            graphics.SetDrawingColor(color);
            graphics.DrawRect(new Rectangle(0, 0, 5, 5));

            var pixels = new ColorRGB888[graphics.GetPixelBufferSize(rect)];
            graphics.ReadPixels(ref pixels, rect);
            pixels.Should().Equal(expected);
        }

        [Fact]
        public void ShouldDrawRects()
        {
            var color = Color.CornflowerBlue;
            var rect = new Rectangle(0, 0, 6, 4);
            // Ignoring the bottom line due to rendering inconsistency bug
            // https://bugzilla.libsdl.org/show_bug.cgi?id=3182
            var expected = new []
            {
                1, 1, 1, 1, 1, 1,
                1, 0, 1, 1, 0, 1,
                1, 0, 1, 1, 0, 1,
                1, 0, 1, 1, 0, 1,
            }.Select(v => v > 0 ? color : Color.Black)
                .Select(c => new ColorRGB888(c))
                .ToArray();

            graphics.Clear(Color.Black);
            graphics.SetDrawingColor(color);
            graphics.DrawRects(
                new Rectangle(0, 0, 3, 5),
                new Rectangle(3, 0, 3, 5)
            );

            var pixels = new ColorRGB888[graphics.GetPixelBufferSize(rect)];
            graphics.ReadPixels(ref pixels, rect);
            pixels.Should().Equal(expected);
        }

        [Fact]
        public void ShouldFillRect()
        {
            var color = Color.CornflowerBlue;
            var rect = new Rectangle(0, 0, 5, 5);
            var expected = new []
            {
                0, 0, 0, 0, 0,
                0, 1, 1, 1, 0,
                0, 1, 1, 1, 0,
                0, 1, 1, 1, 0,
                0, 0, 0, 0, 0,
            }.Select(v => v > 0 ? color : Color.Black)
                .Select(c => new ColorRGB888(c))
                .ToArray();

            graphics.Clear(Color.Black);
            graphics.SetDrawingColor(color);
            graphics.FillRect(new Rectangle(1, 1, 3, 3));

            var pixels = new ColorRGB888[graphics.GetPixelBufferSize(rect)];
            graphics.ReadPixels(ref pixels, rect);
            pixels.Should().Equal(expected);
        }

        [Fact]
        public void ShouldFillRects()
        {
            var color = Color.CornflowerBlue;
            var rect = new Rectangle(0, 0, 5, 5);
            var expected = new []
            {
                1, 1, 1, 0, 0,
                1, 1, 1, 0, 0,
                1, 1, 1, 0, 0,
                0, 0, 0, 1, 1,
                0, 0, 0, 1, 1
            }.Select(v => v > 0 ? color : Color.Black)
                .Select(c => new ColorRGB888(c))
                .ToArray();

            graphics.Clear(Color.Black);
            graphics.SetDrawingColor(color);
            graphics.FillRects(
                new Rectangle(0, 0, 3, 3),
                new Rectangle(3, 3, 2, 2)
            );

            var pixels = new ColorRGB888[graphics.GetPixelBufferSize(rect)];
            graphics.ReadPixels(ref pixels, rect);
            pixels.Should().Equal(expected);
        }

        private Rectangle? Project(Texture texture, Rectangle? src, Rectangle? dst)
        {
            if (src == null && dst == null) return null;

            var frameSize = graphics.OutputSize;
            var texSize = texture.Size;

            var srcWidth = src?.Width ?? texSize.Width;
            var srcHeight = src?.Height ?? texSize.Height;
            var dstWidth = dst?.Width ?? frameSize.Width;
            var dstHeight = dst?.Height ?? frameSize.Height;

            var texelStretchX = (float)dstWidth / srcWidth;
            var texelStretchY = (float)dstHeight / srcHeight;

            return new Rectangle(
                x: dst?.X ?? 0,
                y: dst?.Y ?? 0,
                width: (int)(srcWidth * texelStretchX),
                height: (int)(srcHeight * texelStretchY)
            );
        }

        private Rectangle? Project(Texture texture, Rectangle? src,
            Rectangle? dst, int rotationIn90DegSteps, Point? center)
        {
            rotationIn90DegSteps = Math.Max(0, rotationIn90DegSteps % 3);
            var portion = Project(texture,
                src ?? new Rectangle(0, 0, texture.Width, texture.Height),
                dst ?? new Rectangle(Point.Empty, graphics.OutputSize))
                .Value;
            if (rotationIn90DegSteps == 0)
                return portion;

            var dstWidth = rotationIn90DegSteps % 2 == 0 ?
                portion.Width : portion.Height;
            var dstHeight = rotationIn90DegSteps % 2 == 0 ?
                portion.Height : portion.Width;
            var origin = center ?? new Point(dstWidth / 2, dstHeight / 2);
            var offset = new Size(origin);
            var topLeftCorner = portion.Location + offset;
            topLeftCorner -= new Size(Rotate(origin, rotationIn90DegSteps));

            switch (rotationIn90DegSteps)
            {
                case 1:
                    topLeftCorner.X -= dstWidth;
                    break;
                case 2:
                    topLeftCorner.X -= dstWidth;
                    topLeftCorner.Y -= dstHeight;
                    break;
                case 3:
                    topLeftCorner.Y -= dstHeight;
                    break;
            }

            return new Rectangle(topLeftCorner.X, topLeftCorner.Y, dstWidth, dstHeight);
        }

        private Point Rotate(Point point, int rotationIn90DegSteps)
        {
            rotationIn90DegSteps = Math.Max(0, rotationIn90DegSteps % 3);
            switch (rotationIn90DegSteps)
            {
                case 0:
                    point = new Point(point.X, point.Y);
                    break;
                case 1:
                    point = new Point(-point.Y, point.X);
                    break;
                case 2:
                    point = new Point(-point.X, -point.Y);
                    break;
                case 3:
                    point = new Point(point.Y, -point.Y);
                    break;
            }
            return point;
        }

        private T[] RotateArrayClockwise<T>(T[] array, int srcWidth, int srcHeight)
        {
            T Get(T[] a, int x, int y, int width) => a[x + y * width];
            void Set(T[] a, int x, int y, int width, T value) => a[x + y * width] = value;

            var result = new T[array.Length];
            for (int i = 0; i < srcWidth; i++)
                for (int j = 0; j < srcHeight; j++)
                {
                    var value = Get(array, i, j, srcWidth);
                    Set(result, srcHeight - j - 1, i, srcHeight, value);
                }
            return result;
        }

        public class RectangleDataGenerator : IEnumerable<object[]>
        {
            private static List<object[]> _params = new List<object[]>
            {
                new object [] { null, null },
                new object[] { new Rectangle(1, 2, 5, 10), null },
                new object[] { null, new Rectangle(1, 2, 5, 10) },
                new object[] {
                    new Rectangle(1, 2, 5, 10),
                    new Rectangle(4, 5, 10, 20)
                }
            };

            public IEnumerator<object[]> GetEnumerator() => _params.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class RectangleAngleAndCenterDataGenerator : IEnumerable<object[]>
        {
            private static List<object[]> _params = new List<object[]>
            {
                new object [] { null, null, 0, null }, // draw as it is
                new object [] { null, null, 2, new Point(50, 25)}, // rotate 180
                new object[] {
                    new Rectangle(0, 0, 5, 10),
                    new Rectangle(50, 25, 5, 10),
                    0,
                    new Point(0, 0)
                },
                new object[] {
                    new Rectangle(0, 0, 5, 10),
                    new Rectangle(50, 25, 5, 10),
                    2,
                    new Point(0, 0)
                }
            };

            public IEnumerator<object[]> GetEnumerator() => _params.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}