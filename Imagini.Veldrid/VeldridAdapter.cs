using System;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Imagini.Veldrid
{
	internal static class VeldridAdapter
	{
		public static Sdl2Window GetSdl2Window(Window window, bool threadedProcessing = false)
		{
			return new Sdl2Window(window.Handle, threadedProcessing);
		}

		public static WindowCreateInfo GetWindowCreateInfo(WindowSettings settings)
		{
			return new WindowCreateInfo(
				0,
				0,
				settings.WindowWidth,
				settings.WindowHeight,
				GetWindowState(settings.WindowMode),
				settings.Title
			);
		}

		public static WindowState GetWindowState(WindowMode mode)
		{
			switch (mode)
			{
				case WindowMode.Fullscreen:
					return WindowState.FullScreen;
				case WindowMode.BorderlessFullscreen:
					return WindowState.BorderlessFullScreen;
				default:
					return WindowState.Normal;
			}
		}
	}
}
