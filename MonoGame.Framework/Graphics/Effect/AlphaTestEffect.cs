#region File Description
//-----------------------------------------------------------------------------
// AlphaTestEffect.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Built-in effect that supports alpha testing.
    /// </summary>
    public class AlphaTestEffect : Effect, IEffectMatrices, IEffectFog
    {
        #region Effect Parameters

        EffectParameter textureParam;
        EffectParameter diffuseColorParam;
        EffectParameter alphaTestParam;
        EffectParameter fogColorParam;
        EffectParameter fogVectorParam;
        EffectParameter worldViewProjParam;

        #endregion

        #region Fields

        bool fogEnabled;
        bool vertexColorEnabled;

        Matrix world = Matrix.Identity;
        Matrix view = Matrix.Identity;
        Matrix projection = Matrix.Identity;

        Matrix worldView;

        Vector3 diffuseColor = Vector3.One;

        float alpha = 1;

        float fogStart = 0;
        float fogEnd = 1;

        CompareFunction alphaFunction = CompareFunction.Greater;
        int referenceAlpha;
        bool isEqNe;

        EffectDirtyFlags dirtyFlags = EffectDirtyFlags.All;

        #endregion

        #region Public Properties


        /// <summary>
        /// Gets or sets the world matrix.
        /// </summary>
        public Matrix World
        {
            get { return world; }
            
            set
            {
                world = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the view matrix.
        /// </summary>
        public Matrix View
        {
            get { return view; }
            
            set
            {
                view = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        public Matrix Projection
        {
            get { return projection; }
            
            set
            {
                projection = value;
                dirtyFlags |= EffectDirtyFlags.WorldViewProj;
            }
        }


        /// <summary>
        /// Gets or sets the material diffuse color (range 0 to 1).
        /// </summary>
        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            
            set
            {
                diffuseColor = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }


        /// <summary>
        /// Gets or sets the material alpha.
        /// </summary>
        public float Alpha
        {
            get { return alpha; }
            
            set
            {
                alpha = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }


        /// <summary>
        /// Gets or sets the fog enable flag.
        /// </summary>
        public bool FogEnabled
        {
            get { return fogEnabled; }
            
            set
            {
                if (fogEnabled != value)
                {
                    fogEnabled = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex | EffectDirtyFlags.FogEnable;
                }
            }
        }


        /// <summary>
        /// Gets or sets the fog start distance.
        /// </summary>
        public float FogStart
        {
            get { return fogStart; }
            
            set
            {
                fogStart = value;
                dirtyFlags |= EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the fog end distance.
        /// </summary>
        public float FogEnd
        {
            get { return fogEnd; }
            
            set
            {
                fogEnd = value;
                dirtyFlags |= EffectDirtyFlags.Fog;
            }
        }


        /// <summary>
        /// Gets or sets the fog color.
        /// </summary>
        public Vector3 FogColor
        {
            get { return fogColorParam.GetValueVector3(); }
            set { fogColorParam.SetValue(value); }
        }


        /// <summary>
        /// Gets or sets the current texture.
        /// </summary>
        public Texture2D Texture
        {
            get { return textureParam.GetValueTexture2D(); }
            set { textureParam.SetValue(value); }
        }


        /// <summary>
        /// Gets or sets whether vertex color is enabled.
        /// </summary>
        public bool VertexColorEnabled
        {
            get { return vertexColorEnabled; }
            
            set
            {
                if (vertexColorEnabled != value)
                {
                    vertexColorEnabled = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
            }
        }


        /// <summary>
        /// Gets or sets the alpha compare function (default Greater).
        /// </summary>
        public CompareFunction AlphaFunction
        {
            get { return alphaFunction; }
            
            set
            {
                alphaFunction = value;
                dirtyFlags |= EffectDirtyFlags.AlphaTest;
            }
        }


        /// <summary>
        /// Gets or sets the reference alpha value (default 0).
        /// </summary>
        public int ReferenceAlpha
        {
            get { return referenceAlpha; }
            
            set
            {
                referenceAlpha = value;
                dirtyFlags |= EffectDirtyFlags.AlphaTest;
            }
        }


        #endregion

        #region Methods

        /// <summary>
        /// Creates a new AlphaTestEffect with default parameter settings.
        /// </summary>
        public AlphaTestEffect(GraphicsDevice device)
            : base(device, EffectResource.AlphaTestEffect.Bytecode)
        {
            CacheEffectParameters();
        }

        /// <summary>
        /// Creates a new AlphaTestEffect by cloning parameter settings from an existing instance.
        /// </summary>
        protected AlphaTestEffect(AlphaTestEffect cloneSource)
            : base(cloneSource)
        {
            CacheEffectParameters();

            fogEnabled = cloneSource.fogEnabled;
            vertexColorEnabled = cloneSource.vertexColorEnabled;

            world = cloneSource.world;
            view = cloneSource.view;
            projection = cloneSource.projection;

            diffuseColor = cloneSource.diffuseColor;

            alpha = cloneSource.alpha;

            fogStart = cloneSource.fogStart;
            fogEnd = cloneSource.fogEnd;
            
            alphaFunction = cloneSource.alphaFunction;
            referenceAlpha = cloneSource.referenceAlpha;

        }

        /// <summary>
        /// Creates a clone of the current AlphaTestEffect instance.
        /// </summary>
        public override Effect Clone()
        {
            return new AlphaTestEffect(this);
        }

        /// <summary>
        /// Looks up shortcut references to our effect parameters.
        /// </summary>
        void CacheEffectParameters()
        {
            textureParam        = Parameters["Texture"];
            diffuseColorParam   = Parameters["DiffuseColor"];
            alphaTestParam      = Parameters["AlphaTest"];
            fogColorParam       = Parameters["FogColor"];
            fogVectorParam      = Parameters["FogVector"];
            worldViewProjParam  = Parameters["WorldViewProj"];
        }
        
        /// <summary>
        /// Lazily computes derived parameter values immediately before applying the effect.
        /// </summary>
        protected internal override void OnApply()
        {
            // Recompute the world+view+projection matrix or fog vector?
            dirtyFlags = EffectHelpers.SetWorldViewProjAndFog(dirtyFlags, ref world, ref view, ref projection, ref worldView, fogEnabled, fogStart, fogEnd, worldViewProjParam, fogVectorParam);

            // Recompute the diffuse/alpha material color parameter?
            if ((dirtyFlags & EffectDirtyFlags.MaterialColor) != 0)
            {
                diffuseColorParam.SetValue(new Vector4(diffuseColor * alpha, alpha));

                dirtyFlags &= ~EffectDirtyFlags.MaterialColor;
            }

            // Recompute the alpha test settings?
            if ((dirtyFlags & EffectDirtyFlags.AlphaTest) != 0)
            {
                Vector4 alphaTest = new Vector4();
                bool eqNe = false;
                
                // Convert reference alpha from 8 bit integer to 0-1 float format.
                float reference = (float)referenceAlpha / 255f;
                
                // Comparison tolerance of half the 8 bit integer precision.
                const float threshold = 0.5f / 255f;
                
                switch (alphaFunction)
                {
                    case CompareFunction.Less:
                        // Shader will evaluate: clip((a < x) ? z : w)
                        alphaTest.X = reference - threshold;
                        alphaTest.Z = 1;
                        alphaTest.W = -1;
                        break;

                    case CompareFunction.LessEqual:
                        // Shader will evaluate: clip((a < x) ? z : w)
                        alphaTest.X = reference + threshold;
                        alphaTest.Z = 1;
                        alphaTest.W = -1;
                        break;

                    case CompareFunction.GreaterEqual:
                        // Shader will evaluate: clip((a < x) ? z : w)
                        alphaTest.X = reference - threshold;
                        alphaTest.Z = -1;
                        alphaTest.W = 1;
                        break;

                    case CompareFunction.Greater:
                        // Shader will evaluate: clip((a < x) ? z : w)
                        alphaTest.X = reference + threshold;
                        alphaTest.Z = -1;
                        alphaTest.W = 1;
                        break;

                    case CompareFunction.Equal:
                        // Shader will evaluate: clip((abs(a - x) < Y) ? z : w)
                        alphaTest.X = reference;
                        alphaTest.Y = threshold;
                        alphaTest.Z = 1;
                        alphaTest.W = -1;
                        eqNe = true;
                        break;

                    case CompareFunction.NotEqual:
                        // Shader will evaluate: clip((abs(a - x) < Y) ? z : w)
                        alphaTest.X = reference;
                        alphaTest.Y = threshold;
                        alphaTest.Z = -1;
                        alphaTest.W = 1;
                        eqNe = true;
                        break;

                    case CompareFunction.Never:
                        // Shader will evaluate: clip((a < x) ? z : w)
                        alphaTest.Z = -1;
                        alphaTest.W = -1;
                        break;

                    case CompareFunction.Always:
                    default:
                        // Shader will evaluate: clip((a < x) ? z : w)
                        alphaTest.Z = 1;
                        alphaTest.W = 1;
                        break;
                }
                
                alphaTestParam.SetValue(alphaTest);

                dirtyFlags &= ~EffectDirtyFlags.AlphaTest;
                
                // If we changed between less/greater vs. equal/notequal
                // compare modes, we must also update the shader index.
                if (isEqNe != eqNe)
                {
                    isEqNe = eqNe;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
            }

            // Recompute the shader index?
            if ((dirtyFlags & EffectDirtyFlags.ShaderIndex) != 0)
            {
                int shaderIndex = 0;
                
                if (!fogEnabled)
                    shaderIndex += 1;
                
                if (vertexColorEnabled)
                    shaderIndex += 2;
                
                if (isEqNe)
                    shaderIndex += 4;

                dirtyFlags &= ~EffectDirtyFlags.ShaderIndex;

                CurrentTechnique = Techniques[shaderIndex];
            }
        }


        #endregion
    }
}
