using System;

#if WINDOWS
using GL11 = OpenTK.Graphics.OpenGL.GL;
using MatrixMode11 = OpenTK.Graphics.OpenGL.MatrixMode;
using EnableCap11 = OpenTK.Graphics.OpenGL.EnableCap;
using TextureEnvTarget11 = OpenTK.Graphics.OpenGL.TextureEnvTarget;
using TextureEnvParameter11 = OpenTK.Graphics.OpenGL.TextureEnvParameter;
using ArrayCap11 = OpenTK.Graphics.OpenGL.ArrayCap;
using TextureTarget11 = OpenTK.Graphics.OpenGL.TextureTarget;
#else
using GL11 = OpenTK.Graphics.ES11.GL;
using All11 = OpenTK.Graphics.ES11.All;

using MatrixMode11 = OpenTK.Graphics.ES11.All;
using EnableCap11 = OpenTK.Graphics.ES11.All;
using TextureEnvTarget11 = OpenTK.Graphics.ES11.All;
using TextureEnvParameter11 = OpenTK.Graphics.ES11.All;
using ArrayCap11 = OpenTK.Graphics.ES11.All;
using TextureTarget11 = OpenTK.Graphics.ES11.All;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public class BasicEffect : Effect, IEffectMatrices, IEffectLights, IEffectFog
	{
		Texture2D _texture = null;
		
        public BasicEffect(GraphicsDevice device)
            : base(device)
        {
            createBasicEffect();
        }
		
        internal override void Apply()
        {
            //GLStateManager.Projection(Projection);
            //GLStateManager.World(World);
           // GLStateManager.View(View);
			base.Apply();

            GL11.Viewport(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
         
            GL11.MatrixMode(MatrixMode11.Projection);
            GL11.LoadIdentity();
            Matrix projectionMatrix = Projection;
            GL11.LoadMatrix(ref projectionMatrix.M11);
         
            GL11.MatrixMode(MatrixMode11.Modelview);
            GL11.LoadIdentity();
            Matrix viewMatrix = View;
            GL11.LoadMatrix(ref viewMatrix.M11);
            
						
			// Initialize OpenGL states (ideally move this to initialize somewhere else)	
            GL11.Disable(EnableCap11.DepthTest);
#if WINDOWS
            GL11.TexEnv(TextureEnvTarget11.TextureEnv, TextureEnvParameter11.TextureEnvMode, (int)EnableCap11.Blend);
#else
            GL11.TexEnv(TextureEnvTarget11.TextureEnv, TextureEnvParameter11.TextureEnvMode, (int)All11.BlendSrc);
#endif
            if(Texture != null)
                GL11.Enable(EnableCap11.Texture2D);
            else
                GL11.Disable(EnableCap11.Texture2D);

            GL11.Disable(EnableCap11.CullFace);		
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

            et.Passes["Wtf2"] = new EffectPass(et);
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
			
		public bool LightingEnabled 
		{ 
			get; set; 
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
		
		public Matrix View
		{ 
			get; set; 
		}

		public Matrix World
		{ 
			get; set; 
		}
		
		private void setTexture(Texture2D texture)
		{
            if(texture != null)
                texture.Apply();
		}

        public Texture2D Texture {
			get { return _texture; }
			set { _texture = value; setTexture(value); }
		}
		
		public Vector3 DiffuseColor {
			get; set;
		}
		
		public float Alpha {
			get; set;
		}

		#region IEffectMatrices implementation
		Matrix IEffectMatrices.Projection {
			get; set;
		}

		Matrix IEffectMatrices.View {
			get; set;
		}

		Matrix IEffectMatrices.World {
			get; set;
		}
		#endregion

		#region IEffectLights implementation
		void IEffectLights.EnableDefaultLighting ()
		{
			throw new NotImplementedException ();
		}

		public Vector3 AmbientLightColor {
			get; set;
		}

		DirectionalLight IEffectLights.DirectionalLight0 {
			get {
				throw new NotImplementedException ();
			}
		}

		DirectionalLight IEffectLights.DirectionalLight1 {
			get {
				throw new NotImplementedException ();
			}
		}

		DirectionalLight IEffectLights.DirectionalLight2 {
			get {
				throw new NotImplementedException ();
			}
		}

		bool IEffectLights.LightingEnabled {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		#endregion

		#region IEffectFog implementation

	    public Vector3 FogColor {
			get; set;
		}

	    public bool FogEnabled {
			get { return false; }
			set {
                if(value)
				    throw new NotImplementedException ();
			}
		}

	    public float FogEnd {
			get; set;
		}

	    public float FogStart {
			get; set;
		}
		#endregion
    }
}
