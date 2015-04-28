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
    public struct GraphicsMetrics
    {
        internal ulong _spriteCount;
        internal ulong _drawCount;
        internal ulong _primitivesCount;

        /// <summary>
        /// The count of sprites and text characters rendered via <see cref="SpriteBatch"/>.
        /// </summary>
        public ulong SpriteCount { get { return _spriteCount; } }

        /// <summary>
        /// The count of draw calls.
        /// </summary>
        public ulong DrawCount { get { return _drawCount; } }

        /// <summary>
        /// The count of rendered primitives.
        /// </summary>
        public ulong PrimitivesCount { get { return _primitivesCount; } }

        /// <summary>
        /// Subtracts a <see cref="GraphicsMetrics"/> from a <see cref="GraphicsMetrics"/>.
        /// </summary>
        /// <param name="value1">Source <see cref="GraphicsMetrics"/> on the left of the sub sign.</param>
        /// <param name="value2">Source <see cref="GraphicsMetrics"/> on the right of the sub sign.</param>
        /// <returns>Result of the metrics subtraction.</returns>
        public static GraphicsMetrics operator -(GraphicsMetrics value1, GraphicsMetrics value2)
        {
            return new GraphicsMetrics()
            {
                _spriteCount = value1._spriteCount - value2._spriteCount,
                _drawCount = value1._drawCount - value2._drawCount,
                _primitivesCount = value1._primitivesCount - value2._primitivesCount
            };
        }

        /// <summary>
        /// Adds two metrics.
        /// </summary>
        /// <param name="value1">Source <see cref="GraphicsMetrics"/> on the left of the add sign.</param>
        /// <param name="value2">Source <see cref="GraphicsMetrics"/> on the right of the add sign.</param>
        /// <returns>Sum of the metrics.</returns>
        public static GraphicsMetrics operator +(GraphicsMetrics value1, GraphicsMetrics value2)
        {
            return new GraphicsMetrics()
            {
                _spriteCount = value1._spriteCount + value2._spriteCount,
                _drawCount = value1._drawCount + value2._drawCount,
                _primitivesCount = value1._primitivesCount + value2._primitivesCount
            };
        }
    }
}
