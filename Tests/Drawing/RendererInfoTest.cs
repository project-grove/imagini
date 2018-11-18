using System;
using Imagini.Drawing;
using Xunit;


namespace Tests.Drawing
{
    [DisplayTestMethodName]
    public class RendererInfoTest 
    {
        [Fact]
        public void ShouldGetAvailableRenderers()
        {
            
            var renderers = RendererInfo.All;
#if !HEADLESS
            Assert.NotEmpty(renderers);
#else
            Assert.Empty(renderers);
#endif
        }
    }
}