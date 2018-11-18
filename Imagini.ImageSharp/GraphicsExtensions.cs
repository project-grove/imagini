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
    /// Contains various Graphics-related extensions.
    /// </summary>
    public static class GraphicsExtensions
    {
        /// <summary>
        /// Saves the graphics to the specified stream.
        /// </summary>
        public static void SaveAsPng(this Graphics graphics, Stream stream) =>
            graphics.Save(image => image.SaveAsPng(stream));

        /// <summary>
        /// Saves the graphics to the specified file, overwriting it if it exists.
        /// </summary>
        public static void SaveAsPng(this Graphics graphics, string path)
        {
            using (var stream = new FileStream(path, FileMode.Create))
                graphics.SaveAsPng(stream);
        }

        /// <summary>
        /// Saves the graphics using the specified save action.
        /// </summary>
        /// <example>
        /// graphics.Save(image => image.SaveAsJpeg("file.jpg"))
        /// </example>
        public static void Save(this Graphics graphics,
            Action<Image<Rgba32>> onSave)
        {
            var targetFormat = PixelFormat.Format_ABGR8888;
            var size = graphics.OutputSize;
            var pixelData = new ColorRGB888[graphics.PixelCount];
            graphics.ReadPixels(ref pixelData);
            var pixelHandle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
            try
            {
                var image = new Image<Rgba32>(size.Width, size.Height);
                unsafe
                {
                    fixed (void* target = &MemoryMarshal.GetReference(image.GetPixelSpan()))
                    {
                        Pixels.Convert(size.Width, size.Height,
                            4 * size.Width, 4 * size.Width,
                            PixelFormat.Format_RGB888,
                            targetFormat,
                            pixelHandle.AddrOfPinnedObject(),
                            (IntPtr)target);
                    }
                }
                onSave(image);
                image.Dispose();
            }
            finally
            {
                pixelHandle.Free();
            }
        }
    }
}