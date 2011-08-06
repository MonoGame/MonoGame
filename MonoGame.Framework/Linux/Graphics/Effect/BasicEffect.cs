using System;
using OpenTK.Graphics.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
	public class BasicEffect : Effect, IEffectMatrices, IEffectLights, IEffectFog
	{
		Texture2D _texture = null;
		
		private float _alpha;
		private Vector3 _ambientLightColor;
		private Vector3 _diffuseColor;
		private Vector3 _specularColor;
		private float _specularPower;
		
		private DirectionalLight light0;
		private DirectionalLight light1;
		private DirectionalLight light2;
		private bool lightingEnabled;
		
		
        public BasicEffect(GraphicsDevice device)
            : base(device)
        {
            createBasicEffect();
        }
		
        internal override void Apply()
        {
            GLStateManager.Projection(Projection);
            GLStateManager.World(World);
            GLStateManager.View(View);
			base.Apply();			
			
			// set camera
			Matrix _matrix = Matrix.Identity;
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(0, 320, 480, 0, -1, 1);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix( ref _matrix.M11 );
			GL.Viewport (0, 0, 320, 480);
						
			// Initialize OpenGL states (ideally move this to initialize somewhere else)	
			GL.Disable(EnableCap.DepthTest);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode,(int) All.BlendSrc);
			GL.Enable(EnableCap.Texture2D);
			GL.EnableClientState(ArrayCap.VertexArray);
			GL.EnableClientState(ArrayCap.ColorArray);
			GL.EnableClientState(ArrayCap.TextureCoordArray);
			
			GL.Disable(EnableCap.CullFace);		
        }

		public BasicEffect(GraphicsDevice device, EffectPool effectPool)
            : base(device, new byte[]{0}, CompilerOptions.None, effectPool)

        {
            createBasicEffect();
        }
		
		protected BasicEffect(GraphicsDevice device, BasicEffect clone)
            : base(device, clone)
        {
            createBasicEffect();
        }

		public override Effect Clone(GraphicsDevice device)
        {
            BasicEffect effect = new BasicEffect(device, this);
            return effect;
        }

        private void createBasicEffect()
        {
            var et = new EffectTechnique(this);
            Techniques["Wtf"] = et;
            CurrentTechnique = et;
			var pass = new EffectPass(et);
			pass.Name = "Wtf2";
            et.Passes[pass.Name] = pass; 
        }

        public void EnableDefaultLighting()
        {
           /* this.LightingEnabled = true;
            this.AmbientLightColor = new Vector3(0.05333332f, 0.09882354f, 0.1819608f);
            Vector3 color = new Vector3(1f, 0.9607844f, 0.8078432f);
            this.DirectionalLight0.DiffuseColor = color;
            this.DirectionalLight0.Direction = new Vector3(-0.5265408f, -0.5735765f, -0.6275069f);
            this.DirectionalLight0.SpecularColor = color;
            this.DirectionalLight0.Enabled = true;
            this.DirectionalLight1.DiffuseColor = new Vector3(0.9647059f, 0.7607844f, 0.4078432f);
            this.DirectionalLight1.Direction = new Vector3(0.7198464f, 0.3420201f, 0.6040227f);
            this.DirectionalLight1.SpecularColor = Vector3.Zero;
            this.DirectionalLight1.Enabled = true;
            color = new Vector3(0.3231373f, 0.3607844f, 0.3937255f);
            this.DirectionalLight2.DiffuseColor = color;
            this.DirectionalLight2.Direction = new Vector3(0.4545195f, -0.7660444f, 0.4545195f);
            this.DirectionalLight2.SpecularColor = color;
            this.DirectionalLight2.Enabled = true;*/
        }
   internal static Vector3 EnableDefaultLighting(DirectionalLight light0, DirectionalLight light1, DirectionalLight light2)
    {
      light0.Direction = new Vector3(-0.5265408f, -0.5735765f, -0.6275069f);
      light0.DiffuseColor = new Vector3(1f, 0.9607844f, 0.8078432f);
      light0.SpecularColor = new Vector3(1f, 0.9607844f, 0.8078432f);
      light0.Enabled = true;
      light1.Direction = new Vector3(0.7198464f, 0.3420201f, 0.6040227f);
      light1.DiffuseColor = new Vector3(0.9647059f, 0.7607844f, 0.4078432f);
      light1.SpecularColor = Vector3.Zero;
      light1.Enabled = true;
      light2.Direction = new Vector3(0.4545195f, -0.7660444f, 0.4545195f);
      light2.DiffuseColor = new Vector3(0.3231373f, 0.3607844f, 0.3937255f);
      light2.SpecularColor = new Vector3(0.3231373f, 0.3607844f, 0.3937255f);
      light2.Enabled = true;
      return new Vector3(0.05333332f, 0.09882354f, 0.1819608f);
    }		
			
		public Matrix Projection
		{ 
			get; set; 
		}
		
		public bool TextureEnabled 
		{ 
			get; set; 
		}
		
		public bool VertexColorEnabled 
		{ 
			get; set; 
		}
		#region IEffectMatrices implementation		
		public Matrix View
		{ 
			get; set; 
		}

		public Matrix World
		{ 
			get; set; 
		}
		#endregion
		
		private void setTexture(Texture2D texture)
		{
			GL.BindTexture(TextureTarget.Texture2D, texture.Image.Name);
		}

        public Texture2D Texture {
			get { return _texture; }
			set { _texture = value; setTexture(value); }
		}

		public float Alpha { 
			get {
				return _alpha;
			} 
			set {
				_alpha = value;
			}
		}
		
		public Vector3 DiffuseColor {
			get {
				return _diffuseColor;
			}
			
			set {
				_diffuseColor = value;
			}
		}

		public Vector3 SpecularColor {
			get {
				return _specularColor;
			}
			
			set {
				_specularColor = value;
			}
		}
		
		public float SpecularPower { 
			get { return _specularPower; }
			set { _specularPower = value; } 
		}
		
		#region IEffectLights implementation
		void IEffectLights.EnableDefaultLighting ()
		{
			throw new NotImplementedException ();
		}

		public Vector3 AmbientLightColor {
			get {
				return _ambientLightColor;
			}
			
			set {
				_ambientLightColor = value;
			}
		}

		public DirectionalLight DirectionalLight0 {
			get {
				return light0;
			}
		}

		public DirectionalLight DirectionalLight1 {
			get {
				return light1;
			}
		}

		public DirectionalLight DirectionalLight2 {
			get {
				return light2;
			}
		}

		public bool LightingEnabled {
			get {
				return lightingEnabled;
			}
			set {
				lightingEnabled = value;
			}
		}
		#endregion

		#region IEffectFog implementation
		Vector3 IEffectFog.FogColor {
			get; set;
		}

		bool IEffectFog.FogEnabled {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		float IEffectFog.FogEnd {
			get; set;
		}

		float IEffectFog.FogStart {
			get; set;
		}
		#endregion
    }
}
