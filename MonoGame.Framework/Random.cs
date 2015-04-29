using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework
{
    public static partial class MathHelper
    {
        /// <summary>
        /// Class for generating random numbers consistantly when using Mono or Microsoft compilers
        /// </summary>
        public class Random
        {
            #region Static methods

            public static ulong GenerateSeed()
            {
                byte[] guid = new Guid().ToByteArray();
                return (BitConverter.ToUInt64(guid, 0) ^ BitConverter.ToUInt64(guid, 7));
            }

            #endregion

            #region Private Fields

            private ulong _seed;
            private ulong _lastValue;

            #endregion

            #region Properties

            public ulong Seed
            {
                get
                {
                    return this._seed;
                }
                set
                {
                    this._seed = value;
                    this._lastValue = value;
                }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the random number generator using a new GUID as a default seed value
            /// </summary>
            public Random()
                : this(GenerateSeed())
            {
            }

            /// <summary>
            /// Initializes a new instance of the random number generator using the specified seed
            /// </summary>
            /// <param name="seed">Seed to be used when computing new random numbers</param>
            public Random(ulong seed)
            {
                this.Seed = seed;
            }

            #endregion

            #region Public methods

            public int Next()
            {
                return UInt64ToInt32(XORShift64Star());
            }

            #endregion

            #region Private methods

            private ulong XORShift64Star()
            {
                _lastValue ^= _lastValue >> 12;
                _lastValue ^= _lastValue << 25;
                _lastValue ^= _lastValue >> 27;
                return unchecked(_lastValue * 2685821657736338717UL);
            }

            private ulong Remap(ulong value, ulong newMax)
            {
                //return (((value - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin;
                return ((value * newMax) / ulong.MaxValue);
            }

            #region Integer values

            private byte UInt64ToInt8(ulong uint64)
            {
                // Map the ulong onto a long, then remap that to a byte range by multiplying by the factor of 
                // the maximum value of a byte divided by the maximum value of a long. However, if the long 
                // is negative, return the same multiplied by -1.
                if (UInt64ToInt64(uint64) < 0)
                    return (byte)(-UInt64ToInt64(uint64) * ((double)byte.MaxValue / long.MaxValue));
                return (byte)(UInt64ToInt64(uint64) * ((double)byte.MaxValue / long.MaxValue));
            }

            private short UInt64ToInt16(ulong uint64)
            {
                // Map the ulong onto a long, then remap that to a short range by multiplying by the factor of 
                // the maximum value of a short divided by the maximum value of a long
                return (short)(UInt64ToInt64(uint64) * ((double)short.MaxValue / long.MaxValue));
            }

            private int UInt64ToInt32(ulong uint64)
            {
                // Map the ulong onto a long, then remap that to an integer range by multiplying by the factor of 
                // the maximum value of an integer divided by the maximum value of a long
                return (int)(UInt64ToInt64(uint64) * ((double)int.MaxValue / long.MaxValue));
            }

            private long UInt64ToInt64(ulong uint64)
            {
                unsafe
                {
                    return (*(long*)&uint64) + long.MinValue;
                }
            }

            #endregion

            #region Floating point values

            private float UInt64ToFloat(ulong uint64)
            {
                return (float)UInt64ToDouble(uint64);
            }

            private double UInt64ToDouble(ulong uint64)
            {
                return Convert.ToDouble(UInt64ToInt64(uint64));
            }

            #endregion

            #endregion
        }
    }
}
