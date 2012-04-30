#if GLSLOPTIMIZER

using System;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES20;
#if IPHONE || ANDROID
using ShaderType = OpenTK.Graphics.ES20.All;
#endif
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	internal class GLSLOptimizer
	{
#if IPHONE
		const string libglsl_optmizer_dll = "__Internal";
#elif ANDROID
		const string libglsl_optmizer_dll = "libglsl_optimizer.so";
#else
		const string libglsl_optmizer_dll = "libglsl_optimizer.dll";
#endif
		
		enum glslopt_shader_type {
			kGlslOptShaderVertex = 0,
			kGlslOptShaderFragment,
		};
		
		// Options flags for glsl_optimize
		enum glslopt_options {
			kGlslOptionSkipPreprocessor = (1<<0), // Skip preprocessing shader source. Saves some time if you know you don't need it.
			kGlslOptionNotFullShader = (1<<1), // Passed shader is not the full shader source. This makes some optimizations weaker.
		};
		
		[DllImportAttribute(libglsl_optmizer_dll, EntryPoint="_Z18glslopt_initializeb")]
		static extern IntPtr glslopt_initialize(bool openglES);
		
		[DllImportAttribute(libglsl_optmizer_dll, EntryPoint="_Z15glslopt_cleanupP11glslopt_ctx")]
		static extern IntPtr glslopt_cleanup(IntPtr ctx);
		
		[DllImportAttribute(libglsl_optmizer_dll, EntryPoint="_Z16glslopt_optimizeP11glslopt_ctx19glslopt_shader_typePKcj")]
		static extern IntPtr glslopt_optimize(IntPtr context, glslopt_shader_type type, string shaderSource, uint options);
				
		[DllImportAttribute(libglsl_optmizer_dll, EntryPoint="_Z18glslopt_get_statusP14glslopt_shader")]
		static extern bool glslopt_get_status(IntPtr shader);
		
		[DllImportAttribute(libglsl_optmizer_dll, EntryPoint="_Z18glslopt_get_outputP14glslopt_shader")]
		static extern string glslopt_get_output(IntPtr shader);
		
		[DllImportAttribute(libglsl_optmizer_dll, EntryPoint="_Z15glslopt_get_logP14glslopt_shader")]
		static extern string glslopt_get_log(IntPtr shader);
		
		[DllImportAttribute(libglsl_optmizer_dll, EntryPoint="_Z21glslopt_shader_deleteP14glslopt_shader")]
		static extern void glslopt_shader_delete(IntPtr shader);
		
		static IntPtr ctx = IntPtr.Zero; //todo: instantiate and batch optimizations
		static GLSLOptimizer()
		{
			ctx = glslopt_initialize(false);
		}
		
		public static string Optimize(string shaderCode, ShaderType shaderType) {
			glslopt_shader_type glslType;
			switch (shaderType) {
			case ShaderType.FragmentShader:
				glslType = glslopt_shader_type.kGlslOptShaderFragment;
				break;
			case ShaderType.VertexShader:
				glslType = glslopt_shader_type.kGlslOptShaderVertex;
				break;
			default:
				throw new ArgumentException("Must be pixel or vertex shader");
			}
			
			//pretend to be version 1 or glsloptimizer complains
			shaderCode = shaderCode.Replace ("#version 110", "");
			
			IntPtr shader = glslopt_optimize(ctx, glslType, shaderCode, 0);
			
			string output;
			try {
				if (glslopt_get_status(shader)) {
					output = glslopt_get_output(shader);
				} else {
					string errorLog = glslopt_get_log(shader);
					throw new Exception(errorLog);
				}
			} finally {
				glslopt_shader_delete(shader);
			}
			
			return output;
		}
		
	}
}

#endif