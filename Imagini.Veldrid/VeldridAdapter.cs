using System;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using static SDL2.SDL_video;

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
				SDL_WINDOWPOS_CENTERED_DISPLAY(settings.DisplayIndex),
				SDL_WINDOWPOS_CENTERED_DISPLAY(settings.DisplayIndex),
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
