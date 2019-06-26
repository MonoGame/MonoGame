#region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009-2010 The MonoGame Team
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
    /// Enumuration of values that represent different gestures that can be processed by <see cref="TouchPanel.ReadGesture"/>.
    /// </summary>
	[Flags]
    public enum GestureType
    {
        /// <summary>
        /// No gestures.
        /// </summary>
        None = 0,
        /// <summary>
        /// The user touched a single point.
        /// </summary>
        Tap = 1,
        /// <summary>
        /// States completion of a drag gesture(VerticalDrag, HorizontalDrag, or FreeDrag).
        /// </summary>
        /// <remarks>No position or delta information is available for this sample.</remarks>
		DragComplete = 2,	
	    /// <summary>
        /// States that a touch was combined with a quick swipe.
	    /// </summary>    
	    /// <remarks>Flicks does not contain position information. The velocity of it can be read from <see cref="GestureSample.Delta"/></remarks>
        Flick = 4,
        /// <summary>
        /// The use touched a point and then performed a free-form drag.
        /// </summary>
        FreeDrag = 8,
        /// <summary>        
        /// The use touched a single point for approximately one second.
        /// </summary>
        /// <remarks>As this is a single event, it will not be contionusly fired while the user is holding the touch-point.</remarks>
        Hold = 16,
        /// <summary>
        /// The user touched the screen and performed either left to right or right to left drag gesture.
        /// </summary>
        HorizontalDrag = 32,
        /// <summary>
        /// The user either converged or diverged two touch-points on the screen which is like a two-finger drag.
        /// </summary>
        /// <remarks>When this gesture-type is enabled and two fingers are down, it takes precedence over drag gestures.</remarks>
        Pinch = 64,
        /// <summary>
        /// An in-progress pinch operation was completed.
        /// </summary>
        /// <remarks>No position or delta information is available for this sample.</remarks>
        PinchComplete = 128,
        /// <summary>
        /// The user tapped the device twice which is always preceded by a Tap gesture.
        /// </summary>
        /// <remarks>If the time between two touchs are long enough, insted two seperate single Tap gestures will be generated.</remarks>
        DoubleTap = 256,
        /// <summary>
        /// The user touched the screen and performed either top to bottom or bottom to top drag gesture.
        /// </summary>
        VerticalDrag = 512,
    }
}

