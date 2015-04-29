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

            #region Int32 Next methods

            /// <summary>
            /// Returns a non-negative random integer of type Int32.
            /// </summary>
            /// <returns>Random integer of type Int32</returns>
            public int Next()
            {
                return (int)Remap(UInt64ToInt32(XORShift64Star()), int.MinValue, int.MaxValue, 0, int.MaxValue);
            }

            /// <summary>
            /// Returns a non-negative random integer of type Int32 that is less than the specified maximum.
            /// </summary>
            /// <param name="max">Maximum possible value</param>
            /// <returns>Random integer of type Int32</returns>
            public int Next(int max)
            {
                return (int)Remap(UInt64ToInt32(XORShift64Star()), int.MinValue, int.MaxValue, 0, max);
            }

            /// <summary>
            /// Returns a random integer of type Int32 that is within a specified range.
            /// </summary>
            /// <param name="min">Minimum possible value</param>
            /// <param name="max">Maximum possible value</param>
            /// <returns>Random integer of type Int32</returns>
            public int Next(int min, int max)
            {
                return (int)Remap(UInt64ToInt32(XORShift64Star()), int.MinValue, int.MaxValue, min, max);
            }

            #endregion

            #region Int64 Next methods

            /// <summary>
            /// Returns a non-negative random integer of type Int64.
            /// </summary>
            /// <returns>Random integer of type Int64</returns>
            public long Next()
            {
                return Remap(UInt64ToInt64(XORShift64Star()), long.MinValue, long.MaxValue, 0, long.MaxValue);
            }

            /// <summary>
            /// Returns a non-negative random integer of type Int64 that is less than the specified maximum.
            /// </summary>
            /// <param name="max">Maximum possible value</param>
            /// <returns>Random integer of type Int64</returns>
            public long Next(long max)
            {
                return Remap(UInt64ToInt64(XORShift64Star()), long.MinValue, long.MaxValue, 0, max);
            }

            /// <summary>
            /// Returns a random integer of type Int64 that is within a specified range.
            /// </summary>
            /// <param name="min">Minimum possible value</param>
            /// <param name="max">Maximum possible value</param>
            /// <returns>Random integer of type Int64</returns>
            public long Next(long min, long max)
            {
                return Remap(UInt64ToInt64(XORShift64Star()), long.MinValue, long.MaxValue, min, max);
            }

            #endregion

            #endregion

            #region Private methods

            private ulong XORShift64Star()
            {
                _lastValue ^= _lastValue >> 12;
                _lastValue ^= _lastValue << 25;
                _lastValue ^= _lastValue >> 27;
                return unchecked(_lastValue * 2685821657736338717UL);
            }

            private long Remap(long value, long oldMin, long oldMax, long newMin, long newMax)
            {
                return (((value - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin;
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
