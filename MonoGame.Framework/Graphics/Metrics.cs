// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// The rendering information from <see cref="GraphicsDevice"/>. 
    /// Useful for debugging and profiling purposes. 
    /// Resetted every frame after draw at <see cref="GraphicsDevice.Present"/>.
    /// </summary>
    public struct Metrics
    {
        internal ulong _batchCount;
        internal ulong _drawCount;
        internal ulong _primitivesCount;

        /// <summary>
        /// The count of batches sended to <see cref="SpriteBatch"/>.
        /// </summary>
        public ulong BatchCount { get { return _batchCount; } }

        /// <summary>
        /// The count of draw calls.
        /// </summary>
        public ulong DrawCount { get { return _drawCount; } }

        /// <summary>
        /// The count of rendered primitives.
        /// </summary>
        public ulong PrimitivesCount { get { return _primitivesCount; } }
        
        internal void Reset()
        {
            _batchCount = 0;
            _drawCount = 0;
            _primitivesCount = 0;
        }

        /// <summary>
        /// Subtracts a <see cref="Metrics"/> from a <see cref="Metrics"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="Metrics"/> on the left of the sub sign.</param>
        /// <param name="value2">Source <see cref="Metrics"/> on the right of the sub sign.</param>
        /// <returns>Result of the metrics subtraction.</returns>
        public static Metrics operator -(Metrics value1, Metrics value2)
        {
            return new Metrics()
            {
                _batchCount = value1._batchCount - value2._batchCount,
                _drawCount = value1._drawCount - value2._drawCount,
                _primitivesCount = value1._primitivesCount - value2._primitivesCount
            };
        }
    }
}
