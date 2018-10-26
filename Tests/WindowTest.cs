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
            var settings = new WindowSettings()
            {
                VSync = false
            };
            var window = new Window(settings);
            settings.FullscreenDisplayMode = window.Display.DesktopMode;

            Assert.False(window.Settings.IsFullscreen);
            Assert.False(window.Settings.VSync);
            settings.VSync = settings.IsFullscreen = true;
            window.Apply(settings);
            Assert.True(window.Settings.IsFullscreen);
            Assert.True(window.Settings.VSync);
            window.Dispose();
        }
    }
#endif
}