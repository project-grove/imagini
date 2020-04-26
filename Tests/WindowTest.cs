using System.Drawing;
using System.Threading;
using FluentAssertions;
using Imagini;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
#if !HEADLESS
	[DisplayTestMethodName]
	public class WindowTest
	{
		private readonly ITestOutputHelper output;

		public WindowTest(ITestOutputHelper output)
		{
			this.output = output;
		}

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
			Assert.Equal(WindowMode.Windowed, window.Mode);
			Assert.Equal(0, window.DisplayIndex);
			Assert.False(window.IsVisible);
			window.Destroy();
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
			window.Destroy();
		}

		[Fact]
		public void ShouldSetWindowSize()
		{

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

		[Fact(Timeout = 5000)]
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
			Window.OverrideCurrentWith(window);
			window.IsMinimized.Should().BeFalse();
			window.IsMaximized.Should().BeFalse();

			var attempts = 0;
			window.Maximize();
			while (!window.IsMaximized)
			{
				EventManager.Pump(); // wait until OS sends us a signal
				Thread.Sleep(100);
				attempts++;
				if (attempts > 5)
				{
					output.WriteLine("Warning - could not receive window maximize signal - your desktop environment may not support it, but if it should, it may indicate a bug");
					attempts = 0;
					break;
				}
			}
			window.Minimize();
			while (!window.IsMinimized)
			{
				EventManager.Pump();
				Thread.Sleep(100);
				attempts++;
				if (attempts > 5)
				{
					output.WriteLine("Warning - could not receive window minimize signal - your desktop environment may not support it, but if it should, it may indicate a bug");
					attempts = 0;
					break;
				}
			}
			window.Restore();
			while (window.IsMinimized)
			{
				EventManager.Pump();
				Thread.Sleep(100);
				attempts++;
				if (attempts > 5)
				{
					output.WriteLine("Warning - could not receive window restore signal - your desktop environment may not support it, but if it should, it may indicate a bug");
					attempts = 0;
					break;
				}
			}
			Window.OverrideCurrentWith(null);
			window.Destroy();
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
			window.Destroy();
		}

		[Fact]
		public void ShouldReportWindowCount()
		{

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