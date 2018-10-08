using System;
using System.Collections.Generic;
using static Imagini.Internal.ErrorHandler;
using static SDL2.SDL_rect;
using static SDL2.SDL_video;

namespace Imagini
{
    /// <summary>
    /// Represents a display device.
    /// </summary>
    public class Display
    {
        private static List<Display> s_displays = new List<Display>();
        /// <summary>
        /// Returns all available display devices.
        /// </summary>
        public static List<Display> All => s_displays;

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
    }
}