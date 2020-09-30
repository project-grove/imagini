using System.Runtime.InteropServices;
using System.Threading;

namespace Imagini.Core.Internal
{
	public static class Native
	{
		public static class Windows
		{
			[DllImport("ntdll.dll", SetLastError = true)]
			private static extern int NtQueryTimerResolution(out uint MinimumResolution, out uint MaximumResolution, out uint CurrentResolution);

			private static readonly double threshold;

			static Windows()
			{
				NtQueryTimerResolution(out uint min, out uint max, out uint current);
				threshold = max * 0.0001 + 1;
			}

			private static double QueryTimerResolution()
			{
				NtQueryTimerResolution(out uint min, out uint max, out uint current);
				return current * 0.0001;
			}

			public static void SleepAtMost(double ms)
			{
				if (ms < threshold)
					return;
				var sleepTime = (int)(ms - QueryTimerResolution());
				if (sleepTime > 0)
					Thread.Sleep(sleepTime);
			}
		}
	}
}