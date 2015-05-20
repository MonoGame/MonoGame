using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework
{
    public static partial class MathHelper
    {
        /// <summary>
        /// Class for generating random numbers consistantly when using Mono or Microsoft compilers.
        /// </summary>
        public class Random
        {
            #region Static methods

            /// <summary>
            /// Utility method for generating a seed for the random number generator.
            /// </summary>
            [ClsCompliant(false)]
            public static ulong GenerateSeed()
            {
                byte[] guid = Guid.NewGuid().ToByteArray();
                return (BitConverter.ToUInt64(guid, 0) ^ BitConverter.ToUInt64(guid, 7));
            }

            #endregion

            #region Private Fields

            private ulong _seed;
            private ulong _state;
            private ulong _seq;

            #endregion

            #region Properties

            /// <summary>
            /// Original seed for the random number generator. Assigning to this will 
            /// reset the RNG to this state as well as assigning the seed.
            /// </summary>
            [ClsCompliant(false)]
            public ulong Seed
            {
                get
                {
                    return this._seed;
                }
                set
                {
                    this._seed = value;
                    this._state = value;
                }
            }
            
            /// <summary>
            /// Read-only variable containing the current state of the random number generator internally.
            /// This variable can be assigned as the seed of another random number generator to make them
            /// produce the same output, or can be used to save the state of the RNG.
            /// </summary>
            [ClsCompliant(false)]
            public ulong State
            {
                get
                {
                    return this._state;
                }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the random number generator using the value of 
            /// Random.GenerateSeed() as a default seed value.
            /// </summary>
            public Random()
                : this(GenerateSeed())
            {
            }

            /// <summary>
            /// Initializes a new instance of the random number generator using the specified seed
            /// </summary>
            /// <param name="seed">Seed to be used when computing new random numbers</param>
            [ClsCompliant(false)]
            public Random(ulong seed)
            {
                this.Seed = seed;
            }

            #endregion

            #region Public methods

            #region Integer Next methods

            #region Int8 Next methods

            /// <summary>
            /// Returns a non-negative random integer of type Int8 (byte).
            /// </summary>
            /// <returns>Random integer of type Int8 (byte)</returns>
            public byte NextByte()
            {
                return UInt64ToInt8(PcgXshRS());
            }

            /// <summary>
            /// Returns a non-negative random integer of type Int8 (byte) that is less than the specified maximum.
            /// </summary>
            /// <param name="max">Maximum possible value</param>
            /// <returns>Random integer of type Int8 (byte)</returns>
            public byte NextByte(byte max)
            {
                return (byte)Remap(UInt64ToInt8(PcgXshRS()), 0, byte.MaxValue, 0, max);
            }

            /// <summary>
            /// Returns a random integer of type Int8 (byte) that is within a specified range.
            /// </summary>
            /// <param name="min">Minimum possible value</param>
            /// <param name="max">Maximum possible value</param>
            /// <returns>Random integer of type Int8 (byte)</returns>
            public byte NextByte(byte min, byte max)
            {
                return (byte)Remap(UInt64ToInt16(PcgXshRS()), 0, byte.MaxValue, min, max);
            }

            #endregion

            #region Int16 Next methods

            /// <summary>
            /// Returns a non-negative random integer of type Int16 (short).
            /// </summary>
            /// <returns>Random integer of type Int16 (short)</returns>
            public short NextShort()
            {
                return (short)Remap(UInt64ToInt16(PcgXshRS()), short.MinValue, short.MaxValue, 0, short.MaxValue);
            }

            /// <summary>
            /// Returns a non-negative random integer of type Int16 (short) that is less than the specified maximum.
            /// </summary>
            /// <param name="max">Maximum possible value</param>
            /// <returns>Random integer of type Int16 (short)</returns>
            public short NextShort(short max)
            {
                return (short)Remap(UInt64ToInt16(PcgXshRS()), short.MinValue, short.MaxValue, 0, max);
            }

            /// <summary>
            /// Returns a random integer of type Int16 (short) that is within a specified range.
            /// </summary>
            /// <param name="min">Minimum possible value</param>
            /// <param name="max">Maximum possible value</param>
            /// <returns>Random integer of type Int16 (short)</returns>
            public short NextShort(short min, short max)
            {
                return (short)Remap(UInt64ToInt16(PcgXshRS()), short.MinValue, short.MaxValue, min, max);
            }

            #endregion

            #region Int32 Next methods

            /// <summary>
            /// Returns a non-negative random integer of type Int32 (int).
            /// </summary>
            /// <returns>Random integer of type Int32 (int)</returns>
            public int NextInt()
            {
                return (int)Remap(UInt64ToInt32(PcgXshRS()), int.MinValue, int.MaxValue, 0, int.MaxValue);
            }

            /// <summary>
            /// Returns a non-negative random integer of type Int32 (int) that is less than the specified maximum.
            /// </summary>
            /// <param name="max">Maximum possible value</param>
            /// <returns>Random integer of type Int32 (int)</returns>
            public int NextInt(int max)
            {
                return (int)Remap(UInt64ToInt32(PcgXshRS()), int.MinValue, int.MaxValue, 0, max);
            }

            /// <summary>
            /// Returns a random integer of type Int32 (int) that is within a specified range.
            /// </summary>
            /// <param name="min">Minimum possible value</param>
            /// <param name="max">Maximum possible value</param>
            /// <returns>Random integer of type Int32 (int)</returns>
            public int NextInt(int min, int max)
            {
                return (int)Remap(UInt64ToInt32(PcgXshRS()), int.MinValue, int.MaxValue, min, max);
            }

            /// <summary>
            /// Alias for NextInt().
            /// Returns a non-negative random integer of type Int32 (int).
            /// </summary>
            /// <returns>Random integer of type Int32 (int)</returns>
            public int Next()
            {
                return (int)Remap(UInt64ToInt32(PcgXshRS()), int.MinValue, int.MaxValue, 0, int.MaxValue);
            }

            /// <summary>
            /// Alias for NextInt(int max).
            /// Returns a non-negative random integer of type Int32 (int) that is less than the specified maximum.
            /// </summary>
            /// <param name="max">Maximum possible value</param>
            /// <returns>Random integer of type Int32 (int)</returns>
            public int Next(int max)
            {
                return (int)Remap(UInt64ToInt32(PcgXshRS()), int.MinValue, int.MaxValue, 0, max);
            }

            /// <summary>
            /// Alias for NextInt(int min, int max).
            /// Returns a random integer of type Int32 (int) that is within a specified range.
            /// </summary>
            /// <param name="min">Minimum possible value</param>
            /// <param name="max">Maximum possible value</param>
            /// <returns>Random integer of type Int32 (int)</returns>
            public int Next(int min, int max)
            {
                return (int)Remap(UInt64ToInt32(PcgXshRS()), int.MinValue, int.MaxValue, min, max);
            }

            #endregion

            #region Int64 Next methods

            /// <summary>
            /// Returns a non-negative random integer of type Int64.
            /// </summary>
            /// <returns>Random integer of type Int64</returns>
            public long NextLong()
            {
                return Remap(UInt64ToInt64(PcgXshRS()), long.MinValue, long.MaxValue, 0, long.MaxValue);
            }

            /// <summary>
            /// Returns a non-negative random integer of type Int64 that is less than the specified maximum.
            /// </summary>
            /// <param name="max">Maximum possible value</param>
            /// <returns>Random integer of type Int64</returns>
            public long NextLong(long max)
            {
                return Remap(UInt64ToInt64(PcgXshRS()), long.MinValue, long.MaxValue, 0, max);
            }

            /// <summary>
            /// Returns a random integer of type Int64 that is within a specified range.
            /// </summary>
            /// <param name="min">Minimum possible value</param>
            /// <param name="max">Maximum possible value</param>
            /// <returns>Random integer of type Int64</returns>
            public long NextLong(long min, long max)
            {
                return Remap(UInt64ToInt64(PcgXshRS()), long.MinValue, long.MaxValue, min, max);
            }

            #endregion

            #endregion

            #region Floating Point Next methods

            #region Double Next methods

            /// <summary>
            /// Returns a non-negative random double-precision floating point number in the range of 0 to long.MaxValue.
            /// </summary>
            /// <returns>Random double-precision floating point number</returns>
            public double NextDouble()
            {
                return Remap(UInt64ToDouble(PcgXshRS()), Int64ToDouble(long.MinValue), Int64ToDouble(long.MaxValue), 0, Int64ToDouble(long.MaxValue));
            }

            /// <summary>
            /// Returns a non-negative random double-precision floating point number that is less than the specified maximum.
            /// </summary>
            /// <param name="max">Maximum possible value</param>
            /// <returns>Random double-precision floating point number</returns>
            public double NextDouble(double max)
            {
                return Remap(UInt64ToDouble(PcgXshRS()), Int64ToDouble(long.MinValue), Int64ToDouble(long.MaxValue), 0, max);
            }

            /// <summary>
            /// Returns a random double-precision floating point number that is within a specified range.
            /// </summary>
            /// <param name="min">Minimum possible value</param>
            /// <param name="max">Maximum possible value</param>
            /// <returns>Random double-precision floating point number</returns>
            public double NextDouble(double min, double max)
            {
                return Remap(UInt64ToDouble(PcgXshRS()), Int64ToDouble(long.MinValue), Int64ToDouble(long.MaxValue), min, max);
            }

            #endregion

            #region Float Next methods

            /// <summary>
            /// Returns a non-negative random single-precision floating point number in the range of 0 to long.MaxValue.
            /// </summary>
            /// <returns>Random single-precision floating point number</returns>
            public float NextFloat()
            {
                return (float)NextDouble();
            }

            /// <summary>
            /// Returns a non-negative random single-precision floating point number that is less than the specified maximum.
            /// </summary>
            /// <param name="max">Maximum possible value</param>
            /// <returns>Random single-precision floating point number</returns>
            public float NextFloat(float max)
            {
                return (float)NextDouble((double)max);
            }

            /// <summary>
            /// Returns a random single-precision floating point number that is within a specified range.
            /// </summary>
            /// <param name="min">Minimum possible value</param>
            /// <param name="max">Maximum possible value</param>
            /// <returns>Random single-precision floating point number</returns>
            public float NextFloat(float min, float max)
            {
                return (float)NextDouble((double)min, (double)max);
            }

            #endregion

            #endregion

            #endregion

            #region Private methods

            private uint PcgXshRS()
            {
                _state = unchecked(_state * 6364136223846793005UL + _seq);

                return (uint)(_state ^ (_state >> 22)) >> (22 + (int)(_state >> 61));
            }

            private long Remap(long value, long oldMin, long oldMax, long newMin, long newMax)
            {
                return (((value - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin;
            }

            private double Remap(double value, double oldMin, double oldMax, double newMin, double newMax) 
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

            private double UInt64ToDouble(ulong uint64)
            {
                return Int64ToDouble(UInt64ToInt64(uint64));
            }

            private double Int64ToDouble(long int64)
            {
                return Convert.ToDouble(int64);
            }

            #endregion

            #endregion
        }
    }
}
