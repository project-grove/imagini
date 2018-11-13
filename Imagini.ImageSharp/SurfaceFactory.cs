using System.Buffers;
using System.IO;
using System.Linq;
using Imagini.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

/// <summary>
/// ImageSharp integration module.
/// </summary>
namespace Imagini.ImageSharp
{
    /// <summary>
    /// Contains various functions to aid in surface creation.
    /// </summary>
    public static class SurfaceFactory
    {
        /// <summary>
        /// Gets or sets the ArrayPool used by image loader.
        /// </summary>
        public static ArrayPool<byte> ArrayPool { get; set; } = ArrayPool<byte>.Shared;

        /// <summary>
        /// Creates a RGBA8888 surface by loading a file from the specified path.
        /// </summary>
        public static Surface FromFile(string path) => FromImage(Image.Load(path));

        /// <summary>
        /// Creates a RGBA8888 surface by loading a file from the specified stream.
        /// </summary>
        public static Surface FromStream(Stream stream, IImageDecoder decoder) => 
            FromImage(Image.Load(stream, decoder));
        
        /// <summary>
        /// Creates a RGBA8888 surface from the specified Image object.
        /// </summary>
        public static Surface FromImage<TPixel>(Image<TPixel> image)
            where TPixel : struct, IPixel<TPixel>
        {
            var cloned = image.CloneAs<Rgba32>();
            var pixels = cloned.GetPixelSpan();
            var bytes = ArrayPool.Rent(pixels.Length * 4);
            for (int i = 0; i < pixels.Length; i++)
            {
                var pixel = pixels[i];
                bytes[i * 4] = pixel.A;
                bytes[i * 4 + 1] = pixel.B;
                bytes[i * 4 + 2] = pixel.G;
                bytes[i * 4 + 3] = pixel.R;
            }
            var surface = Surface.CreateFrom(bytes, 
                cloned.Width,
                cloned.Height, 
                PixelFormat.Format_RGBA8888);
            ArrayPool.Return(bytes);
            return surface;
        }
    }
}