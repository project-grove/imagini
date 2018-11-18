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
                Assert.Contains(display.CurrentMode, display.Modes);
                Assert.Contains(display.DesktopMode, display.Modes);
            }
        }
    }
#endif
}