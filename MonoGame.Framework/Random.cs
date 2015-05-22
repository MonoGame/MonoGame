using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework
{
    public static partial class MathHelper
    {
        /// <summary>
        /// Class for generating random numbers consistently when using Mono or Microsoft compilers.
        /// </summary>
        public class Random
        {
            #region Static methods

            /// <summary>
            /// Utility method for generating a seed for the random number generator.
            /// </summary>
            [CLSCompliant(false)]
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

            private const float UINT32_TO_FLOAT = 1.0f / ((uint.MaxValue >> 8) + 1);
            private const double UINT64_TO_DOUBLE = 1.0 / ((ulong.MaxValue >> 11) + 1);

            #endregion

            #region Properties

            /// <summary>
            /// Original seed for the random number generator. Assigning to this will 
            /// reset the RNG to this state as well as assigning the seed.
            /// </summary>
            [CLSCompliant(false)]
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
            [CLSCompliant(false)]
            public ulong State
            {
                get
                {
                    return this._state;
                }
            }

            /// <summary>
            /// Non-negative number identifying the sequence of random numbers produced by the generator.
            /// </summary>
            /// <devdoc>The sequence number determines the increment of the internal LCG. Only uneven,
            /// unsigned values are valid. Allowing any positive long value is more intuitive.</devdoc>
            public long Sequence
            {
                get
                {
                    return (long)(_seq >> 1);
                }
                set
                {
                    if (value < 0)
                        throw new ArgumentOutOfRangeException();
                    this._seq = ((ulong)value << 1) | 1;
                }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the random number generator using the value of 
            /// Random.GenerateSeed() as a default seed value and the default sequence.
            /// </summary>
            /// <devdoc>The default sequence is derived from Knuth's LCG for MMIX. When fed into the
            /// Sequence property, it produces an increment of 1442695040888963407.</devdoc>
            public Random()
                : this(GenerateSeed(), 721347520444481703)
            {
            }

            /// <summary>
            /// Initializes a new instance of the random number generator using the value of
            /// Random.GenerateSeed() as a default seed value and the specified sequence.
            /// </summary>
            /// <param name="sequence">Non-negative number identifying the sequence of random numbers.</param>
            public Random(long sequence)
                : this(GenerateSeed(), sequence)
            {
            }

            /// <summary>
            /// Initializes a new instance of the random number generator using the specified seed and sequence.
            /// </summary>
            /// <param name="seed">Seed to be used when computing new random numbers</param>
            /// <param name="sequence">Non-negative number identifying the sequence of random numbers.</param>
            [CLSCompliant(false)]
            public Random(ulong seed, long sequence)
            {
                this.Seed = seed;
                this.Sequence = sequence;
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
                // Shift to upper 8 bits, then cast (equivalent to multiplication + division)
                return (byte)(PcgXshRS() >> 24);
            }

            /// <summary>
            /// Returns a non-negative random integer of type Int8 (byte) that is less than the specified maximum.
            /// </summary>
            /// <param name="max">Exclusive maximum value.</param>
            /// <returns>Random integer of type Int8 (byte)</returns>
            public byte NextByte(byte max)
            {
                return NextByte(0, max);
            }

            /// <summary>
            /// Returns a random integer of type Int8 (byte) that is within a specified range.
            /// </summary>
            /// <param name="min">Inclusive minimum value.</param>
            /// <param name="max">Exclusive maximum value.</param>
            /// <returns>Random integer of type Int8 (byte)</returns>
            public byte NextByte(byte min, byte max)
            {
                uint range = (uint)(max - min);
                return (byte)((PcgXshRS() % range) + min);
            }

            #endregion

            #region Int16 Next methods

            /// <summary>
            /// Returns a non-negative random integer of type Int16 (short).
            /// </summary>
            /// <returns>Random integer of type Int16 (short)</returns>
            public short NextShort()
            {
                // Shift to upper 15 bits for positive values only, then cast
                return (short)(PcgXshRS() >> 17);
            }

            /// <summary>
            /// Returns a non-negative random integer of type Int16 (short) that is less than the specified maximum.
            /// </summary>
            /// <param name="max">Exclusive maximum value.</param>
            /// <returns>Random integer of type Int16 (short)</returns>
            public short NextShort(short max)
            {
                return NextShort(0, max);
            }

            /// <summary>
            /// Returns a random integer of type Int16 (short) that is within a specified range.
            /// </summary>
            /// <param name="min">Inclusive minimum value.</param>
            /// <param name="max">Exclusive maximum value.</param>
            /// <returns>Random integer of type Int16 (short)</returns>
            public short NextShort(short min, short max)
            {
                // Cast to int, in case the total range is bigger than short.MaxValue
                int range = (int)max - (int)min;

                return (short)((PcgXshRS() % range) + min);
            }

            #endregion

            #region Int32 Next methods

            /// <summary>
            /// Returns a non-negative random integer of type Int32 (int).
            /// </summary>
            /// <returns>Random integer of type Int32 (int)</returns>
            public int Next()
            {
                // Shift to remove sign bit, then cast
                return (int)(PcgXshRS() >> 1);
            }

            /// <summary>
            /// Returns a non-negative random integer of type Int32 (int) that is less than the specified maximum.
            /// </summary>
            /// <param name="max">Exclusice maximum value.</param>
            /// <returns>Random integer of type Int32 (int)</returns>
            public int Next(int max)
            {
                return Next(0, max);
            }

            /// <summary>
            /// Returns a random integer of type Int32 (int) that is within a specified range.
            /// </summary>
            /// <param name="min">Inclusive minimum value.</param>
            /// <param name="max">Exclusive maximum value.</param>
            /// <returns>Random integer of type Int32 (int)</returns>
            public int Next(int min, int max)
            {
                // Cast to uint in case the total range is greater than int.MaxValue
                // Force overflow on negative numbers, so they remain less than positive ones
                uint _min = (uint)min + 0x80000000;
                uint _max = (uint)max + 0x80000000;
                uint range = _max - _min;

                return (int)(PcgXshRS() % range) + min;
            }

            #endregion

            #region Int64 Next methods

            /// <summary>
            /// Returns a non-negative random integer of type Int64.
            /// </summary>
            /// <returns>Random integer of type Int64</returns>
            public long NextLong()
            {
                long lo = (long)PcgXshRS();
                // Remove 1 bit, only 63 needed for positive values
                long hi = (long)(PcgXshRS() >> 1);

                return lo | (hi << 32);
            }

            /// <summary>
            /// Returns a non-negative random integer of type Int64 that is less than the specified maximum.
            /// </summary>
            /// <param name="max">Exclusive maximum possible value.</param>
            /// <returns>Random integer of type Int64</returns>
            public long NextLong(long max)
            {
                return NextLong(0, max);
            }

            /// <summary>
            /// Returns a random integer of type Int64 that is within a specified range.
            /// </summary>
            /// <param name="min">Inclusive minimum value.</param>
            /// <param name="max">Exclusive maximum value.</param>
            /// <returns>Random integer of type Int64</returns>
            public long NextLong(long min, long max)
            {
                // Cast to ulong in case the total range is greater than long.MaxValue
                // Force overflow on negative numbers, so they remain less than positive ones
                ulong _min = (ulong)min + 0x8000000000000000;
                ulong _max = (ulong)max + 0x8000000000000000;
                ulong range = _max - _min;

                ulong value = (ulong)PcgXshRS() | ((ulong)PcgXshRS() << 32);

                return (long)(value % range) + min;
            }

            #endregion

            #endregion

            #region Floating Point Next methods

            #region Double Next methods

            /// <summary>
            /// Returns a non-negative random double-precision floating point number in the range of 0 to 1.0.
            /// </summary>
            /// <returns>Random double-precision floating point number</returns>
            public double NextDouble()
            {
                ulong value = (ulong)PcgXshRS() | ((ulong)PcgXshRS() << 32);
                return (value >> 11) * UINT64_TO_DOUBLE;
            }

            /// <summary>
            /// Returns a non-negative random double-precision floating point number that is less than the specified maximum.
            /// </summary>
            /// <param name="max">Maximum possible value.</param>
            /// <returns>Random double-precision floating point number</returns>
            public double NextDouble(double max)
            {
                return NextDouble() * max;
            }

            /// <summary>
            /// Returns a random double-precision floating point number that is within a specified range.
            /// </summary>
            /// <param name="min">Minimum possible value.</param>
            /// <param name="max">Maximum possible value.</param>
            /// <returns>Random double-precision floating point number</returns>
            public double NextDouble(double min, double max)
            {
                return NextDouble() * (max - min) + min;
            }

            #endregion

            #region Float Next methods

            /// <summary>
            /// Returns a non-negative random single-precision floating point number in the range of 0 to 1.0.
            /// </summary>
            /// <returns>Random single-precision floating point number</returns>
            public float NextFloat()
            {
                return (PcgXshRS() >> 8) * UINT32_TO_FLOAT;
            }

            /// <summary>
            /// Returns a non-negative random single-precision floating point number that is less than the specified maximum.
            /// </summary>
            /// <param name="max">Maximum possible value.</param>
            /// <returns>Random single-precision floating point number</returns>
            public float NextFloat(float max)
            {
                return NextFloat() * max;
            }

            /// <summary>
            /// Returns a random single-precision floating point number that is within a specified range.
            /// </summary>
            /// <param name="min">Minimum possible value.</param>
            /// <param name="max">Maximum possible value.</param>
            /// <returns>Random single-precision floating point number</returns>
            public float NextFloat(float min, float max)
            {
                return NextFloat() * (max - min) + min;
            }

            #endregion

            #endregion

            #endregion

            #region Private methods

            private uint PcgXshRS()
            {
                _state = unchecked(_state * 6364136223846793005UL + _seq);

                return (uint)((_state ^ (_state >> 22)) >> (22 + (int)(_state >> 61)));
            }

            #endregion
        }
    }
}
