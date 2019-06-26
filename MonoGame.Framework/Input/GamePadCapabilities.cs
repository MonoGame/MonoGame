// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// A stuct that represents the controller capabilities.
    /// </summary>
    public struct GamePadCapabilities
    {
        /// <summary>
        /// Gets a value indicating if the controller is connected.
        /// </summary>
        /// <value><c>true</c> if it is connected; otherwise, <c>false</c>.</value>
        public bool IsConnected { get; internal set; }

        /// <summary>
        /// Gets the gamepad display name.
        /// 
        /// This property is not available in XNA.
        /// </summary>
        /// <value>String representing the display name of the gamepad.</value>
        public string DisplayName { get; internal set; }

        /// <summary>
        /// Gets the unique identifier of the gamepad.
        /// 
        /// This property is not available in XNA.
        /// </summary>
        /// <value>String representing the unique identifier of the gamepad.</value>
        public string Identifier { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the button A.
        /// </summary>
        /// <value><c>true</c> if it has the button A; otherwise, <c>false</c>.</value>
        public bool HasAButton { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the button Back.
        /// </summary>
        /// <value><c>true</c> if it has the button Back; otherwise, <c>false</c>.</value>
        public bool HasBackButton { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the button B.
        /// </summary>
        /// <value><c>true</c> if it has the button B; otherwise, <c>false</c>.</value>
        public bool HasBButton { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the directional pad down button.
        /// </summary>
        /// <value><c>true</c> if it has the directional pad down button; otherwise, <c>false</c>.</value>
        public bool HasDPadDownButton { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the directional pad left button.
        /// </summary>
        /// <value><c>true</c> if it has the directional pad left button; otherwise, <c>false</c>.</value>
        public bool HasDPadLeftButton { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the directional pad right button.
        /// </summary>
        /// <value><c>true</c> if it has the directional pad right button; otherwise, <c>false</c>.</value>
        public bool HasDPadRightButton { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the directional pad up button.
        /// </summary>
        /// <value><c>true</c> if it has the directional pad up button; otherwise, <c>false</c>.</value>
        public bool HasDPadUpButton { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the left shoulder button.
        /// </summary>
        /// <value><c>true</c> if it has the left shoulder button; otherwise, <c>false</c>.</value>
        public bool HasLeftShoulderButton { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the left stick button.
        /// </summary>
        /// <value><c>true</c> if it has the left stick button; otherwise, <c>false</c>.</value>
        public bool HasLeftStickButton { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the right shoulder button.
        /// </summary>
        /// <value><c>true</c> if it has the right shoulder button; otherwise, <c>false</c>.</value>
        public bool HasRightShoulderButton { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the right stick button.
        /// </summary>
        /// <value><c>true</c> if it has the right stick button; otherwise, <c>false</c>.</value>
        public bool HasRightStickButton { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the button Start.
        /// </summary>
        /// <value><c>true</c> if it has the button Start; otherwise, <c>false</c>.</value>
        public bool HasStartButton { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the button X.
        /// </summary>
        /// <value><c>true</c> if it has the button X; otherwise, <c>false</c>.</value>
        public bool HasXButton { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the button Y.
        /// </summary>
        /// <value><c>true</c> if it has the button Y; otherwise, <c>false</c>.</value>
        public bool HasYButton { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the guide button.
        /// </summary>
        /// <value><c>true</c> if it has the guide button; otherwise, <c>false</c>.</value>
        public bool HasBigButton { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has X axis for the left stick (thumbstick) button.
        /// </summary>
        /// <value><c>true</c> if it has X axis for the left stick (thumbstick) button; otherwise, <c>false</c>.</value>
        public bool HasLeftXThumbStick { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has Y axis for the left stick (thumbstick) button.
        /// </summary>
        /// <value><c>true</c> if it has Y axis for the left stick (thumbstick) button; otherwise, <c>false</c>.</value>
        public bool HasLeftYThumbStick { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has X axis for the right stick (thumbstick) button.
        /// </summary>
        /// <value><c>true</c> if it has X axis for the right stick (thumbstick) button; otherwise, <c>false</c>.</value>
        public bool HasRightXThumbStick { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has Y axis for the right stick (thumbstick) button.
        /// </summary>
        /// <value><c>true</c> if it has Y axis for the right stick (thumbstick) button; otherwise, <c>false</c>.</value>
        public bool HasRightYThumbStick { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the left trigger button.
        /// </summary>
        /// <value><c>true</c> if it has the left trigger button; otherwise, <c>false</c>.</value>
        public bool HasLeftTrigger { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the right trigger button.
        /// </summary>
        /// <value><c>true</c> if it has the right trigger button; otherwise, <c>false</c>.</value>
        public bool HasRightTrigger { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the left vibration motor.
        /// </summary>
        /// <value><c>true</c> if it has the left vibration motor; otherwise, <c>false</c>.</value>
        public bool HasLeftVibrationMotor { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has the right vibration motor.
        /// </summary>
        /// <value><c>true</c> if it has the right vibration motor; otherwise, <c>false</c>.</value>
        public bool HasRightVibrationMotor { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the controller has a microphone.
        /// </summary>
        /// <value><c>true</c> if it has a microphone; otherwise, <c>false</c>.</value>
        public bool HasVoiceSupport { get; internal set; }

        /// <summary>
        /// Gets the type of the controller.
        /// </summary>
        /// <value>A <see cref="GamePadType"/> representing the controller type..</value>
        public GamePadType GamePadType { get; internal set; }

        /// <summary>
        /// Determines whether a specified instance of <see cref="Microsoft.Xna.Framework.Input.GamePadCapabilities"/>
        /// is equal to another specified <see cref="Microsoft.Xna.Framework.Input.GamePadCapabilities"/>.
        /// </summary>
        /// <param name="left">The first <see cref="Microsoft.Xna.Framework.Input.GamePadCapabilities"/> to compare.</param>
        /// <param name="right">The second <see cref="Microsoft.Xna.Framework.Input.GamePadCapabilities"/> to compare.</param>
        /// <returns><c>true</c> if <c>left</c> and <c>right</c> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(GamePadCapabilities left, GamePadCapabilities right)
        {
            var eq = true;

            eq &= (left.DisplayName == right.DisplayName);
            eq &= (left.Identifier == right.Identifier);
            eq &= (left.IsConnected == right.IsConnected);
            eq &= (left.HasAButton == right.HasAButton);
            eq &= (left.HasBackButton == right.HasBackButton);
            eq &= (left.HasBButton == right.HasBButton);
            eq &= (left.HasDPadDownButton == right.HasDPadDownButton);
            eq &= (left.HasDPadLeftButton == right.HasDPadLeftButton);
            eq &= (left.HasDPadRightButton == right.HasDPadRightButton);
            eq &= (left.HasDPadUpButton == right.HasDPadUpButton);
            eq &= (left.HasLeftShoulderButton == right.HasLeftShoulderButton);
            eq &= (left.HasLeftStickButton == right.HasLeftStickButton);
            eq &= (left.HasRightShoulderButton == right.HasRightShoulderButton);
            eq &= (left.HasRightStickButton == right.HasRightStickButton);
            eq &= (left.HasStartButton == right.HasStartButton);
            eq &= (left.HasXButton == right.HasXButton);
            eq &= (left.HasYButton == right.HasYButton);
            eq &= (left.HasBigButton == right.HasBigButton);
            eq &= (left.HasLeftXThumbStick == right.HasLeftXThumbStick);
            eq &= (left.HasLeftYThumbStick == right.HasLeftYThumbStick);
            eq &= (left.HasRightXThumbStick == right.HasRightXThumbStick);
            eq &= (left.HasRightYThumbStick == right.HasRightYThumbStick);
            eq &= (left.HasLeftTrigger == right.HasLeftTrigger);
            eq &= (left.HasRightTrigger == right.HasRightTrigger);
            eq &= (left.HasLeftVibrationMotor == right.HasLeftVibrationMotor);
            eq &= (left.HasRightVibrationMotor == right.HasRightVibrationMotor);
            eq &= (left.HasVoiceSupport == right.HasVoiceSupport);
            eq &= (left.GamePadType == right.GamePadType);

            return eq;
        }

        /// <summary>
        /// Determines whether a specified instance of <see cref="Microsoft.Xna.Framework.Input.GamePadCapabilities"/>
        /// is not equal to another specified <see cref="Microsoft.Xna.Framework.Input.GamePadCapabilities"/>.
        /// </summary>
        /// <param name="left">The first <see cref="Microsoft.Xna.Framework.Input.GamePadCapabilities"/> to compare.</param>
        /// <param name="right">The second <see cref="Microsoft.Xna.Framework.Input.GamePadCapabilities"/> to compare.</param>
        /// <returns><c>true</c> if <c>left</c> and <c>right</c> are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(GamePadCapabilities left, GamePadCapabilities right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return (obj is GamePadCapabilities) && (this == (GamePadCapabilities)obj);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities"/>.</returns>
        public override string ToString()
        {
            return "[GamePadCapabilities: IsConnected=" + IsConnected +
                ", DisplayName=" + DisplayName +
                ", Identifier=" + Identifier +
                ", HasAButton=" + HasAButton +
                ", HasBackButton=" + HasBackButton +
                ", HasBButton=" + HasBButton +
                ", HasDPadDownButton=" + HasDPadDownButton +
                ", HasDPadLeftButton=" + HasDPadLeftButton +
                ", HasDPadRightButton=" + HasDPadRightButton +
                ", HasDPadUpButton=" + HasDPadUpButton +
                ", HasLeftShoulderButton=" + HasLeftShoulderButton +
                ", HasLeftStickButton=" + HasLeftStickButton +
                ", HasRightShoulderButton=" + HasRightShoulderButton +
                ", HasRightStickButton=" + HasRightStickButton +
                ", HasStartButton=" + HasStartButton +
                ", HasXButton=" + HasXButton +
                ", HasYButton=" + HasYButton +
                ", HasBigButton=" + HasBigButton +
                ", HasLeftXThumbStick=" + HasLeftXThumbStick +
                ", HasLeftYThumbStick=" + HasLeftYThumbStick +
                ", HasRightXThumbStick=" + HasRightXThumbStick +
                ", HasRightYThumbStick=" + HasRightYThumbStick +
                ", HasLeftTrigger=" + HasLeftTrigger +
                ", HasRightTrigger=" + HasRightTrigger +
                ", HasLeftVibrationMotor=" + HasLeftVibrationMotor +
                ", HasRightVibrationMotor=" + HasRightVibrationMotor +
                ", HasVoiceSupport=" + HasVoiceSupport +
                ", GamePadType=" + GamePadType +
                "]";
        }
    }
}
