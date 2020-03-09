using System;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using static Imagini.Logger;
using static SDL2.SDL_video;
using VWindowFlags = Veldrid.Sdl2.SDL_WindowFlags;

namespace Imagini.Veldrid
{
	/// <summary>
	/// Main class which instantiates a window, an event loop and a Veldrid
	/// renderer.
	/// </summary>
	public class VeldridApp : AppBase
	{
		public GraphicsDevice Graphics { get; private set; }

		public VeldridApp(
			WindowSettings settings = null,
			GraphicsDeviceOptions options = new GraphicsDeviceOptions(),
			bool threadedProcessing = false,
			GraphicsBackend? preferredBackend = null) : base()
		{
			var backend = preferredBackend ?? VeldridStartup.GetPlatformDefaultBackend();
			var windowSettings = settings ?? new WindowSettings();

			Log.Information("Using {backend} backend", backend);
			var windowCI = VeldridAdapter.GetWindowCreateInfo(windowSettings);
			if (backend == GraphicsBackend.OpenGL || backend == GraphicsBackend.OpenGLES)
			{
				VeldridStartup.SetSDLGLContextAttributes(options, backend);
			}
			var flags = (VWindowFlags)windowSettings.GetFlags() | VWindowFlags.OpenGL;
			var sdl2window = new Sdl2Window(
				windowSettings.Title,
				windowCI.X,
				windowCI.Y,
				windowSettings.WindowWidth,
				windowSettings.WindowHeight,
				flags,
				threadedProcessing
			);
			var gd = VeldridStartup.CreateGraphicsDevice(
				sdl2window,
				options,
				backend
			);
			Graphics = gd;
			Graphics.SyncToVerticalBlank = windowSettings.VSync;
			Window = new Window(sdl2window.SdlWindowHandle);
			SetupEvents();
			Window.OnSettingsChanged += (s, e) =>
			{
				Graphics.SyncToVerticalBlank = e.VSync;
				Graphics.ResizeMainWindow((uint)e.WindowWidth, (uint)e.WindowHeight);
			};
		}

		protected override void OnDispose()
		{
			Graphics.Dispose();
		}
	}
}