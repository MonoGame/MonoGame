/* Copyright (c) 2010 Michael Lidgren

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
USE OR OTHER DEALINGS IN THE SOFTWARE.

*/
#define IS_STOPWATCH_AVAILABLE

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Lidgren.Network
{
	/// <summary>
	/// Time service
	/// </summary>
	public static class NetTime
	{
#if IS_STOPWATCH_AVAILABLE
		private static readonly long s_timeInitialized = Stopwatch.GetTimestamp();
		private static readonly double s_dInvFreq = 1.0 / (double)Stopwatch.Frequency;

		/// <summary>
		/// Get number of seconds since the application started
		/// </summary>
		public static double Now { get { return (double)(Stopwatch.GetTimestamp() - s_timeInitialized) * s_dInvFreq; } }
#else
		private static readonly uint s_timeInitialized = (uint)Environment.TickCount;

		/// <summary>
		/// Get number of seconds since the application started
		/// </summary>
		public static double Now { get { return (double)((uint)Environment.TickCount - s_timeInitialized) / 1000.0; } }
#endif

		/// <summary>
		/// Given seconds it will output a human friendly readable string (milliseconds if less than 60 seconds)
		/// </summary>
		public static string ToReadable(double seconds)
		{
			if (seconds > 60)
				return TimeSpan.FromSeconds(seconds).ToString();
			return (seconds * 1000.0).ToString("N2") + " ms";
		}
	}
}