#region File Description
//-----------------------------------------------------------------------------
// SpriteEffect.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;




using System;

#if ANDROID || IPHONE
using OpenTK.Graphics.ES20;
using ActiveUniformType = OpenTK.Graphics.ES20.All;
#elif MONOMAC
using MonoMac.OpenGL;
#elif !WINRT
using OpenTK.Graphics.OpenGL;
#endif

#endregion

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// The default effect used by SpriteBatch.
    /// </summary>
    public class SpriteEffect : Effect
    {
        #region Effect Parameters

        EffectParameter matrixParam;

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new SpriteEffect.
        /// </summary>
        public SpriteEffect(GraphicsDevice device)
            : base(device, Effect.LoadEffectResource("SpriteEffect"))
        {
            CacheEffectParameters();
        }

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
            matrixParam = Parameters["MatrixTransform"];
        }

        internal override void Initialize()
        {
#if !WINRT
            matrixParam = new EffectParameter(ActiveUniformType.FloatMat4, "MatrixTransform");
            Parameters.Add(matrixParam);
#endif
        }

        /// <summary>
        /// Lazily computes derived parameter values immediately before applying the effect.
        /// </summary>
        protected internal override void OnApply()
        {
            Viewport viewport = GraphicsDevice.Viewport;

            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);

            matrixParam.SetValue(halfPixelOffset * projection);
        }


        #endregion
    }
}
