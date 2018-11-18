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
    public static class TextureFactory
    {
        /// <summary>
        /// Gets or sets the ArrayPool used by image loader.
        /// </summary>
        public static ArrayPool<byte> ArrayPool { get; set; } = ArrayPool<byte>.Shared;

        /// <summary>
        /// Creates a RGBA8888 surface by loading a file from the specified path.
        /// </summary>
        public static Texture FromFile(Graphics graphics, TextureScalingQuality quality,
            string path) => 
            FromImage(graphics, quality, Image.Load(path));

        /// <summary>
        /// Creates a RGBA8888 surface by loading a file from the specified stream.
        /// </summary>
        public static Texture FromStream(Graphics graphics, Stream stream,
            TextureScalingQuality quality,
            IImageDecoder decoder) => 
            FromImage(graphics, quality, Image.Load(stream, decoder));
        
        /// <summary>
        /// Creates a RGBA8888 surface from the specified Image object.
        /// </summary>
        public static Texture FromImage<TPixel>(Graphics graphics, 
            TextureScalingQuality quality, Image<TPixel> image)
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
            var texture = graphics.CreateTexture(cloned.Width, cloned.Height,
                quality, PixelFormat.Format_RGBA8888);
            texture.SetPixels(ref bytes);
            ArrayPool.Return(bytes);
            return texture;
        }
    }
}