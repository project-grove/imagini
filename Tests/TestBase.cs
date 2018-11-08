using System.Diagnostics;
using System.Reflection;
using static Imagini.Logger;

namespace Tests
{
    public abstract class TestBase
    {
        public TestBase() =>
            Log.Debug("--- Test suite: {name} ---", this.GetType().FullName);

        public void PrintTestName()
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1);
            var currentMethodName = sf.GetMethod().Name;
            Log.Debug("--- Test: {name} ---", currentMethodName);
        }
    }
}