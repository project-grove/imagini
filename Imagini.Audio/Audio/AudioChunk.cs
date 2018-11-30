using System;
using System.Runtime.InteropServices;
using static Imagini.AudioSystem;
using static SDL2.SDL_mixer;
using static SDL2.SDL_error;
using static SDL2.SDL_rwops;


namespace Imagini.Audio
{
    public class AudioChunk
    {
        internal IntPtr Handle { get; private set; }
        internal static IntPtr s_fileMode = Marshal.StringToHGlobalAnsi("rb");

        internal AudioChunk(string path)
        {
            var pathHandle = GCHandle.Alloc(path, GCHandleType.Pinned);
            try
            {
                var rwops = SDL_RWFromFile(pathHandle.AddrOfPinnedObject(),
                    s_fileMode);
                if (rwops == IntPtr.Zero)
                    throw new ImaginiException(
                        $"Could not load file: {SDL_GetError()}");
                Handle = Mix_LoadWAV_RW(rwops, 1);
                if (Handle == IntPtr.Zero)
                    throw new ImaginiException(
                        $"Could not load audio data: {SDL_GetError()}");
            }
            finally
            {
                pathHandle.Free();
            }
        }
    }
}