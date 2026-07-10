// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Defines the buttons on gamepad.
    /// </summary>
    [Flags]
    public enum Buttons
    {
        /// <summary>
        /// No button set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Directional pad up.
        /// </summary>
        DPadUp = 1,

        /// <summary>
        /// Directional pad down.
        /// </summary>
        DPadDown = 2,

        /// <summary>
        /// Directional pad left.
        /// </summary>
        DPadLeft = 4,

        /// <summary>
        /// Directional pad right.
        /// </summary>
        DPadRight = 8,

        /// <summary>
        /// START button.
        /// </summary>
        Start = 16,
      
        /// <summary>
        /// BACK button.
        /// </summary>
        Back = 32,

        /// <summary>
        /// Left stick button (pressing the left stick).
        /// </summary>
        LeftStick = 64,

        /// <summary>
        /// Right stick button (pressing the right stick).
        /// </summary>
        RightStick = 128,

        /// <summary>
        /// Left bumper (shoulder) button.
        /// </summary>
        LeftShoulder = 256,

        /// <summary>
        /// Right bumper (shoulder) button.
        /// </summary>
        RightShoulder = 512,

        /// <summary>
        /// Big button.
        /// </summary>    
        BigButton = 2048,
       
        /// <summary>
        /// A button.
        /// </summary>
        A = 4096,

        /// <summary>
        /// B button.
        /// </summary>
        B = 8192,

        /// <summary>
        /// X button.
        /// </summary>
        X = 16384,

        /// <summary>
        /// Y button.
        /// </summary>
        Y = 32768,    

        /// <summary>
        /// Left stick is towards the left.
        /// </summary>
        LeftThumbstickLeft = 2097152,

        /// <summary>
        /// Right trigger.
        /// </summary>
        RightTrigger = 4194304,

        /// <summary>
        /// Left trigger.
        /// </summary>
        LeftTrigger = 8388608,

        /// <summary>
        /// Right stick is towards up.
        /// </summary>   
        RightThumbstickUp = 16777216,

        /// <summary>
        /// Right stick is towards down.
        /// </summary>   
        RightThumbstickDown = 33554432,

        /// <summary>
        /// Right stick is towards the right.
        /// </summary>
        RightThumbstickRight = 67108864,

        /// <summary>
        /// Right stick is towards the left.
        /// </summary>
        RightThumbstickLeft = 134217728,

        /// <summary>
        /// Left stick is towards up.
        /// </summary>  
        LeftThumbstickUp = 268435456,

        /// <summary>
        /// Left stick is towards down.
        /// </summary>  
        LeftThumbstickDown = 536870912,

        /// <summary>
        /// Left stick is towards the right.
        /// </summary>
        LeftThumbstickRight = 1073741824
    }
}
