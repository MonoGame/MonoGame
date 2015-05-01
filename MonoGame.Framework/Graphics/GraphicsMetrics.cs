// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// A snapshot of rendering statistics from <see cref="GraphicsDevice.Metrics"/> to be used for runtime debugging and profiling.
    /// </summary>
    public struct GraphicsMetrics
    {
        internal ulong _spriteCount;
        internal ulong _drawCount;
        internal ulong _primitiveCount;

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
        public ulong PrimitiveCount { get { return _primitiveCount; } }

        /// <summary>
        /// Returns the difference between two sets of metrics.
        /// </summary>
        /// <param name="value1">Source <see cref="GraphicsMetrics"/> on the left of the sub sign.</param>
        /// <param name="value2">Source <see cref="GraphicsMetrics"/> on the right of the sub sign.</param>
        /// <returns>Difference between two sets of metrics.</returns>
        public static GraphicsMetrics operator -(GraphicsMetrics value1, GraphicsMetrics value2)
        {
            return new GraphicsMetrics()
            {
                _spriteCount = value1._spriteCount - value2._spriteCount,
                _drawCount = value1._drawCount - value2._drawCount,
                _primitiveCount = value1._primitiveCount - value2._primitiveCount
            };
        }

        /// <summary>
        /// Returns the combination of two sets of metrics.
        /// </summary>
        /// <param name="value1">Source <see cref="GraphicsMetrics"/> on the left of the add sign.</param>
        /// <param name="value2">Source <see cref="GraphicsMetrics"/> on the right of the add sign.</param>
        /// <returns>Combination of two sets of metrics.</returns>
        public static GraphicsMetrics operator +(GraphicsMetrics value1, GraphicsMetrics value2)
        {
            return new GraphicsMetrics()
            {
                _spriteCount = value1._spriteCount + value2._spriteCount,
                _drawCount = value1._drawCount + value2._drawCount,
                _primitiveCount = value1._primitiveCount + value2._primitiveCount
            };
        }
    }
}
