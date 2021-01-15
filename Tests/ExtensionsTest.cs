using System;
using FluentAssertions;
using Imagini;
using Imagini.Core;
using Xunit;
using static SDL2.SDL;
using static SDL2.SDL_error;

namespace Tests
{
    [DisplayTestMethodName]
    public class ExtensionsTest
    {
        public ExtensionsTest()
        {
            if (SDL_Init(SDL_INIT_EVERYTHING) != 0)
                throw new Exception($"Could not init SDL: {SDL_GetError()}");
        }

        [Theory]
        [InlineData(Keycode.RETURN, Scancode.RETURN)]
        [InlineData(Keycode.SPACE, Scancode.SPACE)]
        public void ShouldConvertKeycodeToScancode(Keycode keycode, Scancode expected)
        {
            var actual = keycode.ToScancode();
            actual.Should().NotBe(Scancode.UNKNOWN);
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(Scancode.A, Keycode.a)]
        [InlineData(Scancode.RETURN, Keycode.RETURN)]
        public void ShouldConvertScancodeToKeycode(Scancode scancode, Keycode expected)
        {
            var actual = scancode.ToKeycode();
            actual.Should().NotBe(Keycode.UNKNOWN);
            actual.Should().Be(expected);
        }
    }
}