using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// /// Built-in effect that supports optional texturing, vertex coloring, fog, and lighting.
	/// /// </summary>
	public class BasicEffect : Effect, IEffectMatrices, IEffectLights, IEffectFog
	{
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
		EffectParameter shaderIndexParam;
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



		/// <summary>
		/// Gets or sets the world matrix.
		/// </summary>
		public Matrix World {
			get { return world; }

			set {
				world = value;
				dirtyFlags |= EffectDirtyFlags.World | EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
			}
		}


		/// <summary>
		/// Gets or sets the view matrix.
		/// </summary>
		public Matrix View {
			get { return view; }

			set {
				view = value;
				dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.EyePosition | EffectDirtyFlags.Fog;
			}
		}


		/// <summary>
		/// Gets or sets the projection matrix.
		/// </summary>
		public Matrix Projection {
			get { return projection; }
            
			set {
				projection = value;
				dirtyFlags |= EffectDirtyFlags.WorldViewProj;
			}
		}


		/// <summary>
		/// Gets or sets the material diffuse color (range 0 to 1).
		/// </summary>
		public Vector3 DiffuseColor {
			get { return diffuseColor; }
            
			set {
				diffuseColor = value;
				dirtyFlags |= EffectDirtyFlags.MaterialColor;
			}
		}


		/// <summary>
		/// Gets or sets the material emissive color (range 0 to 1).
		/// </summary>
		public Vector3 EmissiveColor {
			get { return emissiveColor; }
            
			set {
				emissiveColor = value;
				dirtyFlags |= EffectDirtyFlags.MaterialColor;
			}
		}


		/// <summary>
		/// Gets or sets the material specular color (range 0 to 1).
		/// </summary>
		public Vector3 SpecularColor {
			get { return specularColorParam.GetValueVector3 (); }
			set { specularColorParam.SetValue (value); }
		}


		/// <summary>
		/// Gets or sets the material specular power.
		/// </summary>
		public float SpecularPower {
			get { return specularPowerParam.GetValueSingle (); }
			set { specularPowerParam.SetValue (value); }
		}


		/// <summary>
		/// Gets or sets the material alpha.
		/// </summary>
		public float Alpha {
			get { return alpha; }
            
			set {
				alpha = value;
				dirtyFlags |= EffectDirtyFlags.MaterialColor;
			}
		}


		/// <summary>
		/// Gets or sets the lighting enable flag.
		/// </summary>
		public bool LightingEnabled {
			get { return lightingEnabled; }
            
			set {
				if (lightingEnabled != value) {
					lightingEnabled = value;
					dirtyFlags |= EffectDirtyFlags.ShaderIndex | EffectDirtyFlags.MaterialColor;
				}
			}
		}


		/// <summary>
		/// Gets or sets the per-pixel lighting prefer flag.
		/// </summary>
		public bool PreferPerPixelLighting {
			get { return preferPerPixelLighting; }
            
			set {
				if (preferPerPixelLighting != value) {
					preferPerPixelLighting = value;
					dirtyFlags |= EffectDirtyFlags.ShaderIndex;
				}
			}
		}


		/// <summary>
		/// Gets or sets the ambient light color (range 0 to 1).
		/// </summary>
		public Vector3 AmbientLightColor {
			get { return ambientLightColor; }

			set {
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
		public bool FogEnabled {
			get { return fogEnabled; }

			set {
				if (fogEnabled != value) {
					fogEnabled = value;
					dirtyFlags |= EffectDirtyFlags.ShaderIndex | EffectDirtyFlags.FogEnable;
				}
			}
		}

		/// <summary>
		/// Gets or sets the fog start distance.
		/// </summary>
		public float FogStart {
			get { return fogStart; }
			set {
				fogStart = value;
				dirtyFlags |= EffectDirtyFlags.Fog;
			}
		}
		/// <summary>
		/// Gets or sets the fog end distance.
		/// </summary>
		public float FogEnd {
			get { return fogEnd; }

			set {
				fogEnd = value;
				dirtyFlags |= EffectDirtyFlags.Fog;
			}
		}


		/// <summary>
		/// Gets or sets the fog color.
		/// </summary>
		public Vector3 FogColor {
			get { return fogColorParam.GetValueVector3 (); }
			set { fogColorParam.SetValue (value); }
		}


		/// <summary>
		/// Gets or sets whether texturing is enabled.
		/// </summary>
		public bool TextureEnabled {
			get { return textureEnabled; }

			set {
				if (textureEnabled != value) {
					textureEnabled = value;
					dirtyFlags |= EffectDirtyFlags.ShaderIndex;
				}
			}
		}


		Texture2D _texture = null;

		/// <summary>
		/// Gets or sets the current texture.
		/// </summary>
		public Texture2D Texture {
			get { return textureParam.GetValueTexture2D (); }
			set {
				_texture = value;
				_texture.Apply();
				textureParam.SetValue (value);
			}
		}


		/// <summary>
		/// Gets or sets whether vertex color is enabled.

		/// </summary>
		public bool VertexColorEnabled {
			get { return vertexColorEnabled; }

			set {
				if (vertexColorEnabled != value) {
					vertexColorEnabled = value;
					dirtyFlags |= EffectDirtyFlags.ShaderIndex;
				}
			}
		}

		bool shadersLoaded = false;
		int[] VSArray = new int[20];
		int[] PSArray = new int[10];

		/// <summary>
		/// Creates a new BasicEffect with default parameter settings.
		/// </summary>
		public BasicEffect (GraphicsDevice device)
			//:base(device, Resources.BasicEffect)
			:base(device)
		{
			if (!shadersLoaded) {

				// Load Vertex Shaders
				VSArray[0] = CreateVertexShaderFromSource (VSBasicEffect.VSBasic);
				VSArray[1] = CreateVertexShaderFromSource (VSBasicEffect.VSBasicNoFog);
				VSArray[2] = CreateVertexShaderFromSource (VSBasicEffect.VSBasicVc);
				VSArray[3] = CreateVertexShaderFromSource (VSBasicEffect.VSBasicVcNoFog);
				VSArray[4] = CreateVertexShaderFromSource (VSBasicEffect.VSBasicTx);
				VSArray[5] = CreateVertexShaderFromSource (VSBasicEffect.VSBasicTxNoFog);
				VSArray[6] = CreateVertexShaderFromSource (VSBasicEffect.VSBasicTxVc);
				VSArray[7] = CreateVertexShaderFromSource (VSBasicEffect.VSBasicTxVcNoFog);

				PSArray[0] = CreateFragmentShaderFromSource (PSBasicEffect.PSBasic);
				PSArray[1] = CreateFragmentShaderFromSource (PSBasicEffect.PSBasicNoFog);
//				PSArray[2] = CreateFragmentShaderFromSource (PSBasicEffect.PSBasicVc);
//				PSArray[3] = CreateFragmentShaderFromSource (PSBasicEffect.PSBasicVcNoFog);
				PSArray[3] = CreateFragmentShaderFromSource (PSBasicEffect.PSBasicTx);
				PSArray[4] = CreateFragmentShaderFromSource (PSBasicEffect.PSBasicTxNoFog);

			}

			DefineTechnique ("BasicEffect", "", 0, 0);
			CurrentTechnique = Techniques ["BasicEffect"];

			CacheEffectParameters (null);
			
//			DirectionalLight0.Enabled = true;
//
//			SpecularColor = Vector3.One;
//			SpecularPower = 16;

		}


		/// <summary>
		/// Creates a new BasicEffect by cloning parameter settings from an existing instance.
		/// </summary>
		protected BasicEffect (BasicEffect cloneSource)
            : base(cloneSource)
		{
			CacheEffectParameters (cloneSource);

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
		public override Effect Clone ()
		{
			return new BasicEffect (this);
		}


		/// <summary>
		/// Sets up the standard key/fill/back lighting rig.
		/// </summary>
		public void EnableDefaultLighting ()
		{
			LightingEnabled = true;

			AmbientLightColor = EffectHelpers.EnableDefaultLighting (light0, light1, light2);
		}

		/// <summary>
		/// Looks up shortcut references to our effect parameters.
		/// </summary>
		void CacheEffectParameters (BasicEffect cloneSource)
		{

			
			textureParam = Parameters ["Texture"];
			diffuseColorParam = Parameters ["DiffuseColor"];
//			emissiveColorParam = Parameters ["EmissiveColor"];
//			specularColorParam = Parameters ["SpecularColor"];
//			specularPowerParam = Parameters ["SpecularPower"];
//			eyePositionParam = Parameters ["EyePosition"];
//			fogColorParam = Parameters ["FogColor"];
//			fogVectorParam = Parameters ["FogVector"];
			worldParam = Parameters ["World"];
//			worldInverseTransposeParam = Parameters ["WorldInverseTranspose"];
			worldViewProjParam = Parameters ["WorldViewProj"];
//			shaderIndexParam = Parameters ["ShaderIndex"];
//
//			light0 = new DirectionalLight (Parameters ["DirLight0Direction"],
//                                          Parameters ["DirLight0DiffuseColor"],
//                                          Parameters ["DirLight0SpecularColor"],
//                                          (cloneSource != null) ? cloneSource.light0 : null);
//
//			light1 = new DirectionalLight (Parameters ["DirLight1Direction"],
//                                          Parameters ["DirLight1DiffuseColor"],
//                                          Parameters ["DirLight1SpecularColor"],
//                                          (cloneSource != null) ? cloneSource.light1 : null);
//
//			light2 = new DirectionalLight (Parameters ["DirLight2Direction"],
//                                          Parameters ["DirLight2DiffuseColor"],
//                                          Parameters ["DirLight2SpecularColor"],
//                                          (cloneSource != null) ? cloneSource.light2 : null);
		}

		int oldIndex = 0;
		int shaderIndex = 0;

		int[] VSIndices = new int[32]
		{
		    0,      // basic
		    1,      // no fog
		    2,      // vertex color
		    3,      // vertex color, no fog
		    4,      // texture
		    5,      // texture, no fog
		    6,      // texture + vertex color
		    7,      // texture + vertex color, no fog
		
		    8,      // vertex lighting
		    8,      // vertex lighting, no fog
		    9,      // vertex lighting + vertex color
		    9,      // vertex lighting + vertex color, no fog
		    10,     // vertex lighting + texture
		    10,     // vertex lighting + texture, no fog
		    11,     // vertex lighting + texture + vertex color
		    11,     // vertex lighting + texture + vertex color, no fog
		
		    12,     // one light
		    12,     // one light, no fog
		    13,     // one light + vertex color
		    13,     // one light + vertex color, no fog
		    14,     // one light + texture
		    14,     // one light + texture, no fog
		    15,     // one light + texture + vertex color
		    15,     // one light + texture + vertex color, no fog
		
		    16,     // pixel lighting
		    16,     // pixel lighting, no fog
		    17,     // pixel lighting + vertex color
		    17,     // pixel lighting + vertex color, no fog
		    18,     // pixel lighting + texture
		    18,     // pixel lighting + texture, no fog
		    19,     // pixel lighting + texture + vertex color
		    19,     // pixel lighting + texture + vertex color, no fog
		};

		int[] PSIndices = new int[32]
		{
		    0,      // basic
		    1,      // no fog
		    0,      // vertex color
		    1,      // vertex color, no fog
		    2,      // texture
		    3,      // texture, no fog
		    2,      // texture + vertex color
		    3,      // texture + vertex color, no fog
		
		    4,      // vertex lighting
		    5,      // vertex lighting, no fog
		    4,      // vertex lighting + vertex color
		    5,      // vertex lighting + vertex color, no fog
		    6,      // vertex lighting + texture
		    7,      // vertex lighting + texture, no fog
		    6,      // vertex lighting + texture + vertex color
		    7,      // vertex lighting + texture + vertex color, no fog
		
		    4,      // one light
		    5,      // one light, no fog
		    4,      // one light + vertex color
		    5,      // one light + vertex color, no fog
		    6,      // one light + texture
		    7,      // one light + texture, no fog
		    6,      // one light + texture + vertex color
		    7,      // one light + texture + vertex color, no fog
		
		    8,      // pixel lighting
		    8,      // pixel lighting, no fog
		    8,      // pixel lighting + vertex color
		    8,      // pixel lighting + vertex color, no fog
		    9,      // pixel lighting + texture
		    9,      // pixel lighting + texture, no fog
		    9,      // pixel lighting + texture + vertex color
		    9,      // pixel lighting + texture + vertex color, no fog
		};


		/// <summary>
		/// Lazily computes derived parameter values immediately before applying the effect.
		/// </summary>
		protected internal override void OnApply ()
		{


			// Recompute the shader index?
//			if ((dirtyFlags & EffectDirtyFlags.ShaderIndex) != 0) {
				int shaderIndex = 0;

				if (!fogEnabled)
					shaderIndex += 1;

				if (vertexColorEnabled)
					shaderIndex += 2;

				if (textureEnabled)
					shaderIndex += 4;

				if (lightingEnabled) {
					if (preferPerPixelLighting)
						shaderIndex += 24;
					else if (oneLight)
						shaderIndex += 16;
					else
						shaderIndex += 8;
				}
//
//				//shaderIndexParam.SetValue (shaderIndex);
//
//				dirtyFlags &= ~EffectDirtyFlags.ShaderIndex;
				if (oldIndex != shaderIndex) {
					int vertexShader = VSArray[VSIndices[shaderIndex]];
					int fragmentShader = PSArray[PSIndices[shaderIndex]];
					UpdateTechnique("BasicEffect", "", vertexShader, fragmentShader);
					oldIndex = shaderIndex;
					// Update here
				}
//			}


			// These are the states that work
			GLStateManager.Projection(Projection);
			GLStateManager.WorldView(world, view);

			// Override this for now for testing purposes
			dirtyFlags |= EffectDirtyFlags.World | EffectDirtyFlags.WorldViewProj;
			dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.EyePosition;
			dirtyFlags &= ~EffectDirtyFlags.FogEnable; // turn off fog for now
			dirtyFlags |= EffectDirtyFlags.MaterialColor;

			GLStateManager.Textures2D(TextureEnabled);
			GLStateManager.ColorArray(VertexColorEnabled);

			// Recompute the world+view+projection matrix or fog vector?
			dirtyFlags = EffectHelpers.SetWorldViewProjAndFog (dirtyFlags, ref world, ref view, ref projection, ref worldView, fogEnabled, fogStart, fogEnd, worldViewProjParam, fogVectorParam);

			// Recompute the diffuse/emissive/alpha material color parameters?
			if ((dirtyFlags & EffectDirtyFlags.MaterialColor) != 0) {
				EffectHelpers.SetMaterialColor (lightingEnabled, alpha, ref diffuseColor, ref emissiveColor, ref ambientLightColor, diffuseColorParam, emissiveColorParam);

				dirtyFlags &= ~EffectDirtyFlags.MaterialColor;
			}

			if (TextureEnabled) {
				textureParam.SetValue(_texture);
			}
//
//			if (lightingEnabled) {
//				// Recompute the world inverse transpose and eye position?
//				dirtyFlags = EffectHelpers.SetLightingMatrices (dirtyFlags, ref world, ref view, worldParam, worldInverseTransposeParam, eyePositionParam);
//                
//				// Check if we can use the only-bother-with-the-first-light shader optimization.
//				bool newOneLight = !light1.Enabled && !light2.Enabled;
//                
//				if (oneLight != newOneLight) {
//					oneLight = newOneLight;
//					dirtyFlags |= EffectDirtyFlags.ShaderIndex;
//				}
//			}




		}



	}
}
