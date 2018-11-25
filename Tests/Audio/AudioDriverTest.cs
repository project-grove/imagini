using Imagini.Audio;
using Xunit;

namespace Tests.Audio
{
    public class AudioDriverTest
    {
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
    }
}