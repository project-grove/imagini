using System.Drawing;
using Imagini;
using Xunit;

namespace Tests
{
#if !HEADLESS
    public class WindowTest
    {
        [Fact]
        public void ShouldCreateWindow()
        {
            var expectedSize = new Size(200, 100);
            var settings = new WindowSettings()
            {
                IsVisible = false,
                WindowWidth = expectedSize.Width,
                WindowHeight = expectedSize.Height,
            };
            var window = new Window(settings);
            Assert.Equal(expectedSize, window.Size);
            Assert.Equal(expectedSize, window.SizeInPixels);
            Assert.Equal(settings.Title, window.Title);
            Assert.False(window.IsVisible);
            window.Dispose();
        }

        [Fact]
        public void ShouldSetMinimumAndMaximumSize()
        {
            var minSize = new Size(100, 50);
            var expectedSize = minSize * 2;
            var maxSize = minSize * 3;
            var settings = new WindowSettings()
            {
                IsVisible = false,
                WindowWidth = expectedSize.Width,
                WindowHeight = expectedSize.Height
            };
            var window = new Window(settings);
            window.MinimumSize = minSize;
            window.MaximumSize = maxSize;
            Assert.Equal(minSize, window.MinimumSize);
            Assert.Equal(maxSize, window.MaximumSize);
            window.Dispose();
        }

        [Fact]
        public void ShouldToggleVSync()
        {
            var expected = false;
            var settings = new WindowSettings()
            {
                IsVisible = false,
                WindowWidth = 100,
                WindowHeight = 50,
                VSync = expected
            };
            var window = new Window(settings);
            
            Assert.Equal(expected, window.Settings.VSync);
            settings.VSync = expected = true;
            window.Apply(settings);
            Assert.Equal(expected, window.Settings.VSync);

            window.Dispose();
        }
    }
#endif
}