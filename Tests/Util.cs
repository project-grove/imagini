using System;
using System.IO;
using System.Reflection;
using SixLabors.ImageSharp;
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
    }
}