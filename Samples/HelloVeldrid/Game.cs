using System;
using Imagini;
using Imagini.Veldrid;
using Veldrid;

namespace HelloVeldrid
{
	public class Game : VeldridApp
	{
		private CommandList _cmdList;

		public Game() : base(new WindowSettings()
		{
			WindowWidth = 800,
			WindowHeight = 600,
			Title = "Hello Veldrid",
		})
		{ }

		protected override void Initialize()
		{
			var factory = Graphics.ResourceFactory;
			_cmdList = factory.CreateCommandList();
			Events.Keyboard.KeyPressed += KeyPressed;
		}

		protected override void Draw(TimeSpan frameTime)
		{
			_cmdList.Begin();
			_cmdList.SetFramebuffer(Graphics.SwapchainFramebuffer);
			_cmdList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
			_cmdList.End();
			Graphics.SubmitCommands(_cmdList);
		}

		protected override void AfterDraw(TimeSpan frameTime)
		{
			Graphics.SwapBuffers();
		}

		private void KeyPressed(object sender, KeyboardEventArgs args)
		{
			switch (args.Key.Scancode)
			{
				case Scancode.ESCAPE:
					Environment.Exit(0);
					break;
				case Scancode.NUMBER_1:
					Window.Apply(new WindowSettings()
					{
						WindowWidth = 640,
						WindowHeight = 480,
						Title = "640x480"
					});
					break;
				case Scancode.NUMBER_2:
					Window.Apply(new WindowSettings()
					{
						WindowWidth = 800,
						WindowHeight = 600,
						Title = "800x600"
					});
					break;
				case Scancode.NUMBER_3:
					Window.Apply(new WindowSettings()
					{
						WindowWidth = 1280,
						WindowHeight = 720,
						Title = "1280x720"
					});
					break;
			}
		}
	}
}