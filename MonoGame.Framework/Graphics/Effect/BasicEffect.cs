#region File Description
//-----------------------------------------------------------------------------
// BasicEffect.cs
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
    /// Built-in effect that supports optional texturing, vertex coloring, fog, and lighting.
    /// </summary>
    public class BasicEffect : Effect, IEffectMatrices, IEffectLights, IEffectFog
    {
        #region Effect Parameters

        EffectParameter textureParam;
        EffectParameter diffuseColorParam;
        EffectParameter emissiveColorParam;
        EffectParameter specularColorParam;
        EffectParameter specularPowerParam;
        EffectParameter eyePositionParam;
        EffectParameter fogColorParam;
        EffectParameter fogVectorParam;
        EffectParameter worldParam;
        EffectParameter worldInverseTransposeParam;
        EffectParameter worldViewProjParam;

        #endregion

        #region Fields

        bool lightingEnabled;
        bool preferPerPixelLighting;
        bool oneLight;
        bool fogEnabled;
        bool textureEnabled;
        bool vertexColorEnabled;

        Matrix world = Matrix.Identity;
        Matrix view = Matrix.Identity;
        Matrix projection = Matrix.Identity;

        Matrix worldView;

        Vector3 diffuseColor = Vector3.One;
        Vector3 emissiveColor = Vector3.Zero;
        Vector3 ambientLightColor = Vector3.Zero;

        float alpha = 1;

        DirectionalLight light0;
        DirectionalLight light1;
        DirectionalLight light2;

        float fogStart = 0;
        float fogEnd = 1;

        EffectDirtyFlags dirtyFlags = EffectDirtyFlags.All;

        #endregion

#if NOMOJO
        static readonly string[] vertexShaderFilenames = new string[] 
		{
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasic.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasicNoFog.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasicVc.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasicVcNoFog.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasicTx.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasicTxNoFog.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasicTxVc.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasicTxVcNoFog.glsl",
			
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasicVertexLighting.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasicVertexLightingVc.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasicVertexLightingTx.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasicVertexLightingTxVc.glsl",
			
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasicOneLight.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasicOneLightVc.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasicOneLightTx.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasicOneLightTxVc.glsl",
			
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasicPixelLighting.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasicPixelLightingVc.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasicPixelLightingTx.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.VSBasicPixelLightingTxVc.glsl",
		};

        static readonly string[] fragmentShaderFilenames = new string[]
		{
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.PSBasic.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.PSBasicNoFog.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.PSBasicTx.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.PSBasicTxNoFog.glsl",
			
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.PSBasicVertexLighting.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.PSBasicVertexLightingNoFog.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.PSBasicVertexLightingTx.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.PSBasicVertexLightingTxNoFog.glsl",
			
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.PSBasicPixelLighting.glsl",
			"Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.PSBasicPixelLightingTx.glsl",
		};

        static readonly Tuple<int, int>[] programIndices = new Tuple<int, int>[]
		{
			new Tuple<int, int>(0, 0),
			new Tuple<int, int>(1, 1),
			new Tuple<int, int>(2, 0),
			new Tuple<int, int>(3, 1),
			new Tuple<int, int>(4, 2),
			new Tuple<int, int>(5, 3),
			new Tuple<int, int>(6, 2),
			new Tuple<int, int>(7, 3),
			new Tuple<int, int>(8, 4),
			new Tuple<int, int>(8, 5),
			new Tuple<int, int>(9, 4),
			new Tuple<int, int>(9, 5),
			new Tuple<int, int>(10, 6),
			new Tuple<int, int>(10, 7),
			new Tuple<int, int>(11, 6),
			new Tuple<int, int>(11, 7),
			new Tuple<int, int>(12, 4),
			new Tuple<int, int>(12, 5),
			new Tuple<int, int>(13, 4),
			new Tuple<int, int>(13, 5),
			new Tuple<int, int>(14, 6),
			new Tuple<int, int>(14, 7),
			new Tuple<int, int>(15, 6),
			new Tuple<int, int>(15, 7),
			new Tuple<int, int>(16, 8),
			new Tuple<int, int>(16, 8),
			new Tuple<int, int>(17, 8),
			new Tuple<int, int>(17, 8),
			new Tuple<int, int>(18, 9),
			new Tuple<int, int>(18, 9),
			new Tuple<int, int>(19, 9),
			new Tuple<int, int>(19, 9),
		};
#endif

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
                dirtyFlags |= EffectDirtyFlags.World | EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
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
                dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.EyePosition | EffectDirtyFlags.Fog;
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
        /// Gets or sets the material emissive color (range 0 to 1).
        /// </summary>
        public Vector3 EmissiveColor
        {
            get { return emissiveColor; }
            
            set
            {
                emissiveColor = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }


        /// <summary>
        /// Gets or sets the material specular color (range 0 to 1).
        /// </summary>
        public Vector3 SpecularColor
        {
            get { return specularColorParam.GetValueVector3(); }
            set { specularColorParam.SetValue(value); }
        }


        /// <summary>
        /// Gets or sets the material specular power.
        /// </summary>
        public float SpecularPower
        {
            get { return specularPowerParam.GetValueSingle(); }
            set { specularPowerParam.SetValue(value); }
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
        /// Gets or sets the lighting enable flag.
        /// </summary>
        public bool LightingEnabled
        {
            get { return lightingEnabled; }
            
            set
            {
                if (lightingEnabled != value)
                {
                    lightingEnabled = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex | EffectDirtyFlags.MaterialColor;
                }
            }
        }


        /// <summary>
        /// Gets or sets the per-pixel lighting prefer flag.
        /// </summary>
        public bool PreferPerPixelLighting
        {
            get { return preferPerPixelLighting; }
            
            set
            {
                if (preferPerPixelLighting != value)
                {
                    preferPerPixelLighting = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
            }
        }


        /// <summary>
        /// Gets or sets the ambient light color (range 0 to 1).
        /// </summary>
        public Vector3 AmbientLightColor
        {
            get { return ambientLightColor; }
            
            set
            {
                ambientLightColor = value;
                dirtyFlags |= EffectDirtyFlags.MaterialColor;
            }
        }


        /// <summary>
        /// Gets the first directional light.
        /// </summary>
        public DirectionalLight DirectionalLight0 { get { return light0; } }


        /// <summary>
        /// Gets the second directional light.
        /// </summary>
        public DirectionalLight DirectionalLight1 { get { return light1; } }


        /// <summary>
        /// Gets the third directional light.
        /// </summary>
        public DirectionalLight DirectionalLight2 { get { return light2; } }


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
        /// Gets or sets whether texturing is enabled.
        /// </summary>
        public bool TextureEnabled
        {
            get { return textureEnabled; }
            
            set
            {
                if (textureEnabled != value)
                {
                    textureEnabled = value;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
            }
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


        #endregion

        #region Methods

#if NOMOJO
        public BasicEffect(GraphicsDevice device)
            : base(device,
                BasicEffect.vertexShaderFilenames,
                BasicEffect.fragmentShaderFilenames,
                BasicEffect.programIndices)
        {

            Initialize();

            CacheEffectParameters(null);

            Techniques.Add(new EffectTechnique(this));

            DirectionalLight0.Enabled = true;

            SpecularColor = Vector3.One;
            SpecularPower = 16;
        }
#else
        /// <summary>
        /// Creates a new BasicEffect with default parameter settings.
        /// </summary>
        public BasicEffect(GraphicsDevice device)
            : base(device, Effect.LoadEffectResource("BasicEffect"))
        {
            CacheEffectParameters(null);

            DirectionalLight0.Enabled = true;

            SpecularColor = Vector3.One;
            SpecularPower = 16;
        }
#endif


        internal override void Initialize()
        {
            textureParam = new EffectParameter(ActiveUniformType.Sampler2D, "Texture");
            Parameters.Add(textureParam);
            diffuseColorParam = new EffectParameter(ActiveUniformType.FloatVec4, "DiffuseColor");
            Parameters.Add(diffuseColorParam);
            emissiveColorParam = new EffectParameter(ActiveUniformType.FloatVec4, "EmissiveColor");
            Parameters.Add(emissiveColorParam);
            specularColorParam = new EffectParameter(ActiveUniformType.FloatVec4, "SpecularColor");
            Parameters.Add(specularColorParam);
            specularPowerParam = new EffectParameter(ActiveUniformType.Float, "SpecularPower");
            Parameters.Add(specularPowerParam);
            eyePositionParam = new EffectParameter(ActiveUniformType.FloatVec3, "EyePosition");
            Parameters.Add(eyePositionParam);
            fogColorParam = new EffectParameter(ActiveUniformType.FloatVec3, "FogColor");
            Parameters.Add(fogColorParam);
            fogVectorParam = new EffectParameter(ActiveUniformType.FloatVec3, "FogVector");
            Parameters.Add(fogVectorParam);
            worldParam = new EffectParameter(ActiveUniformType.FloatMat4, "World");
            Parameters.Add(worldParam);
            worldInverseTransposeParam = new EffectParameter(ActiveUniformType.FloatMat3, "WorldInverseTranspose");
            Parameters.Add(worldInverseTransposeParam);
            worldViewProjParam = new EffectParameter(ActiveUniformType.FloatMat4, "WorldViewProj");
            Parameters.Add(worldViewProjParam);
        }


        /// <summary>
        /// Creates a new BasicEffect by cloning parameter settings from an existing instance.
        /// </summary>
        protected BasicEffect(BasicEffect cloneSource)
            : base(cloneSource)
        {
            CacheEffectParameters(cloneSource);

            lightingEnabled = cloneSource.lightingEnabled;
            preferPerPixelLighting = cloneSource.preferPerPixelLighting;
            fogEnabled = cloneSource.fogEnabled;
            textureEnabled = cloneSource.textureEnabled;
            vertexColorEnabled = cloneSource.vertexColorEnabled;

            world = cloneSource.world;
            view = cloneSource.view;
            projection = cloneSource.projection;

            diffuseColor = cloneSource.diffuseColor;
            emissiveColor = cloneSource.emissiveColor;
            ambientLightColor = cloneSource.ambientLightColor;

            alpha = cloneSource.alpha;

            fogStart = cloneSource.fogStart;
            fogEnd = cloneSource.fogEnd;
        }


        /// <summary>
        /// Creates a clone of the current BasicEffect instance.
        /// </summary>
        public override Effect Clone()
        {
            return new BasicEffect(this);
        }


        /// <summary>
        /// Sets up the standard key/fill/back lighting rig.
        /// </summary>
        public void EnableDefaultLighting()
        {
            LightingEnabled = true;

            AmbientLightColor = EffectHelpers.EnableDefaultLighting(light0, light1, light2);
        }


        /// <summary>
        /// Looks up shortcut references to our effect parameters.
        /// </summary>
        void CacheEffectParameters(BasicEffect cloneSource)
        {
            textureParam                = Parameters["Texture"];
            diffuseColorParam           = Parameters["DiffuseColor"];
            emissiveColorParam          = Parameters["EmissiveColor"];
            specularColorParam          = Parameters["SpecularColor"];
            specularPowerParam          = Parameters["SpecularPower"];
            eyePositionParam            = Parameters["EyePosition"];
            fogColorParam               = Parameters["FogColor"];
            fogVectorParam              = Parameters["FogVector"];
            worldParam                  = Parameters["World"];
            worldInverseTransposeParam  = Parameters["WorldInverseTranspose"];
            worldViewProjParam          = Parameters["WorldViewProj"];
            shaderIndexParam            = Parameters["ShaderIndex"];

            light0 = new DirectionalLight(Parameters["DirLight0Direction"],
                                          Parameters["DirLight0DiffuseColor"],
                                          Parameters["DirLight0SpecularColor"],
                                          (cloneSource != null) ? cloneSource.light0 : null);

            light1 = new DirectionalLight(Parameters["DirLight1Direction"],
                                          Parameters["DirLight1DiffuseColor"],
                                          Parameters["DirLight1SpecularColor"],
                                          (cloneSource != null) ? cloneSource.light1 : null);

            light2 = new DirectionalLight(Parameters["DirLight2Direction"],
                                          Parameters["DirLight2DiffuseColor"],
                                          Parameters["DirLight2SpecularColor"],
                                          (cloneSource != null) ? cloneSource.light2 : null);
        }


        /// <summary>
        /// Lazily computes derived parameter values immediately before applying the effect.
        /// </summary>
        protected internal override void OnApply()
        {
            // Recompute the world+view+projection matrix or fog vector?
            dirtyFlags = EffectHelpers.SetWorldViewProjAndFog(dirtyFlags, ref world, ref view, ref projection, ref worldView, fogEnabled, fogStart, fogEnd, worldViewProjParam, fogVectorParam);
            
            // Recompute the diffuse/emissive/alpha material color parameters?
            if ((dirtyFlags & EffectDirtyFlags.MaterialColor) != 0)
            {
                EffectHelpers.SetMaterialColor(lightingEnabled, alpha, ref diffuseColor, ref emissiveColor, ref ambientLightColor, diffuseColorParam, emissiveColorParam);

                dirtyFlags &= ~EffectDirtyFlags.MaterialColor;
            }

            if (lightingEnabled)
            {
                // Recompute the world inverse transpose and eye position?
                dirtyFlags = EffectHelpers.SetLightingMatrices(dirtyFlags, ref world, ref view, worldParam, worldInverseTransposeParam, eyePositionParam);
                
                // Check if we can use the only-bother-with-the-first-light shader optimization.
                bool newOneLight = !light1.Enabled && !light2.Enabled;
                
                if (oneLight != newOneLight)
                {
                    oneLight = newOneLight;
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
                
                if (textureEnabled)
                    shaderIndex += 4;

                if (lightingEnabled)
                {
                    if (preferPerPixelLighting)
                        shaderIndex += 24;
                    else if (oneLight)
                        shaderIndex += 16;
                    else
                        shaderIndex += 8;
                }

                shaderIndexParam.SetValue(shaderIndex);

                dirtyFlags &= ~EffectDirtyFlags.ShaderIndex;
            }
        }


        #endregion
    }
}
