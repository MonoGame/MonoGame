#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

#region Using Statements
using System;

using SDL2;
#endregion

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Allows reading position and button click information from mouse.
    /// </summary>
    public static class Mouse
    {
        #region Internal Properties and Variables
        
        internal static GameWindow PrimaryWindow;
        
        internal static IntPtr INTERNAL_sdlWindowHandle;

        public static IntPtr WindowHandle
        {
            get
            {
                return INTERNAL_sdlWindowHandle;
            }
        }
        
        internal static int INTERNAL_MouseWheel
        {
            get;
            set;
        }

        internal static int INTERNAL_BackbufferWidth = 800;
        internal static int INTERNAL_BackbufferHeight = 600;
        internal static int INTERNAL_WindowWidth = 800;
        internal static int INTERNAL_WindowHeight = 600;

        #endregion

        #region Public Interface

        /// <summary>
        /// Gets mouse state information that includes position and button
        /// presses for the provided window
        /// </summary>
        /// <returns>Current state of the mouse.</returns>
        public static MouseState GetState(GameWindow window)
        {
            int x, y;
            uint flags = SDL.SDL_GetMouseState(out x, out y);
  
            x = (int)((double)x * INTERNAL_BackbufferWidth / INTERNAL_WindowWidth);
            y = (int)((double)y * INTERNAL_BackbufferHeight / INTERNAL_WindowHeight);

            window.MouseState.X = x;
            window.MouseState.Y = y;
            
            window.MouseState.LeftButton = (ButtonState) (flags & SDL.SDL_BUTTON_LMASK);
            window.MouseState.RightButton = (ButtonState) ((flags & SDL.SDL_BUTTON_RMASK) >> 2);
            window.MouseState.MiddleButton = (ButtonState) ((flags & SDL.SDL_BUTTON_MMASK) >> 1);
            
            window.MouseState.ScrollWheelValue = INTERNAL_MouseWheel;

            return window.MouseState;
        }
        
        /// <summary>
        /// Gets mouse state information that includes position and button presses
        /// for the primary window
        /// </summary>
        /// <returns>Current state of the mouse.</returns>
        public static MouseState GetState()
        {
            return GetState(PrimaryWindow);
        }

        /// <summary>
        /// Sets mouse cursor's relative position to game-window.
        /// </summary>
        /// <param name="x">Relative horizontal position of the cursor.</param>
        /// <param name="y">Relative vertical position of the cursor.</param>
        public static void SetPosition(int x, int y)
        {
            x = (int)((double)x * INTERNAL_WindowWidth / INTERNAL_BackbufferWidth);
            y = (int)((double)y * INTERNAL_WindowHeight / INTERNAL_BackbufferHeight);
            PrimaryWindow.MouseState.X = x;
            PrimaryWindow.MouseState.Y = y;
            
            SDL.SDL_WarpMouseInWindow(INTERNAL_sdlWindowHandle, x, y);
        }

        #endregion
    }
}

