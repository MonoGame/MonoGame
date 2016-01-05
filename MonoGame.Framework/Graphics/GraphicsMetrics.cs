﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// A snapshot of rendering statistics from <see cref="GraphicsDevice.Metrics"/> to be used for runtime debugging and profiling.
    /// </summary>
    public struct GraphicsMetrics
    {
        internal ulong _clearCount;
        internal ulong _drawCount;
        internal ulong _pixelShaderCount;
        internal ulong _primitiveCount;
        internal ulong _spriteCount;
        internal ulong _targetCount;
        internal ulong _textureCount;
        internal ulong _vertexShaderCount;

        /// <summary>
        /// Number of times Clear was called.
        /// </summary>
        public ulong ClearCount { get { return _clearCount; } }

        /// <summary>
        /// Number of times Draw was called.
        /// </summary>
        public ulong DrawCount { get { return _drawCount; } }

        /// <summary>
        /// Number of times the pixel shader was changed on the GPU.
        /// </summary>
        public ulong PixelShaderCount { get { return _pixelShaderCount; } }

        /// <summary>
        /// Number of rendered primitives.
        /// </summary>
        public ulong PrimitiveCount { get { return _primitiveCount; } }

        /// <summary>
        /// Number of sprites and text characters rendered via <see cref="SpriteBatch"/>.
        /// </summary>
        public ulong SpriteCount { get { return _spriteCount; } }

        /// <summary>
        /// Number of times a target was changed on the GPU.
        /// </summary>
        public ulong TargetCount {get { return _targetCount; } }

        /// <summary>
        /// Number of times a texture was changed on the GPU.
        /// </summary>
        public ulong TextureCount { get { return _textureCount; } }

        /// <summary>
        /// Number of times the vertex shader was changed on the GPU.
        /// </summary>
        public ulong VertexShaderCount { get { return _vertexShaderCount; } }

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
                _clearCount = value1._clearCount - value2._clearCount,
                _drawCount = value1._drawCount - value2._drawCount,
                _pixelShaderCount = value1._pixelShaderCount - value2._pixelShaderCount,
                _primitiveCount = value1._primitiveCount - value2._primitiveCount,
                _spriteCount = value1._spriteCount - value2._spriteCount,
                _targetCount = value1._targetCount - value2._targetCount,
                _textureCount = value1._textureCount - value2._textureCount,
                _vertexShaderCount = value1._vertexShaderCount - value2._vertexShaderCount
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
                _clearCount =  value1._clearCount + value2._clearCount,
                _drawCount = value1._drawCount + value2._drawCount,
                _pixelShaderCount = value1._pixelShaderCount + value2._pixelShaderCount,
                _primitiveCount = value1._primitiveCount + value2._primitiveCount,
                _spriteCount = value1._spriteCount + value2._spriteCount,
                _targetCount = value1._targetCount + value2._targetCount,
                _textureCount = value1._textureCount + value2._textureCount,
                _vertexShaderCount = value1._vertexShaderCount + value2._vertexShaderCount
            };
        }
    }
}
