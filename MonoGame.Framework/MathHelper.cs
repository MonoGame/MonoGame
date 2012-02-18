#region License
/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework
{
    public static class MathHelper
    {
        public const float E = (float)Math.E;
        public const float Log10E = 0.4342945f;
        public const float Log2E = 1.442695f;
        public const float Pi = (float)Math.PI;
        public const float PiOver2 = (float)(Math.PI / 2.0);
        public const float PiOver4 = (float)(Math.PI / 4.0);
        public const float TwoPi = (float)(Math.PI * 2.0);
        
        public static float Barycentric(float value1, float value2, float value3, float amount1, float amount2)
        {
            return value1 + (value2 - value1) * amount1 + (value3 - value1) * amount2;
        }

        public static float CatmullRom(float value1, float value2, float value3, float value4, float amount)
        {
            // Using formula from http://www.mvps.org/directx/articles/catmull/
            // Internally using doubles not to lose precission
            double amountSquared = amount * amount;
            double amountCubed = amountSquared * amount;
            return (float)(0.5 * (2.0 * value2 +
                (value3 - value1) * amount +
                (2.0 * value1 - 5.0 * value2 + 4.0 * value3 - value4) * amountSquared +
                (3.0 * value2 - value1 - 3.0 * value3 + value4) * amountCubed));
        }

        public static float Clamp(float value, float min, float max)
        {
            // First we check to see if we're greater than the max
            value = (value > max) ? max : value;

            // Then we check to see if we're less than the min.
            value = (value < min) ? min : value;

            // There's no check to see if min > max.
            return value;
        }
        
        public static float Distance(float value1, float value2)
        {
            return Math.Abs(value1 - value2);
        }
        
        public static float Hermite(float value1, float tangent1, float value2, float tangent2, float amount)
        {
            // All transformed to double not to lose precission
            // Otherwise, for high numbers of param:amount the result is NaN instead of Infinity
            double v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount, result;
            double sCubed = s * s * s;
            double sSquared = s * s;

            if (amount == 0f)
                result = value1;
            else if (amount == 1f)
                result = value2;
            else
                result = (2 * v1 - 2 * v2 + t2 + t1) * sCubed +
                    (3 * v2 - 3 * v1 - 2 * t1 - t2) * sSquared +
                    t1 * s +
                    v1;
            return (float)result;
        }
        
        
        public static float Lerp(float value1, float value2, float amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        public static float Max(float value1, float value2)
        {
            return Math.Max(value1, value2);
        }
        
        public static float Min(float value1, float value2)
        {
            return Math.Min(value1, value2);
        }
        
        public static float SmoothStep(float value1, float value2, float amount)
        {
            // It is expected that 0 < amount < 1
            // If amount < 0, return value1
            // If amount > 1, return value2
#if(USE_FARSEER)
            float result = SilverSpriteMathHelper.Clamp(amount, 0f, 1f);
            result = SilverSpriteMathHelper.Hermite(value1, 0f, value2, 0f, result);
#else
            float result = MathHelper.Clamp(amount, 0f, 1f);
            result = MathHelper.Hermite(value1, 0f, value2, 0f, result);
#endif
            return result;
        }
        
        public static float ToDegrees(float radians)
        {
            // This method uses double precission internally,
            // though it returns single float
            // Factor = 180 / pi
            return (float)(radians * 57.295779513082320876798154814105);
        }
        
        public static float ToRadians(float degrees)
        {
            // This method uses double precission internally,
            // though it returns single float
            // Factor = pi / 180
            return (float)(degrees * 0.017453292519943295769236907684886);
        }

	public static float WrapAngle(float angle)
	{
		angle = (float)Math.IEEERemainder((double)angle, 6.2831854820251465);
		if (angle <= -3.14159274f)
		{
			angle += 6.28318548f;
		}
		else
		{
		if (angle > 3.14159274f)
		{
			angle -= 6.28318548f;
		}
		}
		return angle;
	}

		public static bool IsPowerOfTwo(int value)
		{
			return (value > 0) && ((value & (value - 1)) == 0);
		}
    }
}
