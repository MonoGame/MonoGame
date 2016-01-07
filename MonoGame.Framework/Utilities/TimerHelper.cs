// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices;
using System.Threading;

namespace Microsoft.Xna.Framework.Utilities
{
    internal static class TimerHelper
    {
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtQueryTimerResolution(out int MinimumResolution, out int MaximumResolution, out int CurrentResolution);

        private static readonly double LowestSleepThreshold;

        static TimerHelper()
        {
            int min, max, current;
            NtQueryTimerResolution(out min, out max, out current);
            LowestSleepThreshold = 1.0 + (max / 10000.0);
        }

        /// <summary>
        /// Returns the current timer resolution in milliseconds
        /// </summary>
        public static double GetCurrentResolution()
        {
            int min, max, current;
            NtQueryTimerResolution(out min, out max, out current);
            return current / 10000.0;
        }

        /// <summary>
        /// Sleeps as long as possible without exceeding the specified period
        /// </summary>
        public static void SleepForNoMoreThan(double milliseconds)
        {
            // Assumption is that Thread.Sleep(t) will sleep for at least (t), and at most (t + timerResolution)
            if (milliseconds < LowestSleepThreshold)
                return;
            var sleepTime = (int)(milliseconds - GetCurrentResolution());
            if (sleepTime < 1)
                return;
            Thread.Sleep(sleepTime);
        }
    }
}
