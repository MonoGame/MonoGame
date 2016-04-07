// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input
{
    public struct GamePadThumbSticks
    {
        Vector2 left;
        Vector2 right;

        public Vector2 Left
        {
        	get
            {
                return left;
            }
        }
        public Vector2 Right
        {
        	get
            {
                return right;
            }
        }

        internal Buttons VirtualButtons { get; private set; }

#if DIRECTX && !WINDOWS_PHONE && !WINDOWS_PHONE81 && !WINDOWS_UAP
        // XInput Xbox 360 Controller dead zones
        // Dead zones are slighty different between left and right sticks, this may come from Microsoft usability tests
        private const float leftThumbDeadZone = SharpDX.XInput.Gamepad.LeftThumbDeadZone / (float)short.MaxValue;
        private const float rightThumbDeadZone = SharpDX.XInput.Gamepad.RightThumbDeadZone / (float)short.MaxValue;
#elif OUYA
        // OUYA dead zones should
        // They are a bit larger to accomodate OUYA Gamepad (but will also affect Xbox 360 controllers plugged to an OUYA)
        private const float leftThumbDeadZone = 0.3f;
        private const float rightThumbDeadZone = 0.3f;
#else
        // Default & SDL Xbox 360 Controller dead zones
        // Based on the XInput constants
        private const float leftThumbDeadZone = 0.24f;
        private const float rightThumbDeadZone = 0.265f;
#endif

		public GamePadThumbSticks(Vector2 leftPosition, Vector2 rightPosition):this()
		{
			left = leftPosition;
			right = rightPosition;
            ApplySquareClamp();

            SetVirtualButtons(leftPosition, rightPosition);
		}

        internal GamePadThumbSticks(Vector2 leftPosition, Vector2 rightPosition, GamePadDeadZone deadZoneMode):this()
        {
            // XNA applies dead zones before rounding/clamping values. The public ctor does not allow this because the dead zone must be known before
            left = leftPosition;
            right = rightPosition;
            ApplyDeadZone(deadZoneMode);
            if (deadZoneMode == GamePadDeadZone.Circular)
                ApplyCircularClamp();
            else
                ApplySquareClamp();

            SetVirtualButtons(leftPosition, rightPosition);
        }

        private void ApplySquareClamp()
        {
            left = new Vector2(MathHelper.Clamp(left.X, -1f, 1f), MathHelper.Clamp(left.Y, -1f, 1f));
            right = new Vector2(MathHelper.Clamp(right.X, -1f, 1f), MathHelper.Clamp(right.Y, -1f, 1f));
        }

        private void ApplyCircularClamp()
        {
            if (left.LengthSquared() > 1f)
                left.Normalize();
            if (right.LengthSquared() > 1f)
                right.Normalize();
        }

        private void ApplyDeadZone(GamePadDeadZone dz)
        {
            switch (dz)
            {
                case GamePadDeadZone.None:
                    break;
                case GamePadDeadZone.IndependentAxes:
                    left = ExcludeIndependentAxesDeadZone(left, leftThumbDeadZone);
                    right = ExcludeIndependentAxesDeadZone(right, rightThumbDeadZone);
                    break;
                case GamePadDeadZone.Circular:
                    left = ExcludeCircularDeadZone(left, leftThumbDeadZone);
                    right = ExcludeCircularDeadZone(right, rightThumbDeadZone);
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
            return (left.left == right.left)
                && (left.right == right.right);
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

        public override int GetHashCode ()
        {
            return this.Left.GetHashCode () + 37 * this.Right.GetHashCode ();
        }
    }
}
