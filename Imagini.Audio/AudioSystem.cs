using System;
using System.Runtime.InteropServices;
using static SDL2.SDL;
using static SDL2.SDL_audio;
using static SDL2.SDL_mixer;
using static SDL2.SDL_version;
using static Imagini.ErrorHandler;
using static Imagini.Logger;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using Imagini.Audio;

namespace Imagini
{
    [Flags]
    public enum AudioInitFlags
    {
        FLAC = MIX_InitFlags.MIX_INIT_FLAC,
        MOD = MIX_InitFlags.MIX_INIT_MOD,
        MODPlug = MIX_InitFlags.MIX_INIT_MODPLUG,
        MP3 = MIX_InitFlags.MIX_INIT_MP3,
        OGG = MIX_InitFlags.MIX_INIT_OGG,
        FluidSynth = MIX_InitFlags.MIX_INIT_FLUIDSYNTH
    }

    /// <summary>
    /// Contains audio subsystem initialization and shutdown methods.
    /// </summary>
    public static class AudioSystem
    {
        private static string s_driver;
        /// <summary>
        /// Indicates if the audio system was initialized.
        /// </summary>
        public static bool IsInitialized { get; private set; }

        public static AudioInitFlags? InitFlags { get; private set; }

        /// <summary>
        /// Inits sound subsystem with the specified parameters. Returns enum
        /// containing initialized sound format loaders.
        /// </summary>
        public static AudioInitFlags TryInit(string driver = null,
            AudioInitFlags formats = AudioInitFlags.OGG)
        {
            if (IsInitialized) return InitFlags.Value;
            s_driver = driver;
            if (!IsSDLMixerLoaded)
                throw new ImaginiException("SDL_mixer is not present");
            if (driver == null)
                SDL_Init(SDL_INIT_AUDIO);
            else
            {
                var handle = GCHandle.Alloc(driver, GCHandleType.Pinned);
                try
                {
                    Try(() => SDL_AudioInit(handle.AddrOfPinnedObject()),
                        "SDL_AudioInit");
                }
                finally
                {
                    handle.Free();
                }
            }
            IsInitialized = true;
            InitFlags = (AudioInitFlags)Mix_Init((int)formats);
            
            var expected = JoinFlagNames<AudioInitFlags>(formats);
            var actual = JoinFlagNames<AudioInitFlags>(InitFlags);
            Log.Information("Audio subsystem initialized, " +
                $"driver: {AudioDriver.Current}, " +
                "requested formats: " +
                $"{expected}; found loaders for: {actual}");
            return InitFlags.Value;
        }

        /// <summary>
        /// Shutdowns the audio subsystem.
        /// </summary>
        public static void Shutdown()
        {
            if (s_driver != null)
                SDL_AudioQuit();
            else
                SDL_QuitSubSystem(SDL_INIT_AUDIO);
            Mix_Quit();
            IsInitialized = false;
            Log.Information("Audio subsystem shutted down");
        }

        static AudioSystem()
        {
            var version = Marshal.PtrToStructure<SDL_Version>(Mix_Linked_Version());
            Log.Information("Using SDL_mixer version {major}.{minor}.{patch}",
                version.major, version.minor, version.patch);
        }

        [ExcludeFromCodeCoverage]
        /// <summary>
        /// Inits sound system using specified parameters. If any of the specified
        /// formats is unsupported, throws an exception.
        /// </summary>
        public static void Init(string driver = null,
            AudioInitFlags formats = AudioInitFlags.OGG)
        {
            var actualFormats = TryInit(driver, formats);
            if (actualFormats != formats)
            {
                var expected = JoinFlagNames<AudioInitFlags>(formats);
                var actual = JoinFlagNames<AudioInitFlags>(actualFormats);
                IsInitialized = false;
                InitFlags = null;
                throw new ImaginiException(
                    "Could not initialize specified audio format support: " +
                    $"requested {expected}, but found only {actual}"
                    );
            }
        }

        private static IEnumerable<T> GetFlags<T>(Enum mask) =>
            Enum.GetValues(typeof(T))
                         .Cast<Enum>()
                         .Where(m => mask.HasFlag(m))
                         .Cast<T>();

        private static string JoinFlagNames<T>(Enum mask)
        {
            var flags = GetFlags<T>(mask).Select(m => m.ToString());
            if (!flags.Any()) return "(none)";
            return string.Join(", ", flags);
        }

        [ExcludeFromCodeCoverage]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void EnsureInitialization()
        {
            if (!IsInitialized) Init();
        }

        [ExcludeFromCodeCoverage]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RequireOpened()
        {
            if (!AudioDevice.IsOpened)
                throw new ImaginiException("Audio device must be opened before query");
        }
    }
}