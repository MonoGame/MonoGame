// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// A struct that represents the current stick (thumbstick) states for the controller.
    /// </summary>
    public struct GamePadThumbSticks
    {
        private Vector2 _left, _right;

        /// <summary>
        /// Gets a value indicating the position of the left stick (thumbstick). 
        /// </summary>
        /// <value>A <see cref="Vector2"/> indicating the current position of the left stick (thumbstick).</value>
        public Vector2 Left
        {
            get { return _left; }
        }

        /// <summary>
        /// Gets a value indicating the position of the right stick (thumbstick). 
        /// </summary>
        /// <value>A <see cref="Vector2"/> indicating the current position of the right stick (thumbstick).</value>
        public Vector2 Right
        {
            get { return _right; }
        }

        internal Buttons VirtualButtons { get; private set; }

#if DIRECTX && !WINDOWS_PHONE && !WINDOWS_PHONE81 && !WINDOWS_UAP
        // XInput Xbox 360 Controller dead zones
        // Dead zones are slighty different between left and right sticks, this may come from Microsoft usability tests
        private const float leftThumbDeadZone = SharpDX.XInput.Gamepad.LeftThumbDeadZone / (float)short.MaxValue;
        private const float rightThumbDeadZone = SharpDX.XInput.Gamepad.RightThumbDeadZone / (float)short.MaxValue;
#else
        // Default & SDL Xbox 360 Controller dead zones
        // Based on the XInput constants
        private const float leftThumbDeadZone = 0.24f;
        private const float rightThumbDeadZone = 0.265f;
#endif

        public GamePadThumbSticks(Vector2 leftPosition, Vector2 rightPosition) : this()
        {
            _left = leftPosition;
            _right = rightPosition;
            ApplySquareClamp();

            SetVirtualButtons(leftPosition, rightPosition);
        }

        internal GamePadThumbSticks(Vector2 leftPosition, Vector2 rightPosition, GamePadDeadZone deadZoneMode) : this()
        {
            // XNA applies dead zones before rounding/clamping values. The public ctor does not allow this because the dead zone must be known before
            _left = leftPosition;
            _right = rightPosition;
            ApplyDeadZone(deadZoneMode);
            if (deadZoneMode == GamePadDeadZone.Circular)
                ApplyCircularClamp();
            else
                ApplySquareClamp();

            SetVirtualButtons(leftPosition, rightPosition);
        }

        private void ApplySquareClamp()
        {
            _left = new Vector2(MathHelper.Clamp(Left.X, -1f, 1f), MathHelper.Clamp(Left.Y, -1f, 1f));
            _right = new Vector2(MathHelper.Clamp(Right.X, -1f, 1f), MathHelper.Clamp(Right.Y, -1f, 1f));
        }

        private void ApplyCircularClamp()
        {
            if (_left.LengthSquared() > 1f)
                _left.Normalize();
            if (_right.LengthSquared() > 1f)
                _right.Normalize();
        }

        private void ApplyDeadZone(GamePadDeadZone dz)
        {
            switch (dz)
            {
                case GamePadDeadZone.None:
                    break;
                case GamePadDeadZone.IndependentAxes:
                    _left = ExcludeIndependentAxesDeadZone(Left, leftThumbDeadZone);
                    _right = ExcludeIndependentAxesDeadZone(Right, rightThumbDeadZone);
                    break;
                case GamePadDeadZone.Circular:
                    _left = ExcludeCircularDeadZone(Left, leftThumbDeadZone);
                    _right = ExcludeCircularDeadZone(Right, rightThumbDeadZone);
                    break;
            }
        }

        private Vector2 ExcludeIndependentAxesDeadZone(Vector2 value, float deadZone)
        {
            return new Vector2(ExcludeAxisDeadZone(value.X, deadZone), ExcludeAxisDeadZone(value.Y, deadZone));
        }

        private float ExcludeAxisDeadZone(float value, float deadZone)
        {
            if (value < -deadZone)
                value += deadZone;
            else if (value > deadZone)
                value -= deadZone;
            else
                return 0f;
            return value / (1f - deadZone);
        }

        private Vector2 ExcludeCircularDeadZone(Vector2 value, float deadZone)
        {
            var originalLength = value.Length();
            if (originalLength <= deadZone)
                return Vector2.Zero;
            var newLength = (originalLength - deadZone) / (1f - deadZone);
            return value * (newLength / originalLength);
        }

        private void SetVirtualButtons(Vector2 leftPosition, Vector2 rightPosition)
        {
            VirtualButtons = 0;

            // VirtualButtons should always behave like deadzone is IndependentAxes. 
            // This is consistent with XNA behaviour and generally most convenient (e.g. for menu navigation)
            if (leftPosition.X < -leftThumbDeadZone)
                VirtualButtons |= Buttons.LeftThumbstickLeft;
            else if (leftPosition.X > leftThumbDeadZone)
                VirtualButtons |= Buttons.LeftThumbstickRight;

            if (leftPosition.Y < -leftThumbDeadZone)
                VirtualButtons |= Buttons.LeftThumbstickDown;
            else if (leftPosition.Y > leftThumbDeadZone)
                VirtualButtons |= Buttons.LeftThumbstickUp;

            if (rightPosition.X < -rightThumbDeadZone)
                VirtualButtons |= Buttons.RightThumbstickLeft;
            else if (rightPosition.X > rightThumbDeadZone)
                VirtualButtons |= Buttons.RightThumbstickRight;

            if (rightPosition.Y < -rightThumbDeadZone)
                VirtualButtons |= Buttons.RightThumbstickDown;
            else if (rightPosition.Y > rightThumbDeadZone)
                VirtualButtons |= Buttons.RightThumbstickUp;
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="GamePadThumbSticks"/> are equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, false.</returns>
        public static bool operator ==(GamePadThumbSticks left, GamePadThumbSticks right)
        {
            return (left.Left == right.Left) && (left.Right == right.Right);
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="GamePadThumbSticks"/> are not equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.</returns>
        public static bool operator !=(GamePadThumbSticks left, GamePadThumbSticks right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare to this instance.</param>
        /// <returns>true if <paramref name="obj"/> is a <see cref="GamePadThumbSticks"/> and has the same value as this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return (obj is GamePadThumbSticks) && (this == (GamePadThumbSticks)obj);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:Microsoft.Xna.Framework.Input.GamePadThumbSticks"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Left.GetHashCode() * 397) ^ Right.GetHashCode();
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadThumbSticks"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadThumbSticks"/>.</returns>
        public override string ToString()
        {
            return "[GamePadThumbSticks: Left=" + Left + ", Right=" + Right + "]";
        }
    }
}
