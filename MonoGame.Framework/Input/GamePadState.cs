// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using System;

namespace Microsoft.Xna.Framework.Input
{
    //
    // Summary:
    //     Represents specific information about the state of an Xbox 360 Controller,
    //     including the current state of buttons and sticks. Reference page contains
    //     links to related code samples.
    //     This is implemented as a partial struct to allow for individual platforms
    //     to offer additional data without separate state queries to GamePad.
    public partial struct GamePadState
    {
        /// <summary>
        /// The default initialized gamepad state.
        /// </summary>
        public static readonly GamePadState Default = new GamePadState();

        //
        // Summary:
        //     Indicates whether the Xbox 360 Controller is connected. Reference page contains
        //     links to related code samples.
        public bool IsConnected
        {
            get;
            internal set;
        }
        //
        // Summary:
        //     Gets the packet number associated with this state. Reference page contains
        //     links to related code samples.
        public int PacketNumber
        {
            get;
            internal set;
        }
        
        //
        // Summary:
        //     Returns a structure that identifies what buttons on the Xbox 360 controller
        //     are pressed. Reference page contains links to related code samples.
        public GamePadButtons Buttons
        {
            get;
            internal set;
        }
        //
        // Summary:
        //     Returns a structure that identifies what directions of the directional pad
        //     on the Xbox 360 Controller are pressed.
        public GamePadDPad DPad
        {
            get;
            internal set;
        }
        //
        // Summary:
        //     Returns a structure that indicates the position of the Xbox 360 Controller
        //     sticks (thumbsticks).
        public GamePadThumbSticks ThumbSticks
        {
            get;
            internal set;
        }
        //
        // Summary:
        //     Returns a structure that identifies the position of triggers on the Xbox
        //     360 controller.
        public GamePadTriggers Triggers
        {
            get;
            internal set;
        }

        //
        // Summary:
        //     Initializes a new instance of the GamePadState class using the specified
        //     GamePadThumbSticks, GamePadTriggers, GamePadButtons, and GamePadDPad.
        //
        // Parameters:
        //   thumbSticks:
        //     Initial thumbstick state.
        //
        //   triggers:
        //     Initial trigger state.
        //
        //   buttons:
        //     Initial button state.
        //
        //   dPad:
        //     Initial directional pad state.
        public GamePadState(GamePadThumbSticks thumbSticks, GamePadTriggers triggers, GamePadButtons buttons, GamePadDPad dPad)
            : this()
        {
            ThumbSticks = thumbSticks;
            Triggers = triggers;
            Buttons = buttons;
            DPad = dPad;
			IsConnected = true;

            PlatformConstruct();
        }
        //
        // Summary:
        //     Initializes a new instance of the GamePadState class with the specified stick,
        //     trigger, and button values.
        //
        // Parameters:
        //   leftThumbStick:
        //     Left stick value. Each axis is clamped between −1.0 and 1.0.
        //
        //   rightThumbStick:
        //     Right stick value. Each axis is clamped between −1.0 and 1.0.
        //
        //   leftTrigger:
        //     Left trigger value. This value is clamped between 0.0 and 1.0.
        //
        //   rightTrigger:
        //     Right trigger value. This value is clamped between 0.0 and 1.0.
        //
        //   buttons:
        //     Array or parameter list of Buttons to initialize as pressed.
        public GamePadState(Vector2 leftThumbStick, Vector2 rightThumbStick, float leftTrigger, float rightTrigger, params Buttons[] buttons)
            : this(new GamePadThumbSticks(leftThumbStick, rightThumbStick), new GamePadTriggers(leftTrigger, rightTrigger), new GamePadButtons(buttons), new GamePadDPad(buttons))
        {
        }

        /// <summary>
        /// Define this method in platform partial classes to initialize default
        /// values for platform-specific fields.
        /// </summary>
        partial void PlatformConstruct();
  
        /// <summary>
        /// Gets the button mask along with 'virtual buttons' like LeftThumbstickLeft.
        /// </summary>
        private Buttons GetVirtualButtons () {
            var result = Buttons.buttons;
            var sticks = ThumbSticks;
            
            if (sticks.Left.X < 0)
                result |= Microsoft.Xna.Framework.Input.Buttons.LeftThumbstickLeft;
            else if (sticks.Left.X > 0)
                result |= Microsoft.Xna.Framework.Input.Buttons.LeftThumbstickRight;
            
            if (sticks.Left.Y < 0)
                result |= Microsoft.Xna.Framework.Input.Buttons.LeftThumbstickDown;
            else if (sticks.Left.Y > 0)
                result |= Microsoft.Xna.Framework.Input.Buttons.LeftThumbstickUp;
            
            if (sticks.Right.X < 0)
                result |= Microsoft.Xna.Framework.Input.Buttons.RightThumbstickLeft;
            else if (sticks.Right.X > 0)
                result |= Microsoft.Xna.Framework.Input.Buttons.RightThumbstickRight;
            
            if (sticks.Right.Y < 0)
                result |= Microsoft.Xna.Framework.Input.Buttons.RightThumbstickDown;
            else if (sticks.Right.Y > 0)
                result |= Microsoft.Xna.Framework.Input.Buttons.RightThumbstickUp;

            if (DPad.Down == ButtonState.Pressed)
                result |= Microsoft.Xna.Framework.Input.Buttons.DPadDown;
            if (DPad.Up == ButtonState.Pressed)
                result |= Microsoft.Xna.Framework.Input.Buttons.DPadUp;
            if (DPad.Left == ButtonState.Pressed)
                result |= Microsoft.Xna.Framework.Input.Buttons.DPadLeft;
            if (DPad.Right == ButtonState.Pressed)
                result |= Microsoft.Xna.Framework.Input.Buttons.DPadRight;

            return result;
        }
        
        //
        // Summary:
        //     Determines whether specified input device buttons are pressed in this GamePadState.
        //
        // Parameters:
        //   button:
        //     Buttons to query. Specify a single button, or combine multiple buttons using
        //     a bitwise OR operation.
        public bool IsButtonDown(Buttons button)
        {
            return (GetVirtualButtons() & button) == button;
        }
        //
        // Summary:
        //     Determines whether specified input device buttons are up (not pressed) in
        //     this GamePadState.
        //
        // Parameters:
        //   button:
        //     Buttons to query. Specify a single button, or combine multiple buttons using
        //     a bitwise OR operation.
        public bool IsButtonUp(Buttons button)
        {
            return (GetVirtualButtons() & button) != button;
        }

        //
        // Summary:
        //     Determines whether two GamePadState instances are not equal.
        //
        // Parameters:
        //   left:
        //     Object on the left of the equal sign.
        //
        //   right:
        //     Object on the right of the equal sign.
        public static bool operator !=(GamePadState left, GamePadState right)
        {
            return !left.Equals(right);
        }
        //
        // Summary:
        //     Determines whether two GamePadState instances are equal.
        //
        // Parameters:
        //   left:
        //     Object on the left of the equal sign.
        //
        //   right:
        //     Object on the right of the equal sign.
        public static bool operator ==(GamePadState left, GamePadState right)
        {
            return left.Equals(right);
        }
        //
        // Summary:
        //     Returns a value that indicates whether the current instance is equal to a
        //     specified object.
        //
        // Parameters:
        //   obj:
        //     Object with which to make the comparison.
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        //
        // Summary:
        //     Gets the hash code for this instance.
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        //
        // Summary:
        //     Retrieves a string representation of this object.
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
