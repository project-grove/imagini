using System;

using static SDL2.SDL_events;
using static SDL2.SDL_mouse;
using static SDL2.SDL_timer;
using static SDL2.SDL_render;
using static Imagini.ErrorHandler;
using System.Diagnostics;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

/// <summary>
/// The core namespace.
/// </summary>
namespace Imagini
{
	/// <summary>
	/// Base app class which instantiates a window and event loop.
	/// Derive from this if you want to provide your own renderer for the
	/// game loop.
	/// </summary>
	public abstract class AppBase : IDisposable
	{
		/// <summary>
		/// Returns the app's window.
		/// </summary>
		public Window Window { get; protected set; }
		/// <summary>
		/// Provides access to the events sent to app's window.
		/// </summary>
		public Events Events { get; private set; }
		private EventManager.EventQueue _eventQueue;

		/// <summary>
		/// Returns total number of milliseconds since when the library was initialized.
		/// </summary>
		public static long TotalTime
		{
			get => SDL_GetTicks();
		}

		/// <summary>
		/// Gets the total app running time.
		/// </summary>
		public TimeSpan ElapsedAppTime { get; private set; }

		/// <summary>
		/// Gets or sets if the system mouse cursor should be visible.
		/// </summary>
		public bool IsMouseVisible
		{
			get => TryGet(() => SDL_ShowCursor(SDL_QUERY), "SDL_ShowCursor") > 0;
			set => TryGet(() => SDL_ShowCursor(value ? 1 : 0), "SDL_ShowCursor");
		}

		/// <summary>
		/// Gets or sets whether the mouse should be captured by the window.
		/// If enabled, mouse events will report only relative movement and
		/// the cursor should stay fixed (useful for 3D cameras).
		/// </summary>
		/// <seealso cref="MousePosition" />
		public bool CaptureMouse
		{
			get => TryGet(() => SDL_GetRelativeMouseMode(), "SDL_GetRelativeMouseMode") > 0;
			set => Try(() => SDL_SetRelativeMouseMode(value ? 1 : 0), "SDL_SetRelativeMouseMode");
		}

		/// <summary>
		/// Returns the last recorded mouse position.
		/// Can be used with <see cref="CaptureMouse" /> to query relative mouse movement.
		/// </summary>
		/// <seealso cref="CaptureMouse" />
		public Point MousePosition
		{
			get
			{
				int x = 0, y = 0;
				SDL_GetRelativeMouseState(ref x, ref y);
				return new Point(x, y);
			}
		}

		/// <summary>
		/// Indicates if this app is visible and have input focus.
		/// </summary>
		public bool IsActive
		{
			get => Window.Current == Window;
		}

		/// <summary>
		/// Gets or sets the target time between each frame if 
		/// <see cref="IsFixedTimeStep" /> is set to true.
		/// </summary>
		/// <remarks>Default is ~16 ms (60 FPS).</remarks>
		public TimeSpan TargetElapsedTime
		{
			get => _targetElapsedTime;
			set
			{
				if (value > MaxElapsedTime)
					throw new ArgumentOutOfRangeException("Time must be less or equal to MaxElapsedTime");
				if (value <= TimeSpan.Zero)
					throw new ArgumentOutOfRangeException("Time must be greater than zero");
				if (value > InactiveSleepTime)
					throw new ArgumentOutOfRangeException("Time must be less or equal to InactiveSleepTime");
				_targetElapsedTime = value;
			}
		}
		private TimeSpan _targetElapsedTime = TimeSpan.FromMilliseconds(16.6667);
		private Stopwatch _appStopwatch = new Stopwatch();

		/// <summary>
		/// Gets or sets the target time between each frame if the window is
		/// inactive and <see cref="IsFixedTimeStep" /> is set to true.
		/// </summary>
		/// <remarks>Default is ~33 ms (30 FPS).</remarks>
		public TimeSpan InactiveSleepTime
		{
			get => _inactiveSleepTime;
			set
			{
				if (value > MaxElapsedTime)
					throw new ArgumentOutOfRangeException("Time should be less or equal to MaxElapsedTime");
				if (value <= TimeSpan.Zero)
					throw new ArgumentOutOfRangeException("Time must be greater than zero");
				if (value < TargetElapsedTime)
					throw new ArgumentOutOfRangeException("Time should be greater or equal to TargetElapsedTime");
				_inactiveSleepTime = value;
			}
		}
		private TimeSpan _inactiveSleepTime = TimeSpan.FromMilliseconds(33.3334);

		/// <summary>
		/// Gets or sets the maximum amout of time the app will frameskip.
		/// </summary>
		/// <remarks>Defaults to 500 milliseconds.</remarks>
		public TimeSpan MaxElapsedTime
		{
			get => _maxElapsedTime;
			set
			{
				if (value <= TimeSpan.Zero)
					throw new ArgumentOutOfRangeException("Time must be greater than zero");
				if (value < TargetElapsedTime)
					throw new ArgumentOutOfRangeException("Time must be greater or equal to TargetElapsedTime");
				if (value < InactiveSleepTime)
					throw new ArgumentOutOfRangeException("Time must be greater or equal to InactiveSleepTime");
			}
		}
		private TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);
		/// <summary>
		/// Gets or sets if the time between each frame should be fixed.
		/// </summary>
		public bool IsFixedTimeStep { get; set; } = true;
		/// <summary>
		/// Indicates if the last app frame took longer than <see cref="TargetElapsedTime" />.
		/// </summary>
		public bool IsRunningSlowly { get; private set; }
		private int _slowFrameCount = 0;
		private const int c_MaxSlowFrameCount = 5;
		private long _previousTicks = 0;
		private long _accumulatedTicks = 0;


		/* -------------------------- Initialization ------------------------ */
		/// <summary>
		/// Creates a new app with the specified window settings.
		/// </summary>
		/// <remarks>
		/// If you have your own constructor, make sure to call this
		/// one because it initializes the window and the event queue.
		/// </remarks>
		public AppBase(WindowSettings windowSettings)
		: this(new Window(windowSettings ?? new WindowSettings()))
		{
		}

		public AppBase(Window window)
		{
			Window = window;
			SetupEvents();
		}

		protected AppBase()
		{ }

		public void SetupEvents()
		{
			_eventQueue = EventManager.CreateQueueFor(Window);
			Events = new Events(this);
			Events.Window.StateChanged += OnWindowStateChange;
		}

		/// <summary>
		/// Called when the app starts. Use it for the initialization logic.
		/// </summary>
		protected virtual void Initialize() { }


		/* --------------------------- App events --------------------------- */
		private bool _suppressDraw = false;
		private bool _isExiting = false;
		internal bool IsExiting => _isExiting;
		private bool _isExited = false;
		internal bool IsExited => _isExited;
		private bool _isInitialized = false;

		/// <summary>
		/// Fires when the app's window gets activated.
		/// </summary>
		public event EventHandler<EventArgs> Activated;
		/// <summary>
		/// Fires when the app's window gets deactivated.
		/// </summary>
		public event EventHandler<EventArgs> Deactivated;
		/// <summary>
		/// Fires when the app is disposed.
		/// </summary>
		public event EventHandler<EventArgs> Disposed;
		/// <summary>
		/// Fires when the user requests to exit the app. Can be cancelled
		/// through <see cref="AppExitEventArgs.Cancel" />.
		/// </summary>
		public event EventHandler<AppExitEventArgs> Exiting;
		/// <summary>
		/// Fires when the app's window gets resized.
		/// </summary>
		public event EventHandler<EventArgs> Resized;

		private void ProcessEvents()
		{
			EventManager.Poll();
			_eventQueue.ProcessAll(Events);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CheckIfInitialized()
		{
			if (!_isInitialized)
			{
				Initialize();
				_isInitialized = true;
				_appStopwatch.Start();
			}
		}

		private void OnWindowStateChange(object sender, WindowStateChangeEventArgs args)
		{
			switch (args.State)
			{
				case WindowStateChange.Shown:
				case WindowStateChange.Exposed:
				case WindowStateChange.MouseEnter:
				case WindowStateChange.FocusGained:
					Activated?.Invoke(this, new EventArgs());
					break;
				case WindowStateChange.Hidden:
				case WindowStateChange.MouseLeave:
				case WindowStateChange.FocusLost:
					Deactivated?.Invoke(this, new EventArgs());
					break;
				case WindowStateChange.Maximized:
				case WindowStateChange.Restored:
				case WindowStateChange.SizeChanged:
					Resized?.Invoke(this, new EventArgs());
					break;
				case WindowStateChange.Closed:
					RequestExit();
					break;
			}
		}

		/// <summary>
		/// Requests the app to exit. Can be cancelled by user's code.
		/// </summary>
		/// <seealso cref="Exiting" />
		public void RequestExit() => _isExiting = true;

		/// <summary>
		/// Cancels the app exit request, if it exists.
		/// </summary>
		public void CancelExitRequest() => _isExiting = false;

		/// <summary>
		/// Terminates the app loop and disposes the app.
		/// </summary>
		public void Terminate() => _isExited = true;

		/// <summary>
		/// Suppresses the <see cref="Draw" /> call for the next frame.
		/// </summary>
		public void SuppressDraw() => _suppressDraw = true;

		/* ---------------------------- App loop ---------------------------- */
		/// <summary>
		/// Resets the total elapsed app time.
		/// </summary>
		public void ResetElapsedTime()
		{
			ElapsedAppTime = TimeSpan.Zero;
			_previousTicks = 0;
			_accumulatedTicks = 0;
			_appStopwatch.Reset();
		}
		/// <summary>
		/// Runs the app loop.
		/// </summary>
		public void Run()
		{
			while (!_isExited)
				Tick();
		}

		/// <summary>
		/// Performs a single app loop tick.
		/// </summary>
		public void Tick()
		{
			if (_isExited) return;
			CheckIfNotDisposed();
			CheckIfInitialized();
			TimeSpan elapsedFrameTime;

		RetryTick:
			// Advance the current app time
			var currentTicks = _appStopwatch.Elapsed.Ticks;
			_appStopwatch.Start();
			_accumulatedTicks += (currentTicks - _previousTicks);
			_previousTicks = currentTicks;
			// If the frame took less time than specified, sleep for the
			// remaining time and try again
			if (IsFixedTimeStep && _accumulatedTicks < TargetElapsedTime.Ticks)
			{
				var sleepTime = TimeSpan.FromTicks(TargetElapsedTime.Ticks - _accumulatedTicks);
				Thread.Sleep(sleepTime);
				goto RetryTick;
			}
			// Limit the maximum frameskip time
			if (_accumulatedTicks > _maxElapsedTime.Ticks)
				_accumulatedTicks = _maxElapsedTime.Ticks;

			if (IsFixedTimeStep)
			{
				elapsedFrameTime = TargetElapsedTime;
				var stepCount = 0;
				// Perform as many fixed timestep updates as we can
				while (_accumulatedTicks >= TargetElapsedTime.Ticks && !_isExited)
				{
					ElapsedAppTime += TargetElapsedTime;
					_accumulatedTicks -= TargetElapsedTime.Ticks;
					++stepCount;
					DoUpdate(TargetElapsedTime);
				}
				// If the frame took more time than specified, then we got
				// more than one update step
				_slowFrameCount += Math.Max(0, stepCount - 1);
				// Check if we should reset the IsRunningSlowly flag
				if (IsRunningSlowly)
				{
					if (_slowFrameCount == 0)
						IsRunningSlowly = false;
				} // If the flag is not set, check if we should set it
				else if (_slowFrameCount > c_MaxSlowFrameCount)
					IsRunningSlowly = true;

				// If this frame is not too late, decrease the lag counter
				if (stepCount == 1 && _slowFrameCount > 0)
					_slowFrameCount--;

				// Set the target time for the Draw method
				elapsedFrameTime = TimeSpan.FromTicks(TargetElapsedTime.Ticks * stepCount);
			}
			else
			{
				// Perform a non-fixed timestep update
				var accumulated = TimeSpan.FromTicks(_accumulatedTicks);
				ElapsedAppTime += accumulated;
				elapsedFrameTime = accumulated;
				accumulated = TimeSpan.Zero;
				DoUpdate(elapsedFrameTime);
			}

			if (_suppressDraw)
				_suppressDraw = false;
			else
				DoDraw(elapsedFrameTime);

			// The user requests us to exit
			if (_isExiting)
			{
				var args = new AppExitEventArgs();
				Exiting?.Invoke(this, args);
				if (!args.Cancel) Terminate();
			}
			if (_isExited && !IsDisposed)
				Dispose();
			_appStopwatch.Stop();
		}

		private void DoUpdate(TimeSpan frameTime)
		{
			ProcessEvents();
			Update(frameTime);
		}

		private void DoDraw(TimeSpan frameTime)
		{
			BeforeDraw(frameTime);
			Draw(frameTime);
			AfterDraw(frameTime);
		}

		[ExcludeFromCodeCoverage]
		protected virtual void BeforeDraw(TimeSpan frameTime) { }
		[ExcludeFromCodeCoverage]
		protected virtual void AfterDraw(TimeSpan frameTime) { }

		[ExcludeFromCodeCoverage]
		protected virtual void Update(TimeSpan frameTime) { }
		[ExcludeFromCodeCoverage]
		protected virtual void Draw(TimeSpan frameTime) { }

		/* ------------------------- Disposing logic ------------------------ */
		/// <summary>
		/// Called when the app is being disposed (when exiting or by external code).
		/// </summary>
		[ExcludeFromCodeCoverage]
		protected virtual void OnDispose() { }

		private bool _isDisposed;
		/// <summary>
		/// Indicates if this app is disposed.
		/// </summary>
		public bool IsDisposed => _isDisposed;
		/// <summary>
		/// Disposes the app object and its dependencies.
		/// </summary>
		public void Dispose()
		{
			if (_isDisposed) return;
			OnDispose();
			Events.Window.StateChanged -= OnWindowStateChange;
			EventManager.DeleteQueueFor(Window);
			Window.Destroy();

			Events = null;
			_eventQueue = null;
			Window = null;
			_isDisposed = true;
			Disposed?.Invoke(this, new EventArgs());
		}

		[ExcludeFromCodeCoverage]
		private void CheckIfNotDisposed()
		{
			if (IsDisposed)
				throw new ObjectDisposedException("This app is disposed");
		}
	}

	/// <summary>
	/// App exit event args.
	/// </summary>
	public class AppExitEventArgs : EventArgs
	{
		/// <summary>
		/// Set to true if app should not exit if requested by user.
		/// </summary>
		public bool Cancel { get; set; }
	}
}