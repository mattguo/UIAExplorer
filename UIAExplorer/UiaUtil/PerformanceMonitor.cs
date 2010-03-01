using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mono.Accessibility.UIAExplorer.UiaUtil
{
	public class PerformanceMonitor
	{
		// TODO Re-implement this class with more precise timer

#if WIN32
		[DllImport ("KERNEL32.dll")]
		private static extern bool QueryPerformanceCounter (out long count);

		[DllImport ("Kernel32.dll")]
		private static extern bool QueryPerformanceFrequency (out long frequency);
#else
		[DllImport ("libc.6.so")]
		static extern int gettimeofday (out timeval tv, out timezone tz);

		//timeval t = null;
		//timezone z = null;
		//gettimeofday (out t, out z);
		//Console.WriteLine (t.tv_sec);

		struct timeval
		{
			public int tv_sec;
			public int tv_usec;
		}

		struct timezone
		{
			int tz_minuteswest;
			int tz_dsttime;
		}
#endif
		private DateTime timer;
		private string message;

		public void TimerStart (string message)
		{
			this.message = message;
			timer = DateTime.Now;
		}

		public void TimerEnd ()
		{
			double sec = (DateTime.Now - timer).TotalSeconds;
			Log.Debug ("{0}: {1} seconds", message, sec);
		}
	}
}
