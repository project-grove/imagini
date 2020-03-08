using FluentAssertions;
using Imagini;
using Xunit;

namespace Tests
{
    [DisplayTestMethodName]
    public class DisplayTest
    {
        [Fact]
        public void ShouldEnumerateDisplays()
        {

            Assert.NotNull(Display.All);
#if !HEADLESS
            Assert.NotEmpty(Display.All);
#endif
        }

#if !HEADLESS
        [Fact]
        public void ShouldEnumerateDisplayModes()
        {

            foreach(var display in Display.All)
            {
                Assert.NotEmpty(display.Modes);
                Assert.NotNull(display.Name);
                var currentMode = display.CurrentMode;
                var desktopMode = display.DesktopMode;

                /*
                    Note: workaround for SDL sometimes reporting current/desktop
                    mode index as -1 (ex. when launched in Xephyr)
                */
                display.Modes.Should().Contain(d =>
                    d.Width == currentMode.Width &&
                    d.Height == currentMode.Height);
                display.Modes.Should().Contain(d =>
                    d.Width == desktopMode.Width &&
                    d.Height == desktopMode.Height);
            }
        }
    }
#endif
}