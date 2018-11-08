using Imagini;
using Xunit;

namespace Tests
{
    public class DisplayTest : TestBase
    {
        [Fact]
        public void ShouldEnumerateDisplays()
        {
            PrintTestName();
            Assert.NotNull(Display.All);
#if !HEADLESS
            Assert.NotEmpty(Display.All);
#endif
        }       

#if !HEADLESS
        [Fact]
        public void ShouldEnumerateDisplayModes()
        {
            PrintTestName();
            foreach(var display in Display.All)
            {
                Assert.NotEmpty(display.Modes);
                Assert.NotNull(display.Name);
                Assert.Contains(display.CurrentMode, display.Modes);
                Assert.Contains(display.DesktopMode, display.Modes);
            }
        }
    }
#endif
}