namespace Microsoft.Xna.Framework;

/// <inheritdoc />
public static partial class MathHelper
{
    /// <summary>
    /// Random Number Generator based on Permutation-Congruential-Generator (PCG), which is a fancy wording to
    /// describe a family of RNG which are simple, fast, statistically excellent, and hardly predictable.
    /// 
    /// More interestingly, PCG allows to generate multiple sequences with the same seed, which is very handy
    /// in game development to have a unique seed per game session while using different streams for each
    /// RNG which requires an isolated context (e.g. generating a procedural level, but we don't want the loot
    /// generation to affect subsequent level generations).
    /// 
    /// https://www.pcg-random.org/
    /// 
    /// This code is derived from the minimal C implementation.
    /// https://github.com/imneme/pcg-c-basic
    /// </summary>
    public class Random
    {
        // State
        private ulong _state;
        private readonly ulong _inc;

        /// <summary>
        /// State of the random number generator.
        /// </summary>
        public ulong State
        {
            get => _state;
            set
            {
                _state = value;
                PCG32(); // Advance state to ensure we don't return the same number twice.
            }
        }

        /// <summary>
        /// Initialize a random number generator.
        /// </summary>
        public Random() : this((ulong)System.Environment.TickCount) { }

        /// <summary>
        /// Initialize a random number generator. Specified in two parts, state initializer (a.k.a. seed) and a sequence selection constant (a.k.a. stream id).
        /// </summary>
        /// <param name="state">State initializer (a.k.a. seed).</param>
        /// <param name="streamID">Sequence selection constant (a.k.a. stream id). Defaults to 0.</param>
        public Random(ulong state, ulong streamID = 0)
        {
            _state = 0ul;
            _inc = (streamID << 1) | 1ul;
            PCG32();
            _state += state;
            PCG32();
        }

        /// <summary>
        /// Generate a uniformly distributed number.
        /// </summary>
        /// <returns>A uniformly distributed 32bit unsigned integer.</returns>
        private uint PCG32()
        {
            ulong oldState = _state;
            // Advance internal state
            _state = unchecked(_state * 6364136223846793005ul + _inc);
            // Calculate output function (XSH RR), uses old state for max ILP
            uint xorshifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
            int rot = (int)(oldState >> 59);
            return (xorshifted >> rot) | (xorshifted << ((-rot) & 31));
        }

        /// <summary>
        /// Generate a uniformly distributed number, r, where 0 &lt;= r &lt; <paramref name="bound"/>.
        /// </summary>
        /// <param name="bound">Exclusive upper bound of the number to generate.</param>
        /// <returns>A uniformly distributed 32bit unsigned integer strictly less than <paramref name="bound"/>.</returns>
        private uint PCG32(uint bound)
        {
            if (bound <= 0) { bound = 1; }

            // To avoid bias, we need to make the range of the RNG a multiple of
            // bound, which we do by dropping output less than a threshold.
            uint threshold = ((uint)-bound) % bound;

            // Uniformity guarantees that this loop will terminate.  In practice, it
            // should usually terminate quickly; on average (assuming all bounds are
            // equally likely), 82.25% of the time, we can expect it to require just
            // one iteration.  In the worst case, someone passes a bound of 2^31 + 1
            // (i.e., 2147483649), which invalidates almost 50% of the range.  In 
            // practice, bounds are typically small and only a tiny amount of the range
            // is eliminated.
            while (true)
            {
                uint r = PCG32();
                if (r >= threshold)
                    return r % bound;
            }
        }

        /// <summary>
        /// Generate a random positive number.
        /// </summary>
        /// <returns>A random positive number.</returns>
        public int Next()
        {
            return Next(int.MaxValue);
        }

        /// <summary>
        /// Generate a random number with an exclusive <paramref name="maxValue"/> upper bound.
        /// </summary>
        /// <param name="maxValue">Exclusive upper bound.</param>
        /// <returns>A random number with an exclusive <paramref name="maxValue"/> upper bound.</returns>
        public int Next(int maxValue)
        {
            if (maxValue < 0)
                maxValue = 1;

            return (int)PCG32((uint)maxValue);
        }

        /// <summary>
        /// Generate a random number ranging from <paramref name="minValue"/> to <paramref name="maxValue"/>.
        /// </summary>
        /// <param name="minValue">Lower bound.</param>
        /// <param name="maxValue">Upper bound.</param>
        /// <returns>A random number ranging from <paramref name="minValue"/> to <paramref name="maxValue"/>.</returns>
        public int Next(int minValue, int maxValue)
        {
            if (maxValue < minValue)
                maxValue = minValue;

            return (int)(minValue + PCG32((uint)((long)maxValue - minValue)));
        }

        /// <summary>
        /// Generate a random float ranging from 0.0f to 1.0f.
        /// </summary>
        /// <returns>A random float ranging from 0.0f to 1.0f.</returns>
        public float NextFloat()
        {
            // This is quite hackish because we want to avoid BitConverter, but who cares?
            int bound = int.MaxValue / 2 - 1;
            return Next(bound) * 1.0f / bound;
        }

        /// <summary>
        /// Generate a random float ranging from <paramref name="minValue"/> to <paramref name="maxValue"/>.
        /// </summary>
        /// <param name="minValue">Lower bound. MUST be greater than zero.</param>
        /// <param name="maxValue">Upper bound. MUST be greater than zero.</param>
        /// <returns>A random float ranging from <paramref name="minValue"/> to <paramref name="maxValue"/>.</returns>
        public float NextFloat(float minValue, float maxValue)
        {
            if (minValue < 0)
                minValue = 0;

            if (maxValue < minValue)
                maxValue = minValue;

            return minValue + (maxValue - minValue) * NextFloat();
        }

        /// <summary>
        /// Generate a random bool.
        /// </summary>
        /// <returns>A random bool.</returns>
        public bool NextBool()
        {
            return NextFloat() <= 0.5f;
        }
    }
}
