using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Imagini.Drawing;
using Imagini.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Tests
{
    public static class Util
    {
        public static string NearAssembly(string file) =>
            Path.Join(AssemblyDirectory, file);

        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static void SaveImage(Image<Rgba32> image, string fileName)
        {
            using(var stream = File.OpenWrite(NearAssembly(fileName)))
                image.SaveAsPng(stream);
        }

        public static void OutputIsCloseTo(this Graphics graphics, 
            string pathToReferenceImage, 
            float tolerance = 0.05f)
        {
            if (!File.Exists(pathToReferenceImage))
            {
                graphics.SaveAsPng(pathToReferenceImage);
                throw new FileNotFoundException("No reference present, generated one");
            }
            var reference = SurfaceFactory.FromFile(pathToReferenceImage);
            if (reference.Size != graphics.OutputSize)
                throw new ArgumentException("Wrong reference image size");
            
            var refPixels = new ColorRGB888[reference.PixelCount];
            reference.ReadPixels(ref refPixels);

            var ms = new MemoryStream();
            graphics.SaveAsPng(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var actual = SurfaceFactory.FromStream(ms, new PngDecoder());
            var actualPixels = new ColorRGB888[reference.PixelCount];
            actual.ReadPixels(ref actualPixels);

            reference.Dispose();
            actual.Dispose();

            var difference = 0.0f;
            for (int i = 0; i < refPixels.Length; i++)
            {
                var refColor = refPixels[i];
                var actualColor = actualPixels[i];
                if (refColor.Equals(actualColor)) continue;
                difference += Math.Abs(refColor.R - actualColor.R) / 255.0f;
                difference += Math.Abs(refColor.G - actualColor.G) / 255.0f;
                difference += Math.Abs(refColor.B - actualColor.B) / 255.0f;
            }
            difference /= reference.PixelCount;
            difference.Should().BeLessOrEqualTo(tolerance);
        }
    }
}