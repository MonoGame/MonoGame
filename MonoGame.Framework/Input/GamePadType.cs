// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Defines a type of gamepad.
    /// </summary>
    public enum GamePadType
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// GamePad is the Xbox controller.
        /// </summary>
        GamePad,

        /// <summary>
        /// GamePad is a wheel.
        /// </summary>
        Wheel,

        /// <summary>
        /// GamePad is an arcade stick.
        /// </summary>
        ArcadeStick,

        /// <summary>
        /// GamePad is a flight stick.
        /// </summary>
        FlightStick,

        /// <summary>
        /// GamePad is a dance pad.
        /// </summary>
        DancePad,

        /// <summary>
        /// GamePad is a guitar.
        /// </summary>
        Guitar,

        /// <summary>
        /// GamePad is an alternate guitar.
        /// </summary>
        AlternateGuitar,

        /// <summary>
        /// GamePad is a drum kit.
        /// </summary>
        DrumKit,

        /// <summary>
        /// GamePad is a big button pad.
        /// </summary>
        BigButtonPad = 768
    }
}