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
        public enum GateType
        {
            None,
            Round,
            Square
        };
        public static GateType Gate = GateType.Round;

        Vector2 left;
        Vector2 right;

        public Vector2 Left
        {
        	get
            {
                return left;
            }
        	internal set
            {
        		switch (Gate)
                {
        		case GateType.None:
        			left = value;
        			break;
        		case GateType.Round:
        			if (value.LengthSquared () > 1f)
        				left = Vector2.Normalize (value);
        			else
        				left = value;
        			break;
                    case GateType.Square:
                        left = new Vector2(MathHelper.Clamp(value.X, -1f, 1f), MathHelper.Clamp(value.Y, -1f, 1f));
                        break;
                    default:
                        left = Vector2.Zero;
                        break;
                }
            }
        }
        public Vector2 Right
        {
        	get
            {
                return right;
            }
        	internal set
            {
        		switch (Gate)
                {
        		case GateType.None:
        			right = value;
        			break;
        		case GateType.Round:
        			if (value.LengthSquared () > 1f)
        				right = Vector2.Normalize (value);
        			else
        				right = value;
        			break;
                    case GateType.Square:
                        right = new Vector2(MathHelper.Clamp(value.X, -1f, 1f), MathHelper.Clamp(value.Y, -1f, 1f));
                        break;
                    default:
                        right = Vector2.Zero;
                        break;
                }
            }
        }

		public GamePadThumbSticks(Vector2 leftPosition, Vector2 rightPosition):this()
		{
			Left = leftPosition;
			Right = rightPosition;
		}

        internal void ApplyDeadZone(GamePadDeadZone dz, float size)
        {
            switch (dz)
            {
                case GamePadDeadZone.None:
                    break;
                case GamePadDeadZone.IndependentAxes:
                    if (Math.Abs(left.X) < size)
                        left.X = 0f;
                    if (Math.Abs(left.Y) < size)
                        left.Y = 0f;
                    if (Math.Abs(right.X) < size)
                        right.X = 0f;
                    if (Math.Abs(right.Y) < size)
                        right.Y = 0f;
                    break;
                case GamePadDeadZone.Circular:
                    if (left.LengthSquared() < size * size)
                        left = Vector2.Zero;
                    if (right.LengthSquared() < size * size)
                        right = Vector2.Zero;
                    break;
            }
        }
    }
}