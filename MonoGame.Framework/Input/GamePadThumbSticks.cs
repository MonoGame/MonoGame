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

using Microsoft.Xna.Framework;
using System;

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

		public GamePadThumbSticks(Vector2 leftPosition, Vector2 rightPosition):this()
		{
			left = leftPosition;
			right = rightPosition;
            ApplySquareClamp();
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
#if DIRECTX && !WINDOWS_PHONE && !WINDOWS_PHONE81 && !WINDOWS_UAP
            // XInput Xbox 360 Controller dead zones
            // Dead zones are slighty different between left and right sticks, this may come from Microsoft usability tests
            const float leftThumbDeadZone = SharpDX.XInput.Gamepad.LeftThumbDeadZone / (float)short.MaxValue;
            const float rightThumbDeadZone = SharpDX.XInput.Gamepad.RightThumbDeadZone / (float)short.MaxValue;
#elif OUYA
            // OUYA dead zones should
            // They are a bit larger to accomodate OUYA Gamepad (but will also affect Xbox 360 controllers plugged to an OUYA)
            const float leftThumbDeadZone = 0.3f;
            const float rightThumbDeadZone = 0.3f;
#else
            // Default & SDL Xbox 360 Controller dead zones
            // Based on the XInput constants
            const float leftThumbDeadZone = 0.24f;
            const float rightThumbDeadZone = 0.265f;
#endif
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
