// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
// 

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OpenTK.Graphics.ES20;
using System.Linq;

namespace Microsoft.Xna.Framework.Graphics
{
	public class Effect : GraphicsResource
    {
        public EffectParameterCollection Parameters { get; set; }
        public EffectTechniqueCollection Techniques { get; set; }		
		
		internal List<EffectParameter> _textureMappings = new List<EffectParameter>();

		private int fragment_handle;
        private int vertex_handle;
        private bool fragment;
        private bool vertex;
		
		internal List<int> vertexShaders = new List<int>();
		internal List<int> fragmentShaders = new List<int>();

		protected Effect(Effect cloneSource) : this(cloneSource.GraphicsDevice)
		{
			this.CurrentTechnique = cloneSource.CurrentTechnique;
			this.Name = cloneSource.Name;
			this.Parameters = cloneSource.Parameters;
			this.Tag = cloneSource.Tag;
			this.Techniques = cloneSource.Techniques;
		}

		public Effect(GraphicsDevice aGraphicsDevice, byte[] effectCode): this(aGraphicsDevice)
		{						
			int fragmentblocklength = BitConverter.ToInt32(effectCode, 0);

            int vertexblocklength = BitConverter.ToInt32(effectCode, fragmentblocklength + 4);

            if (fragmentblocklength != 0)
            {
                fragment_handle = GL.CreateShader( All.FragmentShader );
                fragment = true;
            }

            if (vertexblocklength != 0)
            {
                vertex_handle = GL.CreateShader( All.VertexShader );
                vertex = true;
            }

            if (fragment)
            {
                string[] fragmentstring = new string[1] { Encoding.UTF8.GetString(effectCode, 4, fragmentblocklength) };
                int[] fragmentLength = new int[1] { fragmentstring[0].Length };
                GL.ShaderSource(fragment_handle, 1, fragmentstring, fragmentLength);
            }

            if (vertex)
            {
                string[] vertexstring = new string[1] { Encoding.UTF8.GetString(effectCode, fragmentblocklength + 8, vertexblocklength) };
                int[] vertexLength = new int[1] { vertexstring[0].Length };
                GL.ShaderSource(vertex_handle, 1, vertexstring, vertexLength);
            }
			
			int compiled = 0;

            if (fragment)
            {
                GL.CompileShader(fragment_handle);
				
				GL.GetShader(fragment_handle, All.CompileStatus, ref compiled );
				if (compiled == (int)All.False)
				{
#if DEBUG					
					Console.Write("Fragment Compilation Failed!");
#endif
				}
            }

            if (vertex)
            {
                GL.CompileShader(vertex_handle);
				GL.GetShader(vertex_handle, All.CompileStatus, ref compiled );
				if (compiled == (int)All.False)
				{
#if DEBUG					
					Console.Write("Vertex Compilation Failed!");
#endif
				}
            }

		}
		
		internal Effect(GraphicsDevice aGraphicsDevice)
        {
            if (aGraphicsDevice == null)
            {
                throw new ArgumentNullException("Graphics Device Cannot Be Null");
            }
			this.graphicsDevice = aGraphicsDevice;
			
            Parameters = new EffectParameterCollection();
            Techniques = new EffectTechniqueCollection();
            CurrentTechnique = new EffectTechnique(this);
        }
		
		internal Effect (GraphicsDevice aGraphicsDevice, string aFileName) : this(aGraphicsDevice)
		{
#if ANDROID
			StreamReader streamReader = new StreamReader(Game.Activity.Assets.Open(aFileName));
#else			
			StreamReader streamReader = new StreamReader (aFileName);
#endif			
			string text = streamReader.ReadToEnd ();
			streamReader.Close ();
			
			if ( aFileName.ToLower().Contains("fsh") )
			{
				CreateFragmentShaderFromSource(text);
			}
			else if ( aFileName.ToLower().Contains("vsh") )
			{
				CreateVertexShaderFromSource(text);
			}			
			else
			{
				throw new ArgumentException( aFileName + " not supported!" );
			}
			
			DefineTechnique ("Technique1", "Pass1", 0, 0);
			CurrentTechnique = Techniques ["Technique1"];
		}

        internal virtual void Apply()
        {
     
        }
		
		protected internal virtual void OnApply ()
		{
			
		}
		
		protected void CreateVertexShaderFromSource(string source)
		{
			int shader = GL.CreateShader (All.VertexShader);
			// Attach the loaded source string to the shader object
			// TODO GL.ShaderSource(shader, source);
			// Compile the shader
			GL.CompileShader (shader);
			
			vertexShaders.Add(shader);			
		}
		
		
		protected void CreateFragmentShaderFromSource(string source)
		{
			int shader = GL.CreateShader (All.FragmentShader);
			// Attach the loaded source string to the shader object
			// TODO GL.ShaderSource (shader, source);
			// Compile the shader
			GL.CompileShader (shader);
			
			fragmentShaders.Add(shader);			
		}
		
		protected void DefineTechnique (string techniqueName, string passName, int vertexIndex, int fragmentIndex)
		{
			EffectTechnique tech = new EffectTechnique(this);
			tech.Name = techniqueName;
			EffectPass pass = new EffectPass(tech);
			pass.Name = passName;
#if ANDROID			
			pass.VertexIndex = vertexIndex;
			pass.FragmentIndex = fragmentIndex;
			pass.ApplyPass(); 
#endif			
			tech.Passes._passes.Add(pass);
			Techniques._techniques.Add(tech);
#if ANDROID			
			LogShaderParameters(String.Format("Technique {0} - Pass {1} :" ,tech.Name ,pass.Name), pass.shaderProgram);
#endif			
			
		}
		
		public void Begin()
		{
		}
		
		public void Begin(SaveStateMode saveStateMode)
		{
			
		}
		
		public virtual Effect Clone()
		{
			Effect ef = new Effect(this);
			return ef;
		}
		
		public void End()
		{
		}
		
		private static string TryFindAnyCased(string search, string[] arr, params string[] extensions)
        {
            return arr.FirstOrDefault(s => extensions.Any(ext => s.ToLower() == (search.ToLower() + ext)));
        }

        private static bool Contains(string search, string[] arr)
        {
            return arr.Any(s => s == search);
        }
		
		internal static string Normalize(string FileName)
		{
#if ANDROID
			int index = FileName.LastIndexOf(Path.DirectorySeparatorChar);
            string path = string.Empty;
            string file = FileName;
            if (index >= 0)
            {
                file = FileName.Substring(index + 1, FileName.Length - index - 1);
                path = FileName.Substring(0, index);
            }
            string[] files = Game.Activity.Assets.List(path);

            if (Contains(file, files))
                return FileName;
			
			// Check the file extension
			if (!string.IsNullOrEmpty(Path.GetExtension(FileName)))
			{
				return null;
			}
			
            return Path.Combine(path, TryFindAnyCased(file, files, ".fsh", ".vsh"));
#else			
			if (File.Exists(FileName))
				return FileName;
			
			// Check the file extension
			if (!string.IsNullOrEmpty(Path.GetExtension(FileName)))
			{
				return null;
			}
			
			// Concat the file name with valid extensions
			if (File.Exists(FileName+".fsh"))
				return FileName+".fsh";
			if (File.Exists(FileName+".vsh"))
				return FileName+".vsh";
			
			return null;
#endif			
		}
		
		public EffectTechnique CurrentTechnique 
		{ 
			get; set; 
		}
		
		private int GetUniformUserInedx(string uniformName)
		{
			int sPos = uniformName.LastIndexOf("_s");
			int index;
			
			// if there's no such construct on the string or it's not followed by numbers only
			if (sPos == -1 || !int.TryParse(uniformName.Substring(sPos + 2), out index))
			    return -1; // no user index
				
			return index;
		}
		
		// Output the log of an object
		private void LogShaderParameters (string whichObj, int obj)
		{
			int actUnis = 0;
			Parameters._parameters.Clear();
			
			GL.GetProgram (obj, All.ActiveUniforms, ref @actUnis);

			int size;
			All type = All.BoolVec2;
			string name = new StringBuilder(100).ToString();
			int length;
			
			for (int x =0; x < actUnis; x++) 
			{
				int uniformLocation, userIndex;
				string uniformName;
				size = length = 0;
				
				GL.GetActiveUniform((uint)obj,(uint)x,100,ref length,ref size, ref type, name);
				
				uniformName = name.ToString();
				
				userIndex = GetUniformUserInedx(uniformName);
				
				uniformLocation = GL.GetUniformLocation(obj, uniformName);
				
				EffectParameter efp = new EffectParameter(this, uniformName, x, userIndex, uniformLocation,
				                                          type.ToString(), length);
				Parameters._parameters.Add(efp.Name, efp);
				if (efp.ParameterType == EffectParameterType.Texture2D) {
					_textureMappings.Add(efp);
				}
			}
			
		}
	}
}
