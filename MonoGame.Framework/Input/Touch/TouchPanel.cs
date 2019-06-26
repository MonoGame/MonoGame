#region License
// /*
// Microsoft Public License (Ms-PL)
// XnaTouch - Copyright © 2009-2010 The XnaTouch Team
//
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
#endregion License

using System;

namespace Microsoft.Xna.Framework.Input.Touch
{
    /// <summary>
    /// Allows retrieval of information from Touch Panel device.
    /// </summary>
    public static class TouchPanel
    {
        internal static GameWindow PrimaryWindow;

        /// <summary>
        /// Gets the current state of the touch panel.
        /// </summary>
        /// <returns><see cref="TouchCollection"/></returns>
        public static TouchCollection GetState()
        {
            return PrimaryWindow.TouchPanelState.GetState();
        }

        public static TouchPanelState GetState(GameWindow window)
        {
            return window.TouchPanelState;
        }

        public static TouchPanelCapabilities GetCapabilities()
        {
            return PrimaryWindow.TouchPanelState.GetCapabilities();
        }

        internal static void AddEvent(int id, TouchLocationState state, Vector2 position)
        {
            AddEvent(id, state, position, false);
        }

        internal static void AddEvent(int id, TouchLocationState state, Vector2 position, bool isMouse)
        {
            PrimaryWindow.TouchPanelState.AddEvent(id, state, position, isMouse);
        }

        /// <summary>
        /// Returns the next available gesture on touch panel device.
        /// </summary>
        /// <returns><see cref="GestureSample"/></returns>
		public static GestureSample ReadGesture()
        {
            // Return the next gesture.
            return PrimaryWindow.TouchPanelState.GestureList.Dequeue();			
        }

        /// <summary>
        /// The window handle of the touch panel. Purely for Xna compatibility.
        /// </summary>
        public static IntPtr WindowHandle
        {
            get { return PrimaryWindow.TouchPanelState.WindowHandle; }
            set { PrimaryWindow.TouchPanelState.WindowHandle = value; }
        }

        /// <summary>
        /// Gets or sets the display height of the touch panel.
        /// </summary>
        public static int DisplayHeight
        {
            get { return PrimaryWindow.TouchPanelState.DisplayHeight; }
            set { PrimaryWindow.TouchPanelState.DisplayHeight = value; }
        }

        /// <summary>
        /// Gets or sets the display orientation of the touch panel.
        /// </summary>
        public static DisplayOrientation DisplayOrientation
        {
            get { return PrimaryWindow.TouchPanelState.DisplayOrientation; }
            set { PrimaryWindow.TouchPanelState.DisplayOrientation = value; }
        }

        /// <summary>
        /// Gets or sets the display width of the touch panel.
        /// </summary>
        public static int DisplayWidth
        {
            get { return PrimaryWindow.TouchPanelState.DisplayWidth; }
            set { PrimaryWindow.TouchPanelState.DisplayWidth = value; }
        }
		
        /// <summary>
        /// Gets or sets enabled gestures.
        /// </summary>
        public static GestureType EnabledGestures
        {
            get { return PrimaryWindow.TouchPanelState.EnabledGestures; }
            set { PrimaryWindow.TouchPanelState.EnabledGestures = value; }
        }

        public static bool EnableMouseTouchPoint
        {
            get { return PrimaryWindow.TouchPanelState.EnableMouseTouchPoint; }
            set { PrimaryWindow.TouchPanelState.EnableMouseTouchPoint = value; }
        }

        public static bool EnableMouseGestures
        {
            get { return PrimaryWindow.TouchPanelState.EnableMouseGestures; }
            set { PrimaryWindow.TouchPanelState.EnableMouseGestures = value; }
        }

        /// <summary>
        /// Returns true if a touch gesture is available.
        /// </summary>
        public static bool IsGestureAvailable
        {
            get { return PrimaryWindow.TouchPanelState.IsGestureAvailable; }
        }
    }
}