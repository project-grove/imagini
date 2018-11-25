using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static SDL2.SDL_audio;

namespace Imagini.Audio
{
    public static class AudioDriver 
    {
        private static List<string> _drivers = new List<string>();
        public static IReadOnlyList<string> All => _drivers;

        internal static void UpdateDriverInfo()
        {
            _drivers.Clear();
            var count = SDL_GetNumAudioDrivers();
            for (int i = 0; i < count; i++)
                _drivers.Add(SDL_GetAudioDriver(i));
        }

        static AudioDriver()
        {
            Lifecycle.TryInitialize();
            UpdateDriverInfo();
        }
    }
}
