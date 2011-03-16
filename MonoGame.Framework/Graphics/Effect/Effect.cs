//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.IO;
//using OpenTK.Graphics.ES20;
//using OpenTK.Graphics.ES11;
//using GL11 = OpenTK.Graphics.ES11.GL;
//using GL20 = OpenTK.Graphics.ES20.GL;
//using All11 = OpenTK.Graphics.ES11.All;
//using All20 = OpenTK.Graphics.ES20.All;

//namespace Microsoft.Xna.Framework.Graphics
//{
//    public class Effect : IDisposable
//    {
//        public EffectParameterCollection Parameters { get; set; }
//        public EffectTechniqueCollection Techniques { get; set; }
//        private GraphicsDevice graphicsDevice;
//        private int fragment_handle;
//        private int vertex_handle;
//        private bool fragment;
//        private bool vertex;

//        public Effect (
//         GraphicsDevice graphicsDevice,
//         byte[] effectCode,
//         CompilerOptions options,
//         EffectPool pool)
//        {
//            Parameters = new EffectParameterCollection();
//            Techniques = new EffectTechniqueCollection();			

//            if (graphicsDevice == null)
//            {
//                throw new ArgumentNullException("Graphics Device Cannot Be Null");
//            }
//            this.graphicsDevice = graphicsDevice;
			
//            if (pool == null)
//            { 
//                return;
//                // TODO throw new ArgumentNullException("Effect Pool Cannot Be Null");
//            }
			
//            int fragmentblocklength = BitConverter.ToInt32(effectCode, 0);

//            int vertexblocklength = BitConverter.ToInt32(effectCode, fragmentblocklength + 4);

//            if (fragmentblocklength != 0)
//            {
//                fragment_handle = GL20.CreateShader( All20.FragmentShader );
//                fragment = true;
//            }

//            if (vertexblocklength != 0)
//            {
//                vertex_handle = GL20.CreateShader( All20.VertexShader );
//                vertex = true;
//            }

//            if (fragment)
//            {
//                string[] fragmentstring = new string[1] { Encoding.UTF8.GetString(effectCode, 4, fragmentblocklength) };
//                int[] fragmentLength = new int[1] { fragmentstring[0].Length };
//                GL20.ShaderSource(fragment_handle, 1, fragmentstring, fragmentLength);
//            }

//            if (vertex)
//            {
//                string[] vertexstring = new string[1] { Encoding.UTF8.GetString(effectCode, fragmentblocklength + 8, vertexblocklength) };
//                int[] vertexLength = new int[1] { vertexstring[0].Length };
//                GL20.ShaderSource(vertex_handle, 1, vertexstring, vertexLength);
//            }
			
//            int compiled = 0;

//            if (fragment)
//            {
//                GL20.CompileShader(fragment_handle);
				
//                GL20.GetShader(fragment_handle, All20.CompileStatus, ref compiled );
//                if (compiled == (int)All20.False)
//                {
//                    Console.Write("Fragment Compilation Failed!");
//                }
//            }

//            if (vertex)
//            {
//                GL20.CompileShader(vertex_handle);
//                GL20.GetShader(vertex_handle, All20.CompileStatus, ref compiled );
//                if (compiled == (int)All20.False)
//                {
//                    Console.Write("Vertex Compilation Failed!");
//                }
//            }

//        }
		
//        protected Effect(GraphicsDevice graphicsDevice, Effect cloneSource )
//        {
//            Parameters = new EffectParameterCollection();
//            Techniques = new EffectTechniqueCollection();

//            if (graphicsDevice == null)
//            {
//                throw new ArgumentNullException("Graphics Device Cannot Be Null");
//            }
//            this.graphicsDevice = graphicsDevice;
//        }

//        internal virtual void Apply()
//        {
//            GLStateManager.Cull(GraphicsDevice.RasterizerState.CullMode.OpenGL11());
//            // TODO: This is prolly not right (DepthBuffer, etc)
//            GLStateManager.DepthTest(GraphicsDevice.DepthStencilState.DepthBufferEnable);
//        }
		
//        public virtual Effect Clone(GraphicsDevice device)
//        {
//            Effect f = new Effect( graphicsDevice, this );
//            return f;
//        }
		
//        public void Dispose()
//        {
//        }
		
//        internal static string Normalize(string FileName)
//        {
//            if (File.Exists(FileName))
//                return FileName;
			
//            // Check the file extension
//            if (!string.IsNullOrEmpty(Path.GetExtension(FileName)))
//            {
//                return null;
//            }
			
//            // Concat the file name with valid extensions
//            if (File.Exists(FileName+".fsh"))
//                return FileName+".fsh";
//            if (File.Exists(FileName+".vsh"))
//                return FileName+".vsh";
			
//            return null;
//        }
		
//        public EffectTechnique CurrentTechnique { get; set; }

//        internal Effect(GraphicsDevice device)
//        {
//            graphicsDevice = device;
//            Parameters = new EffectParameterCollection();
//            Techniques = new EffectTechniqueCollection();			
//            CurrentTechnique = new EffectTechnique(this);
//        }
//    }
//}
