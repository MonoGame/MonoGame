// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// The default effect used by SpriteBatch.
    /// </summary>
    public class SpriteEffect : Effect
    {
        private EffectParameter _matrixParam;
        private Viewport _lastViewport;
        private Matrix _projection;

        /// <summary>
        /// Creates a new SpriteEffect.
        /// </summary>
        public SpriteEffect(GraphicsDevice device)
            : base(device, EffectResource.SpriteEffect.Bytecode)
        {
            CacheEffectParameters();
        }

        /// <summary>
        /// An optional matrix used to transform the sprite geometry. Uses <see cref="Matrix.Identity"/> if null.
        /// </summary>
        public Matrix? TransformMatrix { get; set; }

        /// <summary>
        /// Creates a new SpriteEffect by cloning parameter settings from an existing instance.
        /// </summary>
        protected SpriteEffect(SpriteEffect cloneSource)
            : base(cloneSource)
        {
            CacheEffectParameters();
        }


        /// <summary>
        /// Creates a clone of the current SpriteEffect instance.
        /// </summary>
        public override Effect Clone()
        {
            return new SpriteEffect(this);
        }


        /// <summary>
        /// Looks up shortcut references to our effect parameters.
        /// </summary>
        void CacheEffectParameters()
        {
            _matrixParam = Parameters["MatrixTransform"];
        }

        /// <summary>
        /// Lazily computes derived parameter values immediately before applying the effect.
        /// </summary>
        protected internal override void OnApply()
        {
            var vp = GraphicsDevice.Viewport;
            if ((vp.Width != _lastViewport.Width) || (vp.Height != _lastViewport.Height))
            {
                // Normal 3D cameras look into the -z direction (z = 1 is in front of z = 0). The
                // sprite batch layer depth is the opposite (z = 0 is in front of z = 1).
                // --> We get the correct matrix with near plane 0 and far plane -1.
                Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, -1, out _projection);

                if (GraphicsDevice.UseHalfPixelOffset)
                {
                    _projection.M41 += -0.5f * _projection.M11;
                    _projection.M42 += -0.5f * _projection.M22;
                }

                _lastViewport = vp;
            }

            if (TransformMatrix.HasValue)
                _matrixParam.SetValue(TransformMatrix.GetValueOrDefault() * _projection);
            else
                _matrixParam.SetValue(_projection);
        }
    }
}
