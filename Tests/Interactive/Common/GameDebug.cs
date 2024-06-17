// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

namespace MonoGame.InteractiveTests
{
    /// <summary>
    /// Allows tests to output console messages including spammy messages that
    /// may be throttled.
    ///
    /// On various platforms, this may be available via console output or via
    /// a special console viewer (Console app on Mac; `adb logcat` on Android etc).
    /// </summary>
    public partial class GameDebug
    {
        /// <summary>Output a single console message.</summary>
        public static void C(string message)
        {
            System.Console.WriteLine($"MGDBG: {message}");
        }

        /// <summary>Output an error message to the console.</summary>
        public static void E(string message)
        {
            System.Console.WriteLine($"****ERROR*****:MGDBG: {message}");
        }

        /// <summary>Maintains the spam message counts to prevent spamming the console.</summary>
        private record MessageCount(int Count)
        {
            public int Count { get; set; } = Count;
        }
        private static readonly Dictionary<string, MessageCount> MESSAGES_COUNTS_ = new();

        /// <summary>Use this to output spammy messages.</summary>
        public static void Spam(string message, int maxNumTimes = 10)
        {
            if (!MESSAGES_COUNTS_.TryGetValue(message, out var numTimes))
            {
                numTimes = new(0);
                MESSAGES_COUNTS_.Add(message, numTimes);
            }

            if (numTimes.Count >= maxNumTimes) { return; }

            System.Console.WriteLine($"MGDBG: {message} ...#{numTimes.Count}");
            numTimes.Count++;
        }
    }
}
