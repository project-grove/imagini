using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Imagini;
using Imagini.Audio;
using Xunit;

namespace Tests.Audio
{
#if !HEADLESS
    public class AudioDeviceTest
    {
        [Fact]
        public void ShouldOpenDeviceWithSpecifiedParameters()
        {
            AudioDevice.IsOpened.Should().BeFalse();
            AudioDevice.Open();
            AudioDevice.IsOpened.Should().BeTrue();
            var successful = AudioDevice.Query(
                out int frequency,
                out AudioSampleFormat format,
                out int channels);
            successful.Should().BeTrue();
            frequency.Should().BeGreaterThan(0);
            channels.Should().BeGreaterThan(0);
            AudioDevice.Close();
            AudioDevice.IsOpened.Should().BeFalse();
        }

        [Fact]
        public void ShouldFailToQueryNotOpenedDevice()
        {
            AudioDevice.Query(out _, out _, out _).Should().BeFalse();
        }

        [Fact]
        public void ShouldEnumerateChunkDecoders()
        {
            Assert.ThrowsAny<ImaginiException>(() => AudioDevice.GetChunkDecoders());
            AudioDevice.Open();
            var decoders = AudioDevice.GetChunkDecoders();
            AudioDevice.Close();
            decoders.Should().NotBeEmpty();
        }

        [Fact]
        public void ShouldLoadSpecifiedFile()
        {
            AudioDevice.Open();
            var chunk = AudioDevice.Load(NearAssembly("2test.ogg"));
            AudioDevice.Close();
        }

        private static string NearAssembly(string file) =>
            Path.Join(AssemblyDirectory, file);

        private static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
#endif
}