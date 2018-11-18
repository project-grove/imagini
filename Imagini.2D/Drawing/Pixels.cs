using System.Runtime.InteropServices;
using static SDL2.SDL_surface;
using static Imagini.ErrorHandler;
using System;

namespace Imagini.Drawing
{
    /// <summary>
    /// Contains pixel-related utility functions.
    /// </summary>
    public static class Pixels
    {
        /// <summary>
        /// Copies a block of pixels of one format to another format.
        /// </summary>
        /// <param name="width">The width of the block to copy, in pixels</param>
        /// <param name="height">The height of the block to copy, in pixels</param>
        /// <param name="srcStride">The stride (pitch) of the block to copy</param>
        /// <param name="dstStride">The stride (pitch) of the destination pixels</param>
        /// <param name="src">Source pixel data</param>
        /// <param name="dst">Destination pixel data</param>
        /// <returns></returns>
        public static void Convert<T1, T2>(int width, int height, int srcStride, int dstStride,
            ref T1[] src, ref T2[] dst)
            where T1 : IColor
            where T2 : IColor
        {
            var srcFormat = src[0].Format;
            var dstFormat = dst[0].Format;
            var srcHandle = GCHandle.Alloc(src, GCHandleType.Pinned);
            var dstHandle = GCHandle.Alloc(dst, GCHandleType.Pinned);
            try
            {
                Convert(width, height, srcStride, dstStride,
                    srcFormat, dstFormat,
                    srcHandle.AddrOfPinnedObject(), dstHandle.AddrOfPinnedObject());
            }
            finally
            {
                srcHandle.Free();
                dstHandle.Free();
            }
        }

        /// <summary>
        /// Copies a block of pixels of one format to another format.
        /// </summary>
        /// <param name="width">The width of the block to copy, in pixels</param>
        /// <param name="height">The height of the block to copy, in pixels</param>
        /// <param name="srcStride">The stride (pitch) of the block to copy</param>
        /// <param name="dstStride">The stride (pitch) of the destination pixels</param>
        /// <param name="srcFormat">Source pixel format</param>
        /// <param name="dstFormat">Destination pixel format</param>
        /// <param name="src">Source pixel data</param>
        /// <param name="dst">Destination pixel data</param>
        public static void Convert(int width, int height, int srcStride, int dstStride,
            PixelFormat srcFormat, PixelFormat dstFormat, IntPtr src, IntPtr dst)
        {
            Try(() => SDL_ConvertPixels(width, height,
                (uint)srcFormat, src, srcStride,
                (uint)dstFormat, dst, dstStride),
                "SDL_ConvertPixels");
        }
    }
}