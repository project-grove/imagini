using System.Collections.Generic;
using FluentAssertions;
using Imagini;
using Imagini.Audio;
using Xunit;

namespace Tests.Audio
{
    [DisplayTestMethodName]
    public class AudioSystemTest
    {

        [Fact]
        public void ShouldGetCurrentAudioDriver()
        {
#if !HEADLESS
            Assert.Null(AudioDriver.Current);
            AudioSystem.Init();
            Assert.NotNull(AudioDriver.Current);
#else
            Assert.Null(AudioDriver.Current);
#endif
            AudioSystem.Shutdown();
        }

        [Fact]
        public void ShouldGetAudioDriverList()
        {
            var drivers = AudioDriver.All;
#if !HEADLESS
            Assert.NotEmpty(drivers);
#else
            Assert.Empty(drivers);
#endif
        }

#if !HEADLESS
        [Fact]
        public void ShouldInitSpecifiedDriver()
        {
            var drivers = AudioDriver.All;
            var initialized = new List<string>();
            foreach (var driver in drivers)
            {
                try
                {
                    AudioSystem.Shutdown();
                    AudioSystem.IsInitialized.Should().BeFalse();
                    AudioDriver.Current.Should().BeNull();
                    AudioSystem.Init(driver);
                    AudioSystem.IsInitialized.Should().BeTrue();
                    AudioDriver.Current.Should().NotBeNull();
                    initialized.Add(driver);
                }
#pragma warning disable
                catch (ImaginiException ex) {}
#pragma warning enable
            }
            AudioSystem.Shutdown();
            initialized.Should().NotBeEmpty();
        }
#endif
    }
}