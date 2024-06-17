// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.InteractiveTests.TestUI
{
    ///<summary>Helps maintain <see cref="View"/> drawing context.</summary>
    public class DrawContext : IDisposable
    {
        private readonly Stack<Matrix> _states;

        public DrawContext(GraphicsDevice graphicsDevice, Matrix matrix)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(graphicsDevice);
            _matrix = matrix;
            _states = new Stack<Matrix>();
        }

        public DrawContext(GraphicsDevice graphicsDevice)
            : this(graphicsDevice, Matrix.Identity)
        {
        }

        ~DrawContext()
        {
            Dispose(false);
        }

        private readonly GraphicsDevice _graphicsDevice;
        public GraphicsDevice GraphicsDevice { get { return _graphicsDevice; } }

        private Matrix _matrix;
        public Matrix Matrix { get { return _matrix; } }

        private readonly SpriteBatch _spriteBatch;
        public SpriteBatch SpriteBatch { get { return _spriteBatch; } }

        public void Begin(Matrix matrix)
        {
            if (_states.Count > 0)
                _spriteBatch.End();

            _states.Push(_matrix);
            _matrix = matrix;
            _spriteBatch.Begin(
                SpriteSortMode.Immediate, BlendState.AlphaBlend,
                null, null, null, null, _matrix);
        }

        public void End()
        {
            _spriteBatch.End();
            _matrix = _states.Pop();

            if (_states.Count > 0)
                _spriteBatch.Begin
                (SpriteSortMode.Immediate, BlendState.AlphaBlend,
                    null, null, null, null, _matrix);
        }

#region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) { _spriteBatch.Dispose(); }
        }

#endregion
    }
}
