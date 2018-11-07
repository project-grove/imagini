using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Imagini.Drawing;
using Xunit;

namespace Tests.Drawing
{
    public class PixelFormatTest
    {
        [Fact]
        public void ShouldCheckIfFormatIsIndexed()
        {
            var formats = new[] {
                PixelFormat.Format_INDEX1LSB,
                PixelFormat.Format_INDEX1MSB,
                PixelFormat.Format_INDEX4LSB,
                PixelFormat.Format_INDEX4MSB,
                PixelFormat.Format_INDEX8
            };
            Assert.All(formats, format =>
            {
                Assert.True(format.IsIndexed());
                Assert.False(format.IsFourCC());
                format.GetComponentType().Should().Be(PixelComponentType.Bitmap);
            });
        }

        [Fact]
        public void ShouldCheckIfFormatIsFourCC()
        {
            var formats = new[] {
                PixelFormat.Format_YV12,
                PixelFormat.Format_IYUV,
                PixelFormat.Format_YUY2,
                PixelFormat.Format_UYVY,
                PixelFormat.Format_YVYU,
            };
            Assert.All(formats, format =>
            {
                Assert.True(format.IsFourCC());
                Assert.False(format.IsIndexed());
            });
        }

        // TODO IsPacked, IsArray

        [Fact]
        public void ShouldReturnComponentType()
        {
            var bitmap = new[] {
                PixelFormat.Format_INDEX1LSB,
                PixelFormat.Format_INDEX1MSB,
                PixelFormat.Format_INDEX4LSB,
                PixelFormat.Format_INDEX4MSB,
                PixelFormat.Format_INDEX8
            };
            bitmap.Select(f => f.GetComponentType()).Distinct()
                .Should().BeEquivalentTo(PixelComponentType.Bitmap);

            var packed = new[] {
                PixelFormat.Format_RGB332,
                PixelFormat.Format_RGB444,
                PixelFormat.Format_RGB555,
                PixelFormat.Format_BGR555,
                PixelFormat.Format_ARGB4444,
                PixelFormat.Format_RGBA4444,
                PixelFormat.Format_ABGR4444,
                PixelFormat.Format_BGRA4444,
                PixelFormat.Format_ARGB1555,
                PixelFormat.Format_RGBA5551,
                PixelFormat.Format_ABGR1555,
                PixelFormat.Format_BGRA5551,
                PixelFormat.Format_RGB565,
                PixelFormat.Format_RGBX8888,
                PixelFormat.Format_BGR888,
                PixelFormat.Format_BGRX8888,
                PixelFormat.Format_ARGB8888,
                PixelFormat.Format_RGBA8888,
                PixelFormat.Format_ABGR8888,
                PixelFormat.Format_BGRA8888,
                PixelFormat.Format_ARGB2101010
            };
            packed.Select(f => f.GetComponentType()).Distinct()
                .Should().BeEquivalentTo(PixelComponentType.Packed);

            var array = new[] {
                PixelFormat.Format_RGB24,
                PixelFormat.Format_BGR24
            };
            array.Select(f => f.GetComponentType()).Distinct()
                .Should().BeEquivalentTo(PixelComponentType.Array);
        }

        [Fact]
        public void ShouldReturnPixelLayout()
        {
            var pairs = new Dictionary<PixelFormat, PixelLayout>()
            {
                { PixelFormat.Format_RGB332, PixelLayout.Packed332 },
                { PixelFormat.Format_RGB444, PixelLayout.Packed4444 },
                { PixelFormat.Format_RGB555, PixelLayout.Packed1555 },
                { PixelFormat.Format_BGR555, PixelLayout.Packed1555 },
                { PixelFormat.Format_ARGB4444, PixelLayout.Packed4444 },
                { PixelFormat.Format_RGBA4444, PixelLayout.Packed4444 },
                { PixelFormat.Format_ABGR4444, PixelLayout.Packed4444 },
                { PixelFormat.Format_BGRA4444, PixelLayout.Packed4444 },
                { PixelFormat.Format_ARGB1555, PixelLayout.Packed1555 },
                { PixelFormat.Format_RGBA5551, PixelLayout.Packed5551 },
                { PixelFormat.Format_ABGR1555, PixelLayout.Packed1555 },
                { PixelFormat.Format_BGRA5551, PixelLayout.Packed5551 },
                { PixelFormat.Format_RGB565, PixelLayout.Packed565 },
                { PixelFormat.Format_RGBX8888, PixelLayout.Packed8888 },
                { PixelFormat.Format_BGR888, PixelLayout.Packed8888 },
                { PixelFormat.Format_BGRX8888, PixelLayout.Packed8888 },
                { PixelFormat.Format_ARGB8888, PixelLayout.Packed8888 },
                { PixelFormat.Format_RGBA8888, PixelLayout.Packed8888 },
                { PixelFormat.Format_ABGR8888, PixelLayout.Packed8888 },
                { PixelFormat.Format_BGRA8888, PixelLayout.Packed8888 },
                { PixelFormat.Format_ARGB2101010, PixelLayout.Packed2101010 },
            };
            foreach (var pair in pairs)
                pair.Key.GetLayout().Should().Be(pair.Value);
        }

        [Fact]
        public void ShouldIndicateAlphaChannelExistence()
        {
            var formats = Enum.GetNames(typeof(PixelFormat))
                .Where(name => name.StartsWith("Format_"))
                .GroupBy(name => name.Contains("A"))
                .ToDictionary(pair => pair.Key, pair => pair.ToList());
            
            var hasAlpha = formats[true].Select(name => Enum.Parse<PixelFormat>(name));
            var noAlpha = formats[false].Select(name => Enum.Parse<PixelFormat>(name));
            Assert.All(hasAlpha, format => Assert.True(format.HasAlpha()));
            Assert.All(noAlpha, format => Assert.False(format.HasAlpha()));
        }
    }
}