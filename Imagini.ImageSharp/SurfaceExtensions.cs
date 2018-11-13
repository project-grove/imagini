using System;
using System.IO;
using System.Runtime.InteropServices;
using Imagini.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Imagini.ImageSharp
{
    /// <summary>
    /// Contains various Surface-related extensions.
    /// </summary>
    public static class SurfaceExtensions
    {
        /// <summary>
        /// Saves the surface to the specified stream.
        /// </summary>
        public static void SaveAsPng(this Surface surface, Stream stream) =>
            surface.Save(image => image.SaveAsPng(stream));

        /// <summary>
        /// Saves the surface to the specified file, overwriting it if it exists.
        /// </summary>
        public static void SaveAsPng(this Surface surface, string path)
        {
            using(var stream = new FileStream(path, FileMode.Create))
                surface.SaveAsPng(stream);
        }

        /// <summary>
        /// Saves the surface using the specified save action.
        /// </summary>
        /// <example>
        /// surface.Save(image => image.SaveAsJpeg("file.jpg"))
        /// </example>
        public static void Save(this Surface surface,
            Action<Image<Rgba32>> onSave)
        {
            var mustDispose = false;
            var source = surface;
            if (surface.PixelInfo.Format != PixelFormat.Format_RGBA8888)
            {
                source = surface.ConvertTo(PixelFormat.Format_RGBA8888);
                mustDispose = true;
            }

            var image = new Image<Rgba32>(surface.Width, surface.Height);
            var data = new byte[surface.SizeInBytes];
            surface.GetPixelData(ref data);
            var srcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            unsafe {
                fixed(void* target = &MemoryMarshal.GetReference(image.GetPixelSpan()))
                {
                    Buffer.MemoryCopy(
                        (void*)srcHandle.AddrOfPinnedObject(),
                        target, 
                        surface.SizeInBytes, surface.SizeInBytes);
                }
            }
            srcHandle.Free();
            onSave(image);

            image.Dispose();
            if (mustDispose)
                source.Dispose();
        }
    }
}