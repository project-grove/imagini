using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Imagini.Drawing;
using Xunit;

namespace Tests.Drawing
{
    public class PixelFormatTest : TestBase
    {
        PixelFormat[] indexedFormats = new[] {
                PixelFormat.Format_INDEX1LSB,
                PixelFormat.Format_INDEX1MSB,
                PixelFormat.Format_INDEX4LSB,
                PixelFormat.Format_INDEX4MSB,
                PixelFormat.Format_INDEX8
            };

        PixelFormat[] packedFormats = new[] {
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

        PixelFormat[] arrayFormats = new[] {
                PixelFormat.Format_RGB24,
                PixelFormat.Format_BGR24
            };

        [Fact]
        public void ShouldCheckIfFormatIsIndexed()
        {
            PrintTestName();
            Assert.All(indexedFormats, format =>
            {
                format.IsIndexed().Should().BeTrue();
                format.IsPacked().Should().BeFalse();
                format.IsArray().Should().BeFalse();
                format.IsFourCC().Should().BeFalse();
            });
        }

        [Fact]
        public void ShouldCheckIfFormatIsFourCC()
        {
            PrintTestName();
            var formats = new[] {
                PixelFormat.Format_YV12,
                PixelFormat.Format_IYUV,
                PixelFormat.Format_YUY2,
                PixelFormat.Format_UYVY,
                PixelFormat.Format_YVYU,
            };
            Assert.All(formats, format =>
            {
                format.IsFourCC().Should().BeTrue();
                format.IsIndexed().Should().BeFalse();
                format.IsPacked().Should().BeFalse();
                format.IsArray().Should().BeFalse();
            });
        }

        [Fact]
        public void ShouldCheckIfFormatIsPacked()
        {
            Assert.All(packedFormats, format =>
            {
                format.IsPacked().Should().BeTrue();
                format.IsIndexed().Should().BeFalse();
                format.IsArray().Should().BeFalse();
                format.IsFourCC().Should().BeFalse();
            });
        }

        [Fact]
        public void ShouldCheckIfFormatIsArray()
        {
            Assert.All(arrayFormats, format =>
            {
                format.IsArray().Should().BeTrue();
                format.IsIndexed().Should().BeFalse();
                format.IsPacked().Should().BeFalse();
                format.IsFourCC().Should().BeFalse();
            });
        }

        [Fact]
        public void ShouldReturnComponentType()
        {
            PrintTestName();
            Func<IEnumerable<PixelFormat>, IEnumerable<PixelComponentType>> ComponentTypeOf =
                input => input.Select(f => f.GetComponentType()).Distinct();

            ComponentTypeOf(indexedFormats).Should().BeEquivalentTo(PixelComponentType.Bitmap);
            ComponentTypeOf(packedFormats).Should().BeEquivalentTo(PixelComponentType.Packed);
            ComponentTypeOf(arrayFormats).Should().BeEquivalentTo(PixelComponentType.Array);
        }

        [Fact]
        public void ShouldReturnPixelLayout()
        {
            PrintTestName();
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
                .ToDictionary(pair => pair.Key, pair => pair.ToList()
                    .Select(name => Enum.Parse<PixelFormat>(name)));

            var hasAlpha = formats[true];
            var noAlpha = formats[false];
            Assert.All(hasAlpha, format => Assert.True(format.HasAlpha()));
            Assert.All(noAlpha, format => Assert.False(format.HasAlpha()));
        }
    }
}