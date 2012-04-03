using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
using System.Reflection;
using System.IO;
#else
using System.Text;
using OpenTK.Graphics.ES20;
#if IPHONE || ANDROID
using ProgramParameter = OpenTK.Graphics.ES20.All;
using ShaderType = OpenTK.Graphics.ES20.All;
using ShaderParameter = OpenTK.Graphics.ES20.All;
using TextureUnit = OpenTK.Graphics.ES20.All;
using TextureTarget = OpenTK.Graphics.ES20.All;
#endif
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	internal class GLSLShader
	{
		public ShaderType shaderType;
		public int shaderHandle;
		public GLSLEffectObject.VertexAttributeInfo[] attributes;
		public GLSLEffectObject.SamplerIndexInfo[] samplerIndices;
		string glslCode;
        string shaderPath;
		const string GLSL_DESKTOP = "#define DESKTOP\n";

        public List<String> attributeList;
        public List<String> uniformsList;

		float[] uniforms_float4;
		int[] uniforms_int4;
		int[] uniforms_bool;
		int uniforms_float4_count = 0;
		int uniforms_int4_count = 0;
		int uniforms_bool_count = 0;
		
//		MojoShader.MOJOSHADER_symbol[] symbols;
//		MojoShader.MOJOSHADER_sampler[] samplers;
//		MojoShader.MOJOSHADER_attribute[] attributes;
//		
//		DXPreshader preshader;

#if NOMOJO

        public GLSLShader(ShaderType shadertype, string filePath)
        {
            // First decide if this is a fragment or pixel shader.

            shaderPath = filePath;
            glslCode = GetShaderFromAssembly(filePath);

            shaderType = shadertype;

            shaderHandle = GL.CreateShader(shaderType);
            // Attach the loaded source string to the shader object
            GL.ShaderSource(shaderHandle, glslCode);
            // Compile the shader
            GL.CompileShader(shaderHandle);

            int compiled = 0;
            //Error check.
            GL.GetShader(shaderHandle, ShaderParameter.CompileStatus, out compiled);
            if (compiled == (int)All.False)
            {
                string log = GL.GetShaderInfoLog(shaderHandle);
                Console.WriteLine(log);
                throw new ArgumentNullException();
            }

            // Shader looks good. Lets grab the variables we'll need from it.

            // Read in and save all of our attribute names to bind them later.
            if (shaderType == ShaderType.VertexShader)
            {
                attributeList = new List<String>();
                buildVariablesList("attribute", attributeList);
            }

            // Build our uniforms.
            if (shaderType == ShaderType.VertexShader || shaderType == ShaderType.FragmentShader)
            {
                uniformsList = new List<String>();
                buildVariablesList("uniform", uniformsList);
            }
            else
                throw new NotSupportedException();
        }

        private void buildVariablesList(string glslVariableType, List<String> variableList)
        {
            // Because OpenGL ES2.0 have precision specifiers, we have to jump around the strings a bit. =/
            string nameOfAttribute;
            var includes = glslCode.Substring(0, glslCode.IndexOf("void main"));
            for (int curPos = includes.IndexOf(glslVariableType); curPos != -1 && curPos < includes.Length; )
            {
                // Find the attribute name.
                var nextSemicolonIndex = includes.IndexOf(';', curPos);
                var length = nextSemicolonIndex - curPos;

                var spaceIndex = includes.LastIndexOf(' ', nextSemicolonIndex);
                nameOfAttribute = includes.Substring(spaceIndex, nextSemicolonIndex - spaceIndex).Trim();

                // Find the type name.
                var nextSpaceIndex = includes.LastIndexOf(' ', spaceIndex - 1);
                var typeOfAttribute = includes.Substring(nextSpaceIndex, spaceIndex - nextSpaceIndex).Trim();

                variableList.Add(nameOfAttribute);

                curPos = includes.IndexOf(glslVariableType, nextSemicolonIndex);
            }
        }

        private string GetShaderFromAssembly(string filePath)
        {
            Assembly assembly = typeof(Effect).Assembly;//Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(filePath);
            StreamReader streamReader = new StreamReader(stream);
            var shaderCode = streamReader.ReadToEnd();
            streamReader.Close();

            return shaderCode;
        }

#endif
		
		public GLSLShader (GLSLEffectObject.ShaderProg shaderProgram)
		{
//			IntPtr parseDataPtr = MojoShader.NativeMethods.MOJOSHADER_parse(
//					"glsl",
//					shaderData,
//					shaderData.Length,
//					IntPtr.Zero,
//					0,
//					IntPtr.Zero,
//					IntPtr.Zero,
//					IntPtr.Zero);
//
//			MojoShader.MOJOSHADER_parseData parseData =
//				(MojoShader.MOJOSHADER_parseData)Marshal.PtrToStructure(
//					parseDataPtr,
//					typeof(MojoShader.MOJOSHADER_parseData));
//
//			if (parseData.error_count > 0) {
//				MojoShader.MOJOSHADER_error[] errors =
//					DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_error>(
//						parseData.errors, parseData.error_count);
//				throw new Exception(errors[0].error);
//			}
//
//			if (parseData.preshader != IntPtr.Zero) {
//				preshader = new DXPreshader(parseData.preshader);
//			}

			switch(shaderProgram.shaderType) {
			case GLSLEffectObject.glslEffectParameterType.PixelShader:
				shaderType = ShaderType.FragmentShader;
				break;
			case GLSLEffectObject.glslEffectParameterType.VertexShader:
				shaderType = ShaderType.VertexShader;
				break;
			default:
				throw new NotSupportedException();
			}

//			symbols = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_symbol>(
//					parseData.symbols, parseData.symbol_count);
//
//			Array.Sort (symbols, (a, b) => a.register_index.CompareTo(b.register_index));
//
//			samplers = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_sampler>(
//					parseData.samplers, parseData.sampler_count);
//
//			attributes = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_attribute>(
//					parseData.attributes, parseData.attribute_count);
//
//			foreach (MojoShader.MOJOSHADER_symbol symbol in symbols) {
//				switch (symbol.register_set) {
//				case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL:
//					uniforms_bool_count += (int)symbol.register_count;
//					break;
//				case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4:
//					uniforms_float4_count += (int)symbol.register_count;
//					break;
//				case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_INT4:
//					uniforms_int4_count += (int)symbol.register_count;
//					break;
//				default:
//					break;
//				}
//			}

			uniforms_float4 = new float[uniforms_float4_count*4];
			uniforms_int4 = new int[uniforms_int4_count*4];
			uniforms_bool = new int[uniforms_bool_count];

#if IPHONE || ANDROID
			glslCode = shaderProgram.shaderCode;
#else
			glslCode = GLSL_DESKTOP + shaderProgram.shaderCode;
#endif

#if ES11
			if (shaderType == ShaderType.VertexShader) {
				foreach (MojoShader.MOJOSHADER_attribute attrb in attributes) {
					switch (attrb.usage) {

					//use builtin attributes in GL 1.1
					case MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_COLOR:
						glslCode = glslCode.Replace ("attribute vec4 "+attrb.name+";",
						                               "#define "+attrb.name+" gl_Color");
						break;
					case MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_POSITION:
						glslCode = glslCode.Replace ("attribute vec4 "+attrb.name+";",
						                               "#define "+attrb.name+" gl_Vertex");
						break;
					case MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_TEXCOORD:
						glslCode = glslCode.Replace ("attribute vec4 "+attrb.name+";",
						                               "#define "+attrb.name+" gl_MultiTexCoord0");
						break;
					case MojoShader.MOJOSHADER_usage.MOJOSHADER_USAGE_NORMAL:
						glslCode = glslCode.Replace ("attribute vec4 "+attrb.name+";",
						                               "#define "+attrb.name+" gl_Normal");
						break;
					default:
						throw new NotImplementedException();
					}
				}
			}
#endif
			
			//glslCode = GLSLOptimizer.Optimize (glslCode, shaderType);
			
#if IPHONE || ANDROID
			glslCode = glslCode.Replace("#version 110\n", "");
			glslCode = @"precision mediump float;
						 precision mediump int;
						"+glslCode;
#endif

            shaderHandle = GL.CreateShader(shaderType);
#if IPHONE || ANDROID
			GL.ShaderSource (shader, 1, new string[]{glslCode}, (int[])null);
#else			
            GL.ShaderSource(shaderHandle, glslCode);
#endif
            GL.CompileShader(shaderHandle);
			
			int compiled = 0;
#if IPHONE || ANDROID
			GL.GetShader (shader, ShaderParameter.CompileStatus, ref compiled);
#else
            GL.GetShader(shaderHandle, ShaderParameter.CompileStatus, out compiled);
#endif
			if (compiled == (int)All.False) {
#if IPHONE || ANDROID
				string log = "";
				int length = 0;
				GL.GetShader (shader, ShaderParameter.InfoLogLength, ref length);
				if (length > 0) {
					var logBuilder = new StringBuilder(length);
					GL.GetShaderInfoLog(shader, length, ref length, logBuilder);
					log = logBuilder.ToString();
				}
#else
                string log = GL.GetShaderInfoLog(shaderHandle);
#endif
				Console.WriteLine (log);

                GL.DeleteShader(shaderHandle);
				throw new InvalidOperationException("Shader Compilation Failed");
			}
				
		}

		public void OnLink(int program) {
#if !ES11
			if (shaderType == ShaderType.VertexShader) {
				//bind attributes
				foreach (GLSLEffectObject.VertexAttributeInfo attInfo in attributes) {
					switch (attInfo.Type) {
					case GLSLEffectObject.VertexAttributeType.Position:
						GL.BindAttribLocation(program, GraphicsDevice.attributePosition, attInfo.Name);
						break;
					case GLSLEffectObject.VertexAttributeType.Normal:
						GL.BindAttribLocation(program, GraphicsDevice.attributeNormal, attInfo.Name);
						break;
					case GLSLEffectObject.VertexAttributeType.Color:
						GL.BindAttribLocation(program, GraphicsDevice.attributeColor, attInfo.Name);
						break;
					case GLSLEffectObject.VertexAttributeType.TexCoord:
						GL.BindAttribLocation(program, GraphicsDevice.attributeTexCoord, attInfo.Name);
						break;
//					case GLSLEffectObject.VertexAttributeType.TexCoord2:
//						GL.BindAttribLocation(program, 8, attInfo.Name);
//					case GLSLEffectObject.VertexAttributeType.BlendIndices:
//						GL.BindAttribLocation(program, GraphicsDevice.attributeBlendIndicies, attInfo.Name);
//					case GLSLEffectObject.VertexAttributeType.BlendWeight:
//						GL.BindAttribLocation(program, GraphicsDevice.attributeBlendWeight, attInfo.Name);

					}
				}

			}
#endif
		}

		private class ProgramInfo
		{
			public int Name { get; set; }
			public UniformInfo[] Uniforms { get; set; }
		}
		
		private class UniformInfo
		{
			public int Location { get; set; }
			public int EffectParameterIndex { get; set; }
			public string Name { get; set; }
#if IPHONE || ANDROID
			public All GLSLType {get; set; }
#else
			public ActiveUniformType GLSLType { get;  set; }
#endif
			//public EffectParameterValue Value { get; set; }
		}

#if NOMOJO
        public void Apply(int program,
                  EffectParameterCollection parameters,
                  GraphicsDevice graphicsDevice)
        {
            TextureCollection textures = graphicsDevice.Textures;
            SamplerStateCollection samplerStates = graphicsDevice.SamplerStates;

            // Relink our attributes.
            if (shaderType == ShaderType.VertexShader)
            {
                int location = 0;
                //bind attributes
                foreach (var attribute in attributeList)
                {
                    if (attribute.Contains("Position"))
                        location = GraphicsDevice.attributePosition;
                    else if (attribute.Contains("Normal"))
                        location = GraphicsDevice.attributeNormal;
                    else if (attribute.Contains("TextureCoordinate"))
                        location = GraphicsDevice.attributeTexCoord;
                    else if (attribute.Contains("Color"))
                        location = GraphicsDevice.attributeColor;
                    else
                        throw new NotSupportedException();

                    GL.BindAttribLocation(program, location, attribute);
                }
            }

            // Set our uniforms.
            foreach (var param in parameters)
            {
                int uniformLocation = GL.GetUniformLocation(program, param.Name);

                var error = GL.GetError();

                if (uniformLocation == -1 || param.Name.Contains("ShaderIndex"))
                    continue;

                // Only set uniforms if they exist for this shader.
                if (!uniformsList.Contains(param.Name))
                    continue;

                switch (param.activeUniformType)
                {
                    case ActiveUniformType.FloatVec2:
                        var v2 = param.GetValueVector2();
                        GL.Uniform2(uniformLocation, v2.X, v2.Y);
                        break;

                    case ActiveUniformType.FloatVec3:
                        var v3 = param.GetValueVector3();
                        GL.Uniform3(uniformLocation, v3.X, v3.Y, v3.Z);
                        break;

                    case ActiveUniformType.FloatVec4:
                        var v4 = param.GetValueVector4();
                        GL.Uniform4(uniformLocation, v4.X, v4.Y, v4.Z, v4.W);
                        break;

                    case ActiveUniformType.Float:
                        GL.Uniform1(uniformLocation, param.GetValueSingle());
                        break;

                    case ActiveUniformType.FloatMat4:
                        var mat4 = (Matrix)param.data;
                        GL.UniformMatrix4(uniformLocation, 1, false, ref mat4.M11);
                        break;

                    case ActiveUniformType.FloatMat3:
                        float[] mat3 = (float[])param.data;
                        GL.UniformMatrix3(uniformLocation, 1, false, ref mat3[0]);
                        break;

                    case ActiveUniformType.Bool:
                        GL.Uniform1(uniformLocation, (param.GetValueBoolean()) ? 1 : 0);
                        break;

                    case ActiveUniformType.Int:
                        GL.Uniform1(uniformLocation, param.GetValueInt32());
                        break;

                    case ActiveUniformType.Sampler2D:
                        var samplerIndex = (int)TextureTarget.Texture2D;
                        GL.Uniform1(uniformLocation, samplerIndex);
                        Texture tex = null;
						tex = (Texture)param.data;
						if (tex == null) 
                        {
							//texutre 0 will be set in drawbatch :/
                            if (samplerIndex == 0)
								continue;

							//are smapler indexes always normal texture indexes?
                            tex = (Texture)textures[samplerIndex];
						}

                        GL.ActiveTexture((TextureUnit)((int)TextureUnit.Texture0 + samplerIndex));
						tex.Activate();
                        samplerStates[samplerIndex].Activate(tex.glTarget, tex.LevelCount > 1);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }
#else

		private float[] matrix3 = new float[9];
		public void Apply(int program,
		                  EffectParameterCollection parameters,
		                  GraphicsDevice graphicsDevice) {


			Viewport vp = graphicsDevice.Viewport;
			TextureCollection textures = graphicsDevice.Textures;
			SamplerStateCollection samplerStates = graphicsDevice.SamplerStates;

			//Populate the uniform register arrays
			//TODO: not necessarily packed contiguously, get info from mojoshader somehow
			int bool_index = 0;
			int float4_index = 0;
			int int4_index = 0;

			int numUniforms = 0;
#if MONOMAC || WINDOWS || LINUX
			GL.GetProgram(program, ProgramParameter.ActiveUniforms, out numUniforms);
#else
			GL.GetProgram(program, ProgramParameter.ActiveUniforms, ref numUniforms);
#endif
			for (int index = 0; index < numUniforms; index++) {
				int size = 0;
				int length = 0;
#if IPHONE || ANDROID
				All type = 0;
				String str = String.Empty;
				GL.GetActiveUniform(program, index, 255, ref length, ref size, ref type, str);
#else
				ActiveUniformType type;
				StringBuilder sb2 = new StringBuilder();
				GL.GetActiveUniform(program, index, 255, out length, out size, out type, sb2);
#endif
			}

			//var parameters = new List<EffectParameter>();
			var parameterIndexByName = new Dictionary<string, int>();
//			for (var i = 0; i < programCount; ++i)
//			{

				//var program = this.programs[i];
				var uniformCount = 0;
#if IPHONE || ANDROID
				GL.GetProgram(program, All.ActiveUniforms, ref uniformCount);
#else
				GL.GetProgram (program, ProgramParameter.ActiveUniforms, out uniformCount);

#endif
				//program.Uniforms = new UniformInfo[uniformCount];
				UniformInfo[] uniforms = new UniformInfo[uniformCount];
				for (var iUniform = 0; iUniform < uniformCount; ++iUniform)
				{
					var uniform = new UniformInfo();
					uniforms[iUniform] = uniform;
					int length = 0;
					int size = 0;
#if IPHONE || ANDROID
					All glType = 0;
					String uniformName = String.Empty;
					GL.GetActiveUniform(program, iUniform, 256, ref length, ref size, ref glType, uniformName);
#else
					ActiveUniformType glType = 0;
					var uniformNameStringBuilder = new StringBuilder(256);
					GL.GetActiveUniform(program, iUniform, 256, out length, out size, out glType, uniformNameStringBuilder);
					var uniformName = uniformNameStringBuilder.ToString();
#endif
					
					var uniformLocation = GL.GetUniformLocation(program, uniformName);
					var parameterIndex = -1;
					if (!parameterIndexByName.TryGetValue(uniformName, out parameterIndex))
					{
						parameterIndex = parameters.Count;
						parameterIndexByName.Add(uniformName, parameterIndex);
						//parameters.Add(new EffectParameter(this, glType, uniformName));
					}
					uniform.EffectParameterIndex = parameterIndex;
					uniform.Location = uniformLocation;
					uniform.Name = uniformName;
					uniform.GLSLType = glType;
					//uniform.Value = default(EffectParameterValue); // TODO: retrieve actual default value from program
				}
//			}
			//if (shaderType == ShaderType.VertexShader)
			for (int i = 0; i < uniforms.Length; ++i)
			{
				var uniform = uniforms[i];
				var parameter = parameters[uniform.Name];
				// We check here if a parameter is found or not.  An example where this could be null is where
				//  we have the posFixup code for RenderTarget y-axis flipping which really does not have
				//  an EffectParameter associatied as it is internally added.
				if (parameter == null)
					continue;

#if IPHONE || ANDROID
				switch (uniform.GLSLType)
				{
				case All.Float :
				{
					// if (parameter.value.Float != uniform.Value.Float)
						GL.Uniform1(uniform.Location, parameter.GetValueSingle());
					break;
				}
				case All.Int:
				{
					//if (parameter.value.Int != uniform.Value.Int)
						GL.Uniform1(uniform.Location, parameter.GetValueInt32());
					break;
				}
				case All.Bool:
				{
					//if (parameter.value.Bool != uniform.Value.Bool)
						GL.Uniform1(uniform.Location, parameter.GetValueBoolean() ? 1 : 0);
					break;
				}
				case All.FloatVec2:
				{
					//if (parameter.value.Vector2 != uniform.Value.Vector2)
						Vector2 val2 = parameter.GetValueVector2();
						GL.Uniform2(uniform.Location, 1, ref val2.X);
					break;
				}
				case All.FloatVec3:
				{
					//if (parameter.value.Vector3 != uniform.Value.Vector3)
						Vector3 val3 = parameter.GetValueVector3();
						GL.Uniform3(uniform.Location, 1, ref val3.X);
					break;
				}
				case All.FloatVec4:
				{
					//if (parameter.value.Vector4 != uniform.Value.Vector4)
						Vector4 val4 = parameter.GetValueVector4();
						GL.Uniform4(uniform.Location, 1, ref val4.X);
					break;
				}
				//case All.IntVec2:
				//case All.IntVec3:
				//case All.IntVec4:
				//case All.BoolVec2:
				//case All.BoolVec3:
				//case All.BoolVec4:
				//case All.FloatMat2:
				case All.FloatMat3:
				{
					// if (parameter.value.Matrix != uniform.Value.Matrix)
					{
						this.matrix3[0] = parameter.GetValueMatrix().M11;
						this.matrix3[1] = parameter.GetValueMatrix().M12;
						this.matrix3[2] = parameter.GetValueMatrix().M13;
						this.matrix3[3] = parameter.GetValueMatrix().M21;
						this.matrix3[4] = parameter.GetValueMatrix().M22;
						this.matrix3[5] = parameter.GetValueMatrix().M23;
						this.matrix3[6] = parameter.GetValueMatrix().M31;
						this.matrix3[7] = parameter.GetValueMatrix().M32;
						this.matrix3[8] = parameter.GetValueMatrix().M33;
						GL.UniformMatrix3(uniform.Location, 1, false, this.matrix3);
					}
					break;
				}
				case All.FloatMat4:
				{
					//if (parameter.value.Matrix != uniform.Value.Matrix)
						float[] mat4 = (float[])parameter.data;
						GL.UniformMatrix4(uniform.Location, 1, true, ref mat4[0]);
					break;
				}
				case All.Sampler2D:
				{
					var sampler = new GLSLEffectObject.SamplerIndexInfo();

					foreach (var sii in samplerIndices) {
						if (parameter.Name == sii.Name) {
							sampler = sii;
							break;
						}

					}
					//this.GraphicsDevice.Textures[usageIndex] = (Texture2D)parameter.GetValueTexture2D();
					//Texture tex = (Texture)parameter.data;
					//this.GraphicsDevice.Textures[parameter.usageIndex] = (Texture2D)parameter.GetValueTexture2D();
//					if (!object.ReferenceEquals(parameter.value.Object, uniform.Value.Object))
						GL.Uniform1(uniform.Location, sampler.Index);
					//int loc = GL.GetUniformLocation (program, sampler.name);
					
					//set the sampler texture slot
					//GL.Uniform1 (loc, sampler.index);
					
//					if (sampler.type == MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_2D) {
//						MojoShader.MOJOSHADER_symbol? samplerSymbol = null;
//						foreach (MojoShader.MOJOSHADER_symbol symbol in symbols) {
//							if (symbol.register_set ==
//							    	MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_SAMPLER &&
//							    symbol.register_index == sampler.index) {
//
//								samplerSymbol = symbol;
//								break;
//							}
//						}

						Texture tex = null;
//						if (samplerSymbol.HasValue) {
//							DXEffectObject.d3dx_sampler samplerState =
//								(DXEffectObject.d3dx_sampler)parameters[samplerSymbol.Value.name].data;
//							if (samplerState.state_count > 0) {
//								string textureName = samplerState.states[0].parameter.name;
//								EffectParameter textureParameter = parameters[textureName];
//								if (textureParameter != null) {
//									tex = (Texture)textureParameter.data;
//								}
//							}
//						}
						tex = (Texture)parameter.data;
						if (tex == null) {
							//texutre 0 will be set in drawbatch :/
							if (sampler.Index == 0) {
								continue;
							}
							//are smapler indexes always normal texture indexes?
							tex = (Texture)textures [sampler.Index];
						}

						GL.ActiveTexture( (TextureUnit)((int)TextureUnit.Texture0 + sampler.Index) );

						tex.Activate ();
						
						samplerStates[sampler.Index].Activate(tex.glTarget);
					break;
				}
	//				case All.SamplerCube:
	//					GL.ActiveTexture(All.Texture0 + textureUnit);
	//					var textureCube = parameter.GetValueTextureCube();
	//					GL.Uniform1(uniform.Location, textureUnit++);
	//					break;
				default : throw new NotSupportedException();
				}
				//uniform.Value = parameter.value;
			//}
#else
				switch (uniform.GLSLType)
				{
				case ActiveUniformType.Float :

					//if (parameter.value.Float != uniform.Value.Float)
						GL.Uniform1(uniform.Location, parameter.GetValueSingle());
					break;

				case ActiveUniformType.Int:

					//if (parameter.value.Int != uniform.Value.Int)
						GL.Uniform1(uniform.Location, parameter.GetValueInt32());
					break;

				case ActiveUniformType.Bool:

					//if (parameter.value.Bool != uniform.Value.Bool)
						GL.Uniform1(uniform.Location, (parameter.GetValueBoolean()) ? 1 : 0);
					break;

				case ActiveUniformType.FloatVec2:

					//if (parameter.value.Vector2 != uniform.Value.Vector2)
						Vector2 val2 = parameter.GetValueVector2();
						GL.Uniform2(uniform.Location, 1, ref val2.X);
					break;

				case ActiveUniformType.FloatVec3:

					//if (parameter.value.Vector3 != uniform.Value.Vector3)
						Vector3 val3 = parameter.GetValueVector3();
						GL.Uniform3(uniform.Location, 1, ref val3.X);
					break;

				case ActiveUniformType.FloatVec4:

					//if (parameter.value.Vector4 != uniform.Value.Vector4)
						Vector4 val4 = parameter.GetValueVector4();
						GL.Uniform4(uniform.Location, 1, ref val4.X);
					break;

				//case All.IntVec2:
				//case All.IntVec3:
				//case All.IntVec4:
				//case All.BoolVec2:
				//case All.BoolVec3:
				//case All.BoolVec4:
				//case All.FloatMat2:
				case ActiveUniformType.FloatMat3:

					//if (parameter.value.Matrix != uniform.Value.Matrix)
					//{
						this.matrix3[0] = parameter.GetValueMatrix().M11;
						this.matrix3[1] = parameter.GetValueMatrix().M12;
						this.matrix3[2] = parameter.GetValueMatrix().M13;
						this.matrix3[3] = parameter.GetValueMatrix().M21;
						this.matrix3[4] = parameter.GetValueMatrix().M22;
						this.matrix3[5] = parameter.GetValueMatrix().M23;
						this.matrix3[6] = parameter.GetValueMatrix().M31;
						this.matrix3[7] = parameter.GetValueMatrix().M32;
						this.matrix3[8] = parameter.GetValueMatrix().M33;
						GL.UniformMatrix3(uniform.Location, 1, false, this.matrix3);
					//}
					break;

				case ActiveUniformType.FloatMat4:

					//if (parameter.value.Matrix != uniform.Value.Matrix)
						float[] mat4 = (float[])parameter.data;
						GL.UniformMatrix4(uniform.Location, 1, true,
					                  ref mat4[0]);
					break;

				case ActiveUniformType.Sampler2D:
					//int usageIndex = 0;
					var sampler = new GLSLEffectObject.SamplerIndexInfo();

					foreach (var sii in samplerIndices) {
						if (parameter.Name == sii.Name) {
							sampler = sii;
							break;
						}

					}
					//this.GraphicsDevice.Textures[usageIndex] = (Texture2D)parameter.GetValueTexture2D();
					//Texture tex = (Texture)parameter.data;
					//this.GraphicsDevice.Textures[parameter.usageIndex] = (Texture2D)parameter.GetValueTexture2D();
//					if (!object.ReferenceEquals(parameter.value.Object, uniform.Value.Object))
						GL.Uniform1(uniform.Location, sampler.Index);
					//int loc = GL.GetUniformLocation (program, sampler.name);
					
					//set the sampler texture slot
					//GL.Uniform1 (loc, sampler.index);
					
//					if (sampler.type == MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_2D) {
//						MojoShader.MOJOSHADER_symbol? samplerSymbol = null;
//						foreach (MojoShader.MOJOSHADER_symbol symbol in symbols) {
//							if (symbol.register_set ==
//							    	MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_SAMPLER &&
//							    symbol.register_index == sampler.index) {
//
//								samplerSymbol = symbol;
//								break;
//							}
//						}

						Texture tex = null;
//						if (samplerSymbol.HasValue) {
//							DXEffectObject.d3dx_sampler samplerState =
//								(DXEffectObject.d3dx_sampler)parameters[samplerSymbol.Value.name].data;
//							if (samplerState.state_count > 0) {
//								string textureName = samplerState.states[0].parameter.name;
//								EffectParameter textureParameter = parameters[textureName];
//								if (textureParameter != null) {
//									tex = (Texture)textureParameter.data;
//								}
//							}
//						}
						tex = (Texture)parameter.data;
						if (tex == null) {
							//texutre 0 will be set in drawbatch :/
							if (sampler.Index == 0) {
								continue;
							}
							//are smapler indexes always normal texture indexes?
							tex = (Texture)textures [sampler.Index];
						}

						GL.ActiveTexture( (TextureUnit)((int)TextureUnit.Texture0 + sampler.Index) );

						tex.Activate ();
						
						samplerStates[sampler.Index].Activate(tex.glTarget, tex.LevelCount > 1);
					break;

	//				case All.SamplerCube:
	//					GL.ActiveTexture(All.Texture0 + textureUnit);
	//					var textureCube = parameter.GetValueTextureCube();
	//					GL.Uniform1(uniform.Location, textureUnit++);
	//					break;
				default : throw new NotSupportedException();
				}
				//uniform.Value = parameter.value;
			//}

#endif
			}
			//TODO: only populate modified stuff?
//			foreach (MojoShader.MOJOSHADER_symbol symbol in symbols) {
//				//todo: support array parameters
//				EffectParameter parameter = parameters[symbol.name];
//				switch (symbol.register_set) {
//				case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL:
//					if (parameter.Elements.Count > 0) {
//						throw new NotImplementedException();
//					}
//					uniforms_bool[bool_index*4] = (int)parameter.data;
//					bool_index += (int)symbol.register_count;
//					break;
//				case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4:
//
//					Single[] data = parameter.GetValueSingleArray();
//
//					switch (parameter.ParameterClass) {
//					case EffectParameterClass.Scalar:
//						if (parameter.Elements.Count > 0) {
//							throw new NotImplementedException();
//						}
//						for (int i=0; i<data.Length; i++) {
//							uniforms_float4[float4_index*4+i] = (float)data[i];
//						}
//						break;
//					case EffectParameterClass.Vector:
//					case EffectParameterClass.Matrix:
//						long rows = Math.Min (symbol.register_count, parameter.RowCount);
//						if (parameter.Elements.Count > 0) {
//							rows = Math.Min (symbol.register_count, parameter.Elements.Count*parameter.RowCount);
//						}
//						for (int y=0; y<rows; y++) {
//							for (int x=0; x<parameter.ColumnCount; x++) {
//								uniforms_float4[(float4_index+y)*4+x] = (float)data[y*parameter.ColumnCount+x];
//							}
//						}
//						break;
//					default:
//						throw new NotImplementedException();
//					}
//					float4_index += (int)symbol.register_count;
//					break;
//				case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_INT4:
//					throw new NotImplementedException();
//				case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_SAMPLER:
//					break; //handled by ActivateTextures
//				default:
//					throw new NotImplementedException();
//				}
//			}

			//execute the preshader
//			if (preshader != null) {
//				preshader.Run (parameters, uniforms_float4);
//			}
			
			//Upload the uniforms
//			string prefix;
//			switch(shaderType) {
//			case ShaderType.FragmentShader:
//				prefix = "ps";
//				break;
//			case ShaderType.VertexShader:
//				prefix = "vs";
//				break;
//			default:
//				throw new NotImplementedException();
//			}
//
//
//			if (uniforms_float4_count > 0) {
//				int vec4_loc = GL.GetUniformLocation (program, prefix+"_uniforms_vec4");
//				GL.Uniform4 (vec4_loc, uniforms_float4_count, uniforms_float4);
//			}
//			if (uniforms_int4_count > 0) {
//				int int4_loc = GL.GetUniformLocation (program, prefix+"_uniforms_ivec4");
//				GL.Uniform4 (int4_loc, uniforms_int4_count, uniforms_int4);
//			}
//			if (uniforms_bool_count > 0) {
//				int bool_loc = GL.GetUniformLocation (program, prefix+"_uniforms_bool");
//				GL.Uniform1 (bool_loc, uniforms_bool_count, uniforms_bool);
//			}
//
//			if (shaderType == ShaderType.FragmentShader) {
//				//activate textures
//				foreach (MojoShader.MOJOSHADER_sampler sampler in samplers) {
//					int loc = GL.GetUniformLocation (program, sampler.name);
//
//					//set the sampler texture slot
//					GL.Uniform1 (loc, sampler.index);
//
//					if (sampler.type == MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_2D) {
//						MojoShader.MOJOSHADER_symbol? samplerSymbol = null;
//						foreach (MojoShader.MOJOSHADER_symbol symbol in symbols) {
//							if (symbol.register_set ==
//							    	MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_SAMPLER &&
//							    symbol.register_index == sampler.index) {
//
//								samplerSymbol = symbol;
//								break;
//							}
//						}
//
//						Texture tex = null;
//						if (samplerSymbol.HasValue) {
//							DXEffectObject.d3dx_sampler samplerState =
//								(DXEffectObject.d3dx_sampler)parameters[samplerSymbol.Value.name].data;
//							if (samplerState.state_count > 0) {
//								string textureName = samplerState.states[0].parameter.name;
//								EffectParameter textureParameter = parameters[textureName];
//								if (textureParameter != null) {
//									tex = (Texture)textureParameter.data;
//								}
//							}
//						}
//						if (tex == null) {
//							//texutre 0 will be set in drawbatch :/
//							if (sampler.index == 0) {
//								continue;
//							}
//							//are smapler indexes always normal texture indexes?
//							tex = (Texture)textures [sampler.index];
//						}
//
//						GL.ActiveTexture( (TextureUnit)((int)TextureUnit.Texture0 + sampler.index) );
//
//						tex.Apply ();
//
//						samplerStates[sampler.index].Activate(tex.GLTarget);
//
//					}
//
//				}
//			}
		}
#endif

	}
}

