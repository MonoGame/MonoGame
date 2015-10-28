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

        /// <summary>
        /// Returns the current timer resolution in 100ns units
        /// </summary>
        public static int GetCurrentResolution()
        {
            int min, max, current;
            NtQueryTimerResolution(out min, out max, out current);
            return current;
        }

        /// <summary>
        /// Sleeps as long as possible without exceeding the specified period
        /// </summary>
        public static void SleepForNoMoreThan(double milliseconds)
        {
            if (milliseconds < 1.0)
                return;
            var timerResolution = GetCurrentResolution();
            int adjustedTime = (int)(milliseconds * 10000);
            if (timerResolution > adjustedTime)
                return;
            var tickCount = adjustedTime / timerResolution;
            int adjustedSleepTime = (((tickCount - 1) * timerResolution) / 10000) + 1;
            Thread.Sleep(adjustedSleepTime);
        }
    }
}
