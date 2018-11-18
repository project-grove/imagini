using System.Diagnostics;
using System.Reflection;
using Xunit.Sdk;
using static Imagini.Logger;

namespace Tests
{
    public class DisplayTestMethodNameAttribute : BeforeAfterTestAttribute
    {
        public override void Before(MethodInfo methodUnderTest)
        {
            var nameOfRunningTest = methodUnderTest.Name;
            Log.Debug("@@@ SETUP: '{0}.'", methodUnderTest.Name);
        }

        public override void After(MethodInfo methodUnderTest)
        {
            var nameOfRunningTest = methodUnderTest.Name;
            Log.Debug("### TEARDOWN: '{0}.'", methodUnderTest.Name);
        }
    }
}