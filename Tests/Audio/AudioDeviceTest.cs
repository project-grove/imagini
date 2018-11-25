using FluentAssertions;
using Imagini.Audio.Audio;
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
    }
#endif
}