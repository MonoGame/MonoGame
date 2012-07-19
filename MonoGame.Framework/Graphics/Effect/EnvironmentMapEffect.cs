#region File Description
//-----------------------------------------------------------------------------
// EnvironmentMapEffect.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Built-in effect that supports environment mapping.
    /// </summary>
    public class EnvironmentMapEffect : Effect, IEffectMatrices, IEffectLights, IEffectFog
    {
        #region Effect Parameters

        EffectParameter textureParam;
        EffectParameter environmentMapParam;
        EffectParameter environmentMapAmountParam;
        EffectParameter environmentMapSpecularParam;
        EffectParameter fresnelFactorParam;
        EffectParameter diffuseColorParam;
        EffectParameter emissiveColorParam;
        EffectParameter eyePositionParam;
        EffectParameter fogColorParam;
        EffectParameter fogVectorParam;
        EffectParameter worldParam;
        EffectParameter worldInverseTransposeParam;
        EffectParameter worldViewProjParam;

        int _shaderIndex = -1;

        #endregion

        #region Fields

        bool oneLight;
        bool fogEnabled;
        bool fresnelEnabled;
        bool specularEnabled;

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

        static readonly byte[] Bytecode = LoadEffectResource(
#if DIRECTX
            "Microsoft.Xna.Framework.Graphics.Effect.Resources.EnvironmentMapEffect.dx11.mgfxo"
#else
            "Microsoft.Xna.Framework.Graphics.Effect.Resources.EnvironmentMapEffect.ogl.mgfxo"
#endif
        );

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
        /// Gets or sets the current texture.
        /// </summary>
        public Texture2D Texture
        {
            get { return textureParam.GetValueTexture2D(); }
            set { textureParam.SetValue(value); }
        }


        /// <summary>
        /// Gets or sets the current environment map texture.
        /// </summary>
        public TextureCube EnvironmentMap
        {
            get { return environmentMapParam.GetValueTextureCube(); }
            set { environmentMapParam.SetValue(value); }
        }
        
        
        /// <summary>
        /// Gets or sets the amount of the environment map RGB that will be blended over 
        /// the base texture. Range 0 to 1, default 1. If set to zero, the RGB channels 
        /// of the environment map will completely ignored (but the environment map alpha 
        /// may still be visible if EnvironmentMapSpecular is greater than zero).
        /// </summary>
        public float EnvironmentMapAmount
        {
            get { return environmentMapAmountParam.GetValueSingle(); }
            set { environmentMapAmountParam.SetValue(value); }
        }


        /// <summary>
        /// Gets or sets the amount of the environment map alpha channel that will 
        /// be added to the base texture. Range 0 to 1, default 0. This can be used 
        /// to implement cheap specular lighting, by encoding one or more specular 
        /// highlight patterns into the environment map alpha channel, then setting 
        /// EnvironmentMapSpecular to the desired specular light color.
        /// </summary>
        public Vector3 EnvironmentMapSpecular
        {
            get { return environmentMapSpecularParam.GetValueVector3(); }

            set
            {
                environmentMapSpecularParam.SetValue(value);
                
                bool enabled = (value != Vector3.Zero);
                
                if (specularEnabled != enabled)
                {
                    specularEnabled = enabled;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
            }
        }
        
        
        /// <summary>
        /// Gets or sets the Fresnel factor used for the environment map blending. 
        /// Higher values make the environment map only visible around the silhouette 
        /// edges of the object, while lower values make it visible everywhere. 
        /// Setting this property to 0 disables Fresnel entirely, making the 
        /// environment map equally visible regardless of view angle. The default is 
        /// 1. Fresnel only affects the environment map RGB (the intensity of which is 
        /// controlled by EnvironmentMapAmount). The alpha contribution (controlled by 
        /// EnvironmentMapSpecular) is not affected by the Fresnel setting.
        /// </summary>
        public float FresnelFactor
        {
            get { return fresnelFactorParam.GetValueSingle(); }

            set
            {
                fresnelFactorParam.SetValue(value);
                
                bool enabled = (value != 0);
                
                if (fresnelEnabled != enabled)
                {
                    fresnelEnabled = enabled;
                    dirtyFlags |= EffectDirtyFlags.ShaderIndex;
                }
            }
        }


        /// <summary>
        /// This effect requires lighting, so we explicitly implement
        /// IEffectLights.LightingEnabled, and do not allow turning it off.
        /// </summary>
        bool IEffectLights.LightingEnabled
        {
            get { return true; }
            set { if (!value) throw new NotSupportedException("EnvironmentMapEffect does not support setting LightingEnabled to false."); }
        }


        #endregion

        #region Methods


        /// <summary>
        /// Creates a new EnvironmentMapEffect with default parameter settings.
        /// </summary>
        public EnvironmentMapEffect(GraphicsDevice device)
            : base(device, Bytecode)
        {
            CacheEffectParameters(null);

            DirectionalLight0.Enabled = true;

            EnvironmentMapAmount = 1;
            EnvironmentMapSpecular = Vector3.Zero;
            FresnelFactor = 1;
        }


        /// <summary>
        /// Creates a new EnvironmentMapEffect by cloning parameter settings from an existing instance.
        /// </summary>
        protected EnvironmentMapEffect(EnvironmentMapEffect cloneSource)
            : base(cloneSource)
        {
            CacheEffectParameters(cloneSource);

            fogEnabled = cloneSource.fogEnabled;
            fresnelEnabled = cloneSource.fresnelEnabled;
            specularEnabled = cloneSource.specularEnabled;

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
        /// Creates a clone of the current EnvironmentMapEffect instance.
        /// </summary>
        public override Effect Clone()
        {
            return new EnvironmentMapEffect(this);
        }


        /// <summary>
        /// Sets up the standard key/fill/back lighting rig.
        /// </summary>
        public void EnableDefaultLighting()
        {
            AmbientLightColor = EffectHelpers.EnableDefaultLighting(light0, light1, light2);
        }


        /// <summary>
        /// Looks up shortcut references to our effect parameters.
        /// </summary>
        void CacheEffectParameters(EnvironmentMapEffect cloneSource)
        {
            textureParam                = Parameters["Texture"];
            environmentMapParam         = Parameters["EnvironmentMap"];
            environmentMapAmountParam   = Parameters["EnvironmentMapAmount"];
            environmentMapSpecularParam = Parameters["EnvironmentMapSpecular"];
            fresnelFactorParam          = Parameters["FresnelFactor"];
            diffuseColorParam           = Parameters["DiffuseColor"];
            emissiveColorParam          = Parameters["EmissiveColor"];
            eyePositionParam            = Parameters["EyePosition"];
            fogColorParam               = Parameters["FogColor"];
            fogVectorParam              = Parameters["FogVector"];
            worldParam                  = Parameters["World"];
            worldInverseTransposeParam  = Parameters["WorldInverseTranspose"];
            worldViewProjParam          = Parameters["WorldViewProj"];

            light0 = new DirectionalLight(Parameters["DirLight0Direction"],
                                          Parameters["DirLight0DiffuseColor"],
                                          null,
                                          (cloneSource != null) ? cloneSource.light0 : null);

            light1 = new DirectionalLight(Parameters["DirLight1Direction"],
                                          Parameters["DirLight1DiffuseColor"],
                                          null,
                                          (cloneSource != null) ? cloneSource.light1 : null);

            light2 = new DirectionalLight(Parameters["DirLight2Direction"],
                                          Parameters["DirLight2DiffuseColor"],
                                          null,
                                          (cloneSource != null) ? cloneSource.light2 : null);
        }


        /// <summary>
        /// Lazily computes derived parameter values immediately before applying the effect.
        /// </summary>
        protected internal override bool OnApply()
        {
            // Recompute the world+view+projection matrix or fog vector?
            dirtyFlags = EffectHelpers.SetWorldViewProjAndFog(dirtyFlags, ref world, ref view, ref projection, ref worldView, fogEnabled, fogStart, fogEnd, worldViewProjParam, fogVectorParam);

            // Recompute the world inverse transpose and eye position?
            dirtyFlags = EffectHelpers.SetLightingMatrices(dirtyFlags, ref world, ref view, worldParam, worldInverseTransposeParam, eyePositionParam);
            
            // Recompute the diffuse/emissive/alpha material color parameters?
            if ((dirtyFlags & EffectDirtyFlags.MaterialColor) != 0)
            {
                EffectHelpers.SetMaterialColor(true, alpha, ref diffuseColor, ref emissiveColor, ref ambientLightColor, diffuseColorParam, emissiveColorParam);

                dirtyFlags &= ~EffectDirtyFlags.MaterialColor;
            }

            // Check if we can use the only-bother-with-the-first-light shader optimization.
            bool newOneLight = !light1.Enabled && !light2.Enabled;
            
            if (oneLight != newOneLight)
            {
                oneLight = newOneLight;
                dirtyFlags |= EffectDirtyFlags.ShaderIndex;
            }

            // Recompute the shader index?
            if ((dirtyFlags & EffectDirtyFlags.ShaderIndex) != 0)
            {
                int shaderIndex = 0;
                
                if (!fogEnabled)
                    shaderIndex += 1;
                
                if (fresnelEnabled)
                    shaderIndex += 2;
                
                if (specularEnabled)
                    shaderIndex += 4;

                if (oneLight)
                    shaderIndex += 8;

                dirtyFlags &= ~EffectDirtyFlags.ShaderIndex;

                if (_shaderIndex != shaderIndex)
                {
                    _shaderIndex = shaderIndex;
                    CurrentTechnique = Techniques[_shaderIndex];
                    return true;
                }
            }

            return false;
        }


        #endregion
    }
}
