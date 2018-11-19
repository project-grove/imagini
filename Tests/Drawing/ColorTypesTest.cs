using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Imagini.Drawing;
using Xunit;

namespace Tests.Drawing
{
    [DisplayTestMethodName]
    public class ColorTypesTest
    {
        [Theory]
        [ClassData(typeof(ColorDataGenerator))]
        public void ShouldConvertToColorAndBack(dynamic color)
        {
            var convertedToColor = (Color)color;
            var inversedColor = Activator.CreateInstance(color.GetType(),
                Color.FromArgb(~convertedToColor.ToArgb())
            );
            var sameColor = Activator.CreateInstance(color.GetType(), convertedToColor);
            
            Assert.Equal(color, sameColor);
            Assert.NotEqual(color, inversedColor);
        }
    }

    public class ColorDataGenerator : IEnumerable<object[]>
    {
        Color TestColor = Color.FromArgb(unchecked((int)0xDEADBEEF));

        private readonly List<object[]> _data = new List<object[]>();
        private static readonly List<Type> _colorTypes = new List<Type>
        {
            typeof(ColorRGBA8888),
            typeof(ColorARGB8888),
            typeof(ColorRGB888)
        };

        public ColorDataGenerator()
        {
            foreach (var type in _colorTypes)
            {
                var instance = Activator.CreateInstance(type, TestColor);
                _data.Add(new object[] { instance });
            }
        }

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}