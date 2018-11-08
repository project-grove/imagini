using System;
using Imagini.Drawing;
using Xunit;


namespace Tests.Drawing
{
    public class RendererInfoTest : TestBase
    {
        [Fact]
        public void ShouldGetAvailableRenderers()
        {
            PrintTestName();
            var renderers = RendererInfo.All;
#if !HEADLESS
            Assert.NotEmpty(renderers);
#else
            Assert.Empty(renderers);
#endif
        }
    }
}