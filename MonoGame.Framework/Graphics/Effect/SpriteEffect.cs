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
using OpenTK.Graphics.OpenGL;
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

#if NOMOJO

        public static readonly string[] vertexShaderFilenames = new string[] 
		{
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.SpriteEffect.VSSprite.glsl"
		};

        public static readonly string[] fragmentShaderFilenames = new string[]
		{
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.SpriteEffect.PSSprite.glsl"
		};

        static readonly int[] vertexShaderIndices = new int[] { 0 };
        static readonly int[] fragmentShaderIndices = new int[] { 0 };

        public static readonly Tuple<int, int>[] programIndices = new Tuple<int, int>[]
		{
			new Tuple<int, int>(0, 0)
		};


        public SpriteEffect(GraphicsDevice device)
            : base(device,
                SpriteEffect.vertexShaderFilenames,
                SpriteEffect.fragmentShaderFilenames,
                SpriteEffect.programIndices)
        {
            Initialize();

            CacheEffectParameters();

            Techniques.Add(new EffectTechnique(this));
        }
#else
        /// <summary>
        /// Creates a new SpriteEffect.
        /// </summary>
        public SpriteEffect(GraphicsDevice device)
            : base(device, Effect.LoadEffectResource("SpriteEffect"))
        {
            CacheEffectParameters();
        }
#endif






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
            matrixParam = new EffectParameter(ActiveUniformType.FloatMat4, "MatrixTransform");
            Parameters.Add(matrixParam);
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
