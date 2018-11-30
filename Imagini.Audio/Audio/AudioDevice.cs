using static SDL2.SDL_mixer;
using static Imagini.ErrorHandler;
using static Imagini.AudioSystem;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Imagini.Audio
{
    /// <summary>
    /// Represents a sound output device.
    /// </summary>
    public static class AudioDevice
    {
        /// <summary>
        /// Indicates if a sound device was opened.
        /// </summary>
        public static bool IsOpened { get; private set; }

        /// <summary>
        /// Opens the audio output device with the specified parameters.
        /// </summary>
        public static void Open(
            int frequency = MIX_DEFAULT_FREQUENCY,
            AudioSampleFormat sampleFormat = (AudioSampleFormat)MIX_DEFAULT_FORMAT,
            int channels = MIX_DEFAULT_CHANNELS,
            int chunkSize = 4096)
        {
            EnsureInitialization();
            if (IsOpened) return;
            Try(() =>
                Mix_OpenAudio(frequency, (ushort)sampleFormat, channels, chunkSize),
                "Mix_OpenAudio");
            IsOpened = true;
        }

        /// <summary>
        /// Closes the audio output device.
        /// </summary>
        public static void Close()
        {
            EnsureInitialization();
            if (!IsOpened) return;
            Mix_CloseAudio();
            IsOpened = false;
        }

        /// <summary>
        /// Queries the opened audio device about it's parameters. Returns true
        /// and sets the output parameters if the query was successful.
        /// </summary>
        public static bool Query(out int frequency, out AudioSampleFormat sampleFormat,
            out int channels)
        {
            EnsureInitialization();
            RequireOpened();
            var freq = 0; ushort fmt = 0; var ch = 0;
            var result = Mix_QuerySpec(ref freq, ref fmt, ref ch);
            if (result == 0)
            {
                frequency = 0;
                sampleFormat = AudioSampleFormat.U8;
                channels = 0;
                return false;
            }
            frequency = freq;
            sampleFormat = (AudioSampleFormat)fmt;
            channels = ch;
            return true;
        }

        /// <summary>
        /// Loads a sound file.
        /// </summary>
        public static AudioChunk Load(string path)
        {
            EnsureInitialization();
            RequireOpened();
            return new AudioChunk(path);
        }

        /// <summary>
        /// Returns available chunk decoders.
        /// </summary>
        public static IReadOnlyList<string> GetChunkDecoders()
        {
            EnsureInitialization();
            RequireOpened();

            var count = Mix_GetNumChunkDecoders();
            var result = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                var value = Marshal.PtrToStringAnsi(
                    Mix_GetChunkDecoder(i)
                );
                result.Add(value);
            }
            return result;
        }
    }
}