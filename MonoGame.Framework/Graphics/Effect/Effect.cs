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
using System.Linq;
using System.Reflection;

#if IPHONE || ANDROID
using OpenTK.Graphics.ES20;
using ShaderType = OpenTK.Graphics.ES20.All;
using ActiveUniformType = OpenTK.Graphics.ES20.All;
using ProgramParameter = OpenTK.Graphics.ES20.All;
using ShaderParameter = OpenTK.Graphics.ES20.All;
#elif MONOMAC
using MonoMac.OpenGL;
#elif !WINRT
using OpenTK.Graphics.OpenGL;

#endif


namespace Microsoft.Xna.Framework.Graphics
{
	public class Effect : GraphicsResource
    {
        public EffectParameterCollection Parameters { get; set; }

        public EffectTechniqueCollection Techniques { get; set; }

        public EffectTechnique CurrentTechnique { get; set; }

        internal protected EffectParameter shaderIndexParam;

#if NOMOJO

        static internal Dictionary<int, GLSLShader[]> shaderObjectLookup = new Dictionary<int, GLSLShader[]>();

        internal static Dictionary<Type, int[]> programsByType = new Dictionary<Type, int[]>();
        internal int[] shaderIndexLookupTable;

#else

		DXEffectObject effectObject;
		GLSLEffectObject glslEffectObject;

        internal static Dictionary<byte[], DXEffectObject> effectObjectCache =
			new Dictionary<byte[], DXEffectObject>(new ByteArrayComparer());

		//cache effect objects so we don't create a bunch of instances
		//of the same shader
		//(Some programs create heaps of instances of BasicEffect,
		// which was causing ridiculous memory usage)
		private class ByteArrayComparer : IEqualityComparer<byte[]> {
			public bool Equals(byte[] left, byte[] right) {
				if ( left == null || right == null ) {
					return left == right;
				}
				return left.SequenceEqual(right);
			}
			public int GetHashCode(byte[] key) {
				if (key == null)
					throw new ArgumentNullException("key");
				return key.Sum(b => b);
			}
		}

#endif

#if !WINRT
        internal int CurrentProgram = 0;
#endif

        protected Effect (GraphicsDevice graphicsDevice)
		{
			if (graphicsDevice == null)
				throw new ArgumentNullException ("Graphics Device Cannot Be Null");

			this.graphicsDevice = graphicsDevice;
			Techniques = new EffectTechniqueCollection ();
			Parameters = new EffectParameterCollection();

            shaderIndexParam = new EffectParameter(ActiveUniformType.Int, "ShaderIndex");
		}
			
		protected Effect (Effect cloneSource)
		{
		}

#if NOMOJO

        internal Effect (GraphicsDevice graphicsDevice, string[] vertexShaderFilenames, string[] fragmentShaderFilenames, Tuple<int, int>[] programShaderIndexPairs)
            : this(graphicsDevice)
        {
            var type = this.GetType();
            if (!programsByType.TryGetValue(GetType(), out shaderIndexLookupTable))
                initializeEffects(vertexShaderFilenames, fragmentShaderFilenames, programShaderIndexPairs);
            
            shaderIndexParam.SetValue(0);
            Parameters.Add(shaderIndexParam);
        }

#else

		public Effect (GraphicsDevice graphicsDevice, byte[] effectCode)
            : this(graphicsDevice)
		{
			uint magic = BitConverter.ToUInt32(effectCode,0);

			//0xBCF00BCF XNA 4 effects
			//0xFEFF0901 effect too old or too new, or ascii which we can't compile atm
			//0x6151EFFE GLSL

			if (magic == 0x6151EFFE) {// GLSL shader is to be used from fxg file
				glslEffectObject = new GLSLEffectObject(effectCode);

				foreach (GLSLEffectObject.glslParameter parameter in glslEffectObject.parameter_handles) {
					Parameters._parameters.Add (new EffectParameter(parameter));
				}

				foreach (GLSLEffectObject.glslTechnique technique in glslEffectObject.technique_handles) {
					Techniques._techniques.Add (new EffectTechnique(this, technique));
				}
			}
			else {
				//try getting a cached effect object
				if (!effectObjectCache.TryGetValue(effectCode, out effectObject))
				{
					effectObject = new DXEffectObject(effectCode);
					effectObjectCache.Add (effectCode, effectObject);
				}
	
				foreach (DXEffectObject.d3dx_parameter parameter in effectObject.parameter_handles) {
					Parameters._parameters.Add (new EffectParameter(parameter));
				}
	
				foreach (DXEffectObject.d3dx_technique technique in effectObject.technique_handles) {
					Techniques._techniques.Add (new EffectTechnique(this, technique));
				}
			}

            CurrentTechnique = Techniques[0];			
		}

#endif

#if NOMOJO

        private void initializeEffects(string[] vertexShaderFilenames, string[] fragmentShaderFilenames, Tuple<int, int>[] programShaderIndexPairs)
        {
            var type = this.GetType();

            var programCount = programShaderIndexPairs.Length;

            shaderIndexLookupTable = new int[programCount];
            programsByType.Add(type, shaderIndexLookupTable);

            var vertexShaderCount = vertexShaderFilenames.Length;
            var vertexShaders = new GLSLShader[vertexShaderCount];
            for (var i = 0; i < vertexShaderCount; ++i)
                vertexShaders[i] = new GLSLShader(ShaderType.VertexShader, vertexShaderFilenames[i]);

            var fragmentShaderCount = fragmentShaderFilenames.Length;
            var fragmentShaders = new GLSLShader[fragmentShaderCount];
            for (var i = 0; i < fragmentShaderCount; ++i)
                fragmentShaders[i] = new GLSLShader(ShaderType.FragmentShader, fragmentShaderFilenames[i]);

            for (var i = 0; i < programCount; ++i)
            {
                var programShaderIndexPair = programShaderIndexPairs[i];
                shaderIndexLookupTable[i] = this.CreateProgram(vertexShaders[programShaderIndexPair.Item1], fragmentShaders[programShaderIndexPair.Item2]);
                shaderObjectLookup.Add(shaderIndexLookupTable[i], new GLSLShader[2] { vertexShaders[programShaderIndexPair.Item1], fragmentShaders[programShaderIndexPair.Item2] });
            }
        }

        private int CreateProgram(GLSLShader glVertexShader, GLSLShader glFragmentShader)
        {
            var glProgram = GL.CreateProgram();

            GL.AttachShader(glProgram, glVertexShader.shaderHandle);
            GL.AttachShader(glProgram, glFragmentShader.shaderHandle);

            GL.BindAttribLocation(glProgram, GraphicsDevice.attributePosition, "Position");
            GL.BindAttribLocation(glProgram, GraphicsDevice.attributeNormal, "Normal");
            GL.BindAttribLocation(glProgram, GraphicsDevice.attributeColor, "Color");
            GL.BindAttribLocation(glProgram, GraphicsDevice.attributeTexCoord, "TextureCoordinate");

            GL.LinkProgram(glProgram);

            var error = GL.GetError();

            int result = 0;
#if IPHONE || ANDROID
            GL.GetProgram(glProgram, ProgramParameter.LinkStatus, ref result);
#else
            GL.GetProgram(glProgram, ProgramParameter.LinkStatus, out result);
#endif // IPHONE || ANDROID
            if (result == 0)
            {
                var maxInfoLogLength = 0;
#if IPHONE|| ANDROID
                GL.GetProgram(glProgram, ProgramParameter.InfoLogLength, ref maxInfoLogLength);
#else
                GL.GetProgram(glProgram, ProgramParameter.InfoLogLength, out maxInfoLogLength);
#endif //IPHONE || ANDROID
                if (maxInfoLogLength > 0)
                {
                    var infoLogLength = 0;
                    var infoLog = new StringBuilder(maxInfoLogLength);
#if IPHONE || ANDROID
                    GL.GetProgramInfoLog(glProgram, maxInfoLogLength, ref infoLogLength, infoLog);
#else
                    GL.GetProgramInfoLog(glProgram, maxInfoLogLength, out infoLogLength, infoLog);
#endif // IPHONE || ANDROID
                    System.Diagnostics.Debug.WriteLine(infoLog.ToString());
                }
            }

            return glProgram;
        }

#else

		internal static byte[] LoadEffectResource(string name)
		{
            var assembly = typeof(Effect).Assembly;

#if GLSL_EFFECTS
            name += "GLSL.bin";
#else
            name += ".bin";
#endif
            var stream = assembly.GetManifestResourceStream("Microsoft.Xna.Framework.Graphics.Effect." + name);
            using (MemoryStream ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				return ms.ToArray();
			}
		}

#endif

        internal virtual void Initialize()
        {
        }

		public virtual Effect Clone ()
		{
			throw new NotImplementedException();
		}

		public void End ()
		{
		}

		protected internal virtual void OnApply ()
		{
		}
	}
}
