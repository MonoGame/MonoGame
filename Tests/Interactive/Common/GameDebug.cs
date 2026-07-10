// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

namespace MonoGame.InteractiveTests
{
    /// <summary>
    /// Allows tests to output console messages. This is separate from
    /// GraphicsDebug which is an internal class to MonoGame platform code.
    ///
    /// On various platforms, this may be available via console output or via
    /// a special console viewer: Console app terminal window output on Mac;
    /// <code>adb logcat</code> on Android and so on.
    /// </summary>
    public partial class GameDebug
    {
        /// <summary>Output a single console message.</summary>
        public static void LogInfo(string message)
        {
            System.Console.WriteLine($"MGDBG: {message}");
        }

        /// <summary>Output an error message to the console.</summary>
        public static void LogError(string message)
        {
            System.Console.WriteLine($"****ERROR*****:MGDBG: {message}");
        }
    }
}
