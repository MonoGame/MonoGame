// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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
        /// The user touched a point and then performed a free-form drag.
        /// </summary>
        FreeDrag = 8,
        /// <summary>        
        /// The user touched a single point for approximately one second.
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

