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
#elif PSS
using Sce.Pss.Core.Graphics;
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

        static internal readonly byte[] Bytecode = LoadEffectResource(
#if DIRECTX
            "Microsoft.Xna.Framework.Graphics.Effect.Resources.SpriteEffect.dx11.mgfxo"
#elif PSS
            "MonoGame.Framework.PSSuite.PSSuite.Graphics.Resources.Texture.cgx" //FIXME: This shader is totally incomplete
#else
            "Microsoft.Xna.Framework.Graphics.Effect.Resources.SpriteEffect.ogl.mgfxo"
#endif
        );

        #region Methods

        /// <summary>
        /// Creates a new SpriteEffect.
        /// </summary>
        public SpriteEffect(GraphicsDevice device)
            : base(device, Bytecode)
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

        /// <summary>
        /// Lazily computes derived parameter values immediately before applying the effect.
        /// </summary>
        protected internal override bool OnApply()
        {
            var viewport = GraphicsDevice.Viewport;

            var projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            var halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);

            matrixParam.SetValue(halfPixelOffset * projection);

            return false;
        }


        #endregion
    }
}
