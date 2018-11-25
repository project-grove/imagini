using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static SDL2.SDL_audio;

/// <summary>
/// Namespace for SDL2-based audio subsystem for Imagini.
/// </summary>
namespace Imagini.Audio
{
    /// <summary>
    /// Gives access to system audio driver info.
    /// </summary>
    public static class AudioDriver 
    {
        private static List<string> _drivers = new List<string>();
        /// <summary>
        /// Returns a list of all available sound drivers.
        /// </summary>
        public static IReadOnlyList<string> All => _drivers;

        /// <summary>
        /// Returns the currently active sound driver.
        /// </summary>
        public static string Current => SDL_GetCurrentAudioDriver();

        internal static void UpdateDriverInfo()
        {
            _drivers.Clear();
            var count = SDL_GetNumAudioDrivers();
            for (int i = 0; i < count; i++)
                _drivers.Add(SDL_GetAudioDriver(i));
        }

        static AudioDriver() => UpdateDriverInfo();
    }
}
