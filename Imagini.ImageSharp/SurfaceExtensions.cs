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
            var targetFormat = PixelFormat.Format_ABGR8888;
            if (surface.PixelInfo.Format != targetFormat)
            {
                source = surface.ConvertTo(targetFormat);
                mustDispose = true;
            }

            var image = new Image<Rgba32>(surface.Width, surface.Height);
            unsafe {
                fixed(void* target = &MemoryMarshal.GetReference(image.GetPixelSpan()))
                {
                    Buffer.MemoryCopy(
                        (void*)source.PixelsHandle,
                        target, 
                        surface.SizeInBytes, surface.SizeInBytes);
                }
            }
            onSave(image);

            image.Dispose();
            if (mustDispose)
                source.Dispose();
        }
    }
}