using System;
using System.Collections.Generic;
using System.Linq;
using static Imagini.Internal.ErrorHandler;
using static SDL2.SDL_video;

namespace Imagini
{
    /// <summary>
    /// Defines window display mode.
    /// </summary>
    public enum WindowMode
    {
        Windowed,
        Borderless,
        Fullscreen,
        BorderlessFullscreen
    }

    /// <summary>
    /// Defines a fullscreen display mode.
    /// </summary>
    public struct DisplayMode
    {
        internal SDL_DisplayMode _mode;
        /// <summary>
        /// Fullscreen width.
        /// </summary>
        public int Width => _mode.w;
        /// <summary>
        /// Fullscreen height.
        /// </summary>
        public int Height => _mode.h;
        /// <summary>
        /// Refresh rate in Hz.
        /// </summary>
        public int RefreshRate => _mode.refresh_rate;

        internal int displayIndex;
        internal int modeIndex;

        public override string ToString() => $"{Width}x{Height} {RefreshRate}Hz";

        internal static List<DisplayMode> GetAvailable(int displayIndex)
        {
            var result = new List<DisplayMode>();
            var numModes = TryGet(() =>
                SDL_GetNumDisplayModes(displayIndex),
                "SDL_GetNumDisplayModes");
            for (var modeIndex = 0; modeIndex < numModes; modeIndex++)
            {
                var modeData = new SDL_DisplayMode();
                SDL_GetDisplayMode(displayIndex, modeIndex, ref modeData);
                result.Add(new DisplayMode()
                {
                    _mode = modeData,
                    displayIndex = displayIndex,
                    modeIndex = modeIndex
                });
            }
            return result;
        }

        internal static DisplayMode GetCurrent(int displayIndex)
        {
            var modeData = new SDL_DisplayMode();
            Try(() =>
                SDL_GetCurrentDisplayMode(displayIndex, ref modeData),
                "SDL_GetCurrentDisplayMode");
            var result = new DisplayMode()
            {
                _mode = modeData,
                displayIndex = displayIndex,
            };
            result.modeIndex = DisplayMode
                .GetAvailable(displayIndex)
                .IndexOf(result);
            return result;
        }

        internal static DisplayMode GetDesktop(int displayIndex)
        {
            var modeData = new SDL_DisplayMode();
            Try(() =>
                SDL_GetDesktopDisplayMode(displayIndex, ref modeData),
                "SDL_GetDesktopDisplayMode");
            var result = new DisplayMode()
            {
                _mode = modeData,
                displayIndex = displayIndex,
            };
            result.modeIndex = DisplayMode
                .GetAvailable(displayIndex)
                .IndexOf(result);
            return result;
        }
    }

    /// <summary>
    /// Defines app window settings.
    /// </summary>
    public class WindowSettings
    {
        /// <summary>
        /// Width of the window in pixels.
        /// </summary>
        public int WindowWidth { get; set; } = 800;
        /// <summary>
        /// Height of the window in pixels.
        /// </summary>
        public int WindowHeight { get; set; } = 600;
        /// <summary>
        /// Flag indicating if the window should be fullscreen.
        /// </summary>
        public bool IsFullscreen { get; set; } = false;
        /// <summary>
        /// Flag indicating if the window should be visible.
        /// </summary>
        public bool IsVisible { get; set; } = true;
        /// <summary>
        /// Flag indicating if the window should be borderless.
        /// </summary>
        public bool IsBorderless { get; set; } = false;
        /// <summary>
        /// Flag indicating if the window should be resizable.
        /// </summary>
        /// <remarks>Should be specified at window creation, cannot be changed later.</remarks>
        public bool IsResizable { get; set; } = false;
        /// <summary>
        /// Flag indicating if the OS should treat the window as high-DPI aware.
        /// </summary>
        /// <remarks>Should be specified at window creation, cannot be changed later.</remarks>
        // public bool AllowHighDpi { get; set; } = false;

        /// <summary>
        /// Specifies a display index on which the window should be positioned.
        /// </summary>
        public int DisplayIndex { get; set; } = 0;

        /// <summary>
        /// Gets or sets the window mode.
        /// </summary>
        public WindowMode WindowMode
        {
            get => GetWindowMode(IsFullscreen, IsBorderless);
            set
            {
                IsFullscreen =
                    value == WindowMode.Fullscreen ||
                    value == WindowMode.BorderlessFullscreen;
                IsBorderless =
                    value == WindowMode.Borderless ||
                    value == WindowMode.BorderlessFullscreen;
            }
        }

        /// <summary>
        /// Gets or sets the display mode used when the app is fullscreen.
        /// </summary>
        public DisplayMode FullscreenDisplayMode { get; set; } = DisplayMode.GetCurrent(0);

        private WindowMode GetWindowMode(bool isFullscreen, bool isBorderless)
        {
            if (!isFullscreen && !isBorderless) return WindowMode.Windowed;
            else if (!isFullscreen && isBorderless) return WindowMode.Borderless;
            else if (isFullscreen && !isBorderless) return WindowMode.Fullscreen;
            else return WindowMode.BorderlessFullscreen;
        }

        internal uint GetFlags()
        {
            var result = 0u;
            result |= (uint)SDL_WINDOWPOS_CENTERED_DISPLAY(DisplayIndex);
            if (IsFullscreen) result |= (uint)SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
            if (IsVisible)
                result |= (uint)SDL_WindowFlags.SDL_WINDOW_SHOWN;
            else
                result |= (uint)SDL_WindowFlags.SDL_WINDOW_HIDDEN;
            if (IsBorderless) result |= (uint)SDL_WindowFlags.SDL_WINDOW_BORDERLESS;
            if (IsResizable) result |= (uint)SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
            // if (AllowHighDpi) result |= (uint)SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI;
            return result;
        }

        internal void Apply(IntPtr window)
        {
            var flags = SDL_GetWindowFlags(window);
            bool alreadyFullscreen = HasFlag(flags, SDL_WindowFlags.SDL_WINDOW_FULLSCREEN);
            bool alreadyBorderless = HasFlag(flags, SDL_WindowFlags.SDL_WINDOW_BORDERLESS);

            var curMode = GetWindowMode(alreadyFullscreen, alreadyBorderless);
            var curIndex = Display.GetCurrentDisplayIndexForWindow(window);
            var targetMode = WindowMode;
            var targetIndex = DisplayIndex;

            // If the WindowMode or target display have changed, do some preparation
            ChangeVideoMode(window, curMode, targetMode, curIndex, targetIndex);
        }

        private void ChangeVideoMode(IntPtr window, WindowMode curMode, WindowMode targetMode,
            int curIndex, int targetIndex)
        {
            // Set target window size for Windowed and Borderless modes
            SDL_SetWindowSize(window, WindowWidth, WindowHeight);
            var flags = SDL_GetWindowFlags(window);
            var shouldBeFullscreen =
                (targetMode == WindowMode.Fullscreen) ||
                (targetMode == WindowMode.BorderlessFullscreen);

            // switch to windowed mode and move to corresponding display
            // if the target display for fullscreen mode have changed
            if (curIndex != targetIndex && shouldBeFullscreen)
            {
                Try(() => SDL_SetWindowFullscreen(window, 0), "SDL_SetWindowFullscreen");
                MoveToDisplay(window, targetIndex);
                curMode = HasFlag(flags, SDL_WindowFlags.SDL_WINDOW_BORDERLESS) ?
                    WindowMode.Borderless : WindowMode.Windowed;
                curIndex = targetIndex;
            }
            var alreadyFullscreen =
                (curMode == WindowMode.Fullscreen) ||
                (curMode == WindowMode.BorderlessFullscreen);
            // Set the target params for the Fullscreen mode
            if (shouldBeFullscreen)
            {
                var targetDM = FullscreenDisplayMode;
                Try(() =>
                    SDL_SetWindowDisplayMode(window, ref targetDM._mode),
                    "SDL_SetWindowDisplayMode");
            }
            if (curMode == targetMode) return;
            // Set the fullscreen mode flag
            var fullscreenOpt = 0u;
            switch(targetMode) {
                case WindowMode.BorderlessFullscreen:
                    fullscreenOpt = (uint)SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
                    break;
                case WindowMode.Fullscreen:
                    fullscreenOpt = (uint)SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
                    break;
            }
            Try(() => 
                SDL_SetWindowFullscreen(window, fullscreenOpt),
                "SDL_SetWindowFullscreen");
            // Set the window border flag if it doesn't occupy the whole screen
            // because SDL_WINDOW_FULLSCREEN_DESKTOP sets it automatically
            if (targetMode == WindowMode.Borderless)
                SDL_SetWindowBordered(window, 0);
            else if (targetMode == WindowMode.Windowed)
                SDL_SetWindowBordered(window, 1);
        }

        private void MoveToDisplay(IntPtr window, int displayIndex, int x = 0, int y = 0)
        {
            var display = Display.All[displayIndex];
            var bounds = display.Bounds;
            var targetX = bounds.x + x;
            var targetY = bounds.y + y;
            SDL_SetWindowPosition(window, targetX, targetY);
        }

        private bool HasFlag(uint flags, SDL_WindowFlags flag) =>
            (flags & (uint)flag) == (uint)flag;
    }
}