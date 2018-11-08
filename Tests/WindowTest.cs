using System.Drawing;
using FluentAssertions;
using Imagini;
using Xunit;

namespace Tests
{
#if !HEADLESS
    public class WindowTest : TestBase
    {
        [Fact]
        public void ShouldCreateWindow()
        {
            PrintTestName();
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
            window.Destroy();
        }

        [Fact]
        public void ShouldSetMinimumAndMaximumSize()
        {
            PrintTestName();
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
            window.Destroy();
        }

        [Fact]
        public void ShouldSetWindowSize()
        {
            PrintTestName();
            var startSize = new Size(100, 50);
            var newSize = startSize * 2;
            var settings = new WindowSettings()
            {
                IsVisible = false,
                WindowWidth = startSize.Width,
                WindowHeight = startSize.Height
            };
            var window = new Window(settings);
            window.Size.Should().Be(startSize);
            window.Size = newSize;
            window.Size.Should().Be(newSize);
            window.Destroy();
        }

        // TODO FIXME Looks like the problem is in the event queue
        /*
        [Fact]
        public void ShouldMaximizeMinimizeAndRestoreIfResizable()
        {
            var size = new Size(100, 50);
            var settings = new WindowSettings()
            {
                WindowWidth = size.Width,
                WindowHeight = size.Height,
                IsResizable = true
            };
            var window = new Window(settings);
            window.IsMinimized.Should().BeFalse();
            window.IsMaximized.Should().BeFalse();
            window.Maximize();
            EventManager.Poll();
            window.IsMinimized.Should().BeFalse();
            window.IsMaximized.Should().BeTrue();
            window.Minimize();
            EventManager.Poll();
            window.IsMinimized.Should().BeTrue();
            window.IsMaximized.Should().BeFalse();
            window.Restore();
            EventManager.Poll();
            window.IsMinimized.Should().BeFalse();
            window.IsMaximized.Should().BeFalse();
            window.Destroy();
        }
        */

        [Fact]
        public void ShouldToggleVSync()
        {
            PrintTestName();
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
            window.Destroy();
        }

        [Fact]
        public void ShouldReportWindowCount()
        {
            PrintTestName();
            var settings = new WindowSettings()
            {
                WindowWidth = 100,
                WindowHeight = 100,
            };
            Window.Windows.Should().BeEmpty();
            Assert.Null(Window.Current);

            var window1 = new Window(settings);
            Window.Windows.Should().HaveCount(1);
            Window.Windows.Should().Contain(window1);
            Window.OverrideCurrentWith(window1);
            Window.Current.Should().Be(window1);

            var window2 = new Window(settings);
            Window.Windows.Should().HaveCount(2);
            Window.Windows.Should().Contain(window2);
            Window.OverrideCurrentWith(window2);
            Window.Current.Should().Be(window2);

            Window.OverrideCurrentWith(null);
            window1.Destroy();
            window2.Destroy();
        }
    }
#endif
}