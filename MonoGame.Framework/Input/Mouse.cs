// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Allows reading position and button click information from mouse.
    /// </summary>
    public static partial class Mouse
    {
        internal static GameWindow PrimaryWindow;

        private static readonly MouseState _defaultState = new MouseState();

        /// <summary>
        /// Gets or sets the window handle for current mouse processing.
        /// </summary> 
        public static IntPtr WindowHandle
        {
            get { return PlatformGetWindowHandle(); }
            set { PlatformSetWindowHandle(value); }
        }

        /// <summary>
        /// This API is an extension to XNA.
        /// Gets mouse state information that includes position and button
        /// presses for the provided window
        /// </summary>
        /// <returns>Current state of the mouse.</returns>
        public static MouseState GetState(GameWindow window)
        {
            return PlatformGetState(window);
        }

        /// <summary>
        /// Gets mouse state information that includes position and button presses
        /// for the primary window
        /// </summary>
        /// <returns>Current state of the mouse.</returns>
        public static MouseState GetState()
        {
            if (PrimaryWindow != null)
                return GetState(PrimaryWindow);

            return _defaultState;
        }

        /// <summary>
        /// Sets mouse cursor's relative position to game-window.
        /// </summary>
        /// <param name="x">Relative horizontal position of the cursor.</param>
        /// <param name="y">Relative vertical position of the cursor.</param>
        public static void SetPosition(int x, int y)
        {
            PlatformSetPosition(x, y);
        }

        /// <summary>
        /// Sets the cursor image to the specified MouseCursor.
        /// </summary>
        /// <param name="cursor">Mouse cursor to use for the cursor image.</param>
        public static void SetCursor(MouseCursor cursor)
        {
            PlatformSetCursor(cursor);
        }
    }
}
