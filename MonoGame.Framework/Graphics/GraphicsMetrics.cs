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
        internal ulong _pixelShaderCount;
        internal ulong _vertexShaderCount;
        internal ulong _textureCount;
        internal ulong _clearCount;

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
        /// The count of pixel shader switches
        /// </summary>
        public ulong PixelShaderCount { get { return _pixelShaderCount; } }

        /// <summary>
        /// The count of vertex shader switches
        /// </summary>
        public ulong VertexShaderCount { get { return _vertexShaderCount; } }

        /// <summary>
        /// The count of texture switches
        /// </summary>
        public ulong TextureCount { get { return _textureCount; } }

        /// <summary>
        /// The count of Clear calls
        /// </summary>
        public ulong ClearCount { get { return _clearCount; } }

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
                _primitiveCount = value1._primitiveCount - value2._primitiveCount,
                _pixelShaderCount = value1._pixelShaderCount - value2._pixelShaderCount,
                _vertexShaderCount = value1._vertexShaderCount - value2._vertexShaderCount,
                _textureCount = value1._textureCount - value2._textureCount,
                _clearCount = value1._clearCount - value2._clearCount
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
                _primitiveCount = value1._primitiveCount + value2._primitiveCount,
                _pixelShaderCount = value1._pixelShaderCount + value2._pixelShaderCount,
                _vertexShaderCount = value1._vertexShaderCount + value2._vertexShaderCount,
                _textureCount = value1._textureCount + value2._textureCount,
                _clearCount = value1._clearCount + value2._clearCount
            };
        }
    }
}
