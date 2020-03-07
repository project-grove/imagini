using System;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using static Imagini.Logger;
using static SDL2.SDL_video;

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
			GraphicsBackend? preferredBackend = null) : base()
		{
			var backend = preferredBackend ?? VeldridStartup.GetPlatformDefaultBackend();
			var WindowSettings = settings ?? new WindowSettings();

			Log.Information("Using {backend} backend", backend);
			VeldridStartup.CreateWindowAndGraphicsDevice(
				VeldridAdapter.GetWindowCreateInfo(WindowSettings),
				options,
				backend,
				out Sdl2Window sdl2window,
				out GraphicsDevice gd
			);
			Window = new Window(sdl2window.Handle);
			Graphics = gd;
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