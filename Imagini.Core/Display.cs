using System;
using System.Collections.Generic;

using static Imagini.ErrorHandler;
using static SDL2.SDL_rect;
using static SDL2.SDL_video;

namespace Imagini
{
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

        public override string ToString() => $"{Width}x{Height} {RefreshRate}Hz (display {displayIndex}, mode {modeIndex})";

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
    /// Represents a display device.
    /// </summary>
    public class Display
    {
        private static List<Display> s_displays = new List<Display>();
        /// <summary>
        /// Returns all available display devices.
        /// </summary>
        public static IReadOnlyList<Display> All => s_displays;

        private int _index;
        /// <summary>
        /// Returns the available display modes for this display.
        /// </summary>
        public IEnumerable<DisplayMode> Modes { get; private set; }
        /// <summary>
        /// Returns the current display mode for this display.
        /// </summary>
        public DisplayMode CurrentMode { get; set; }
        /// <summary>
        /// Returns the desktop display mode for this display.
        /// </summary>
        public DisplayMode DesktopMode { get; set; }
        /// <summary>
        /// Returns the name of this display device.
        /// </summary>
        public string Name { get; private set; }
        internal SDL_Rect Bounds { get; private set; }

        internal Display(int index)
        {
            _index = index;
            Modes = DisplayMode.GetAvailable(index);
            Name = SDL_GetDisplayName(_index);
            if (Name == null || Name == "") Name = _index.ToString();
            var bounds = new SDL_Rect();
            Try(() => SDL_GetDisplayBounds(_index, ref bounds), "SDL_GetDisplayBounds");
            Bounds = bounds;
            CurrentMode = DisplayMode.GetCurrent(_index);
            DesktopMode = DisplayMode.GetDesktop(_index);
        }

        internal static void UpdateDisplayInfo()
        {
            s_displays.Clear();
            var numDisplays = TryGet(SDL_GetNumVideoDisplays, "SDL_GetNumVideoDisplays");
            for (var displayIndex = 0; displayIndex < numDisplays; displayIndex++)
                s_displays.Add(new Display(displayIndex));
        }

        internal static int GetCurrentDisplayIndexForWindow(IntPtr window) =>
            TryGet(() => SDL_GetWindowDisplayIndex(window), "SDL_GetWindowDisplayIndex");

        internal static Display GetCurrentDisplayForWindow(IntPtr window) =>
            s_displays[GetCurrentDisplayIndexForWindow(window)];

        static Display()
        {
            Lifecycle.TryInitialize();
            UpdateDisplayInfo();
        }
    }
}