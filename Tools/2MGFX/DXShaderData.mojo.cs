using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Microsoft.Xna.Framework.Graphics
{
	internal partial class DXShaderData
	{

		private MojoShader.MOJOSHADER_symbol[] _symbols;

		public static DXShaderData CreateGLSL (byte[] byteCode, List<DXConstantBufferData> cbuffers, int sharedIndex, Dictionary<string, SamplerState> samplerStates)
		{
			var dxshader = new DXShaderData ();
			dxshader.SharedIndex = sharedIndex;
			dxshader.Bytecode = (byte[])byteCode.Clone ();

			// Use MojoShader to convert the HLSL bytecode to GLSL.

			var parseDataPtr = MojoShader.NativeMethods.MOJOSHADER_parse (
				"glsl",
				byteCode,
				byteCode.Length,
				IntPtr.Zero,
				0,
				IntPtr.Zero,
				0,
				IntPtr.Zero,
				IntPtr.Zero,
				IntPtr.Zero);

			var parseData = DXHelper.Unmarshal<MojoShader.MOJOSHADER_parseData> (parseDataPtr);
			if (parseData.error_count > 0) {
				var errors = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_error> (
					parseData.errors,
					parseData.error_count
				);
				throw new Exception (errors [0].error);
			}

			switch (parseData.shader_type) {
			case MojoShader.MOJOSHADER_shaderType.MOJOSHADER_TYPE_PIXEL:
				dxshader.IsVertexShader = false;
				break;
			case MojoShader.MOJOSHADER_shaderType.MOJOSHADER_TYPE_VERTEX:
				dxshader.IsVertexShader = true;
				break;
			default:
				throw new NotSupportedException ();
			}
	

			// Conver the attributes.
			//
			// TODO: Could this be done using DX shader reflection?
			//
			{
				var attributes = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_attribute> (
						parseData.attributes, parseData.attribute_count);

				dxshader._attributes = new Attribute[attributes.Length];
				for (var i = 0; i < attributes.Length; i++) {
					dxshader._attributes [i].name = attributes [i].name;
					dxshader._attributes [i].index = attributes [i].index;
					dxshader._attributes [i].usage = DXEffectObject.ToXNAVertexElementUsage (attributes [i].usage);
				}
			}

			var symbols = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_symbol> (
					parseData.symbols, parseData.symbol_count);

			//try to put the symbols in the order they are eventually packed into the uniform arrays
			//this /should/ be done by pulling the info from mojoshader
			Array.Sort (symbols, delegate(MojoShader.MOJOSHADER_symbol a, MojoShader.MOJOSHADER_symbol b) {
				uint va = a.register_index;
				if (a.info.elements == 1)
					va += 1024; //hax. mojoshader puts array objects first
				uint vb = b.register_index;
				if (b.info.elements == 1)
					vb += 1024;
				return va.CompareTo (vb);
			}
			);//(a, b) => ((int)(a.info.elements > 1))a.register_index.CompareTo(b.register_index));

			// For whatever reason the register indexing is 
			// incorrect from MojoShader.
			{
				uint bool_index = 0;
				uint float4_index = 0;
				uint int4_index = 0;

				for (var i = 0; i < symbols.Length; i++) {
					switch (symbols [i].register_set) {
					case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL:
						symbols [i].register_index = bool_index;
						bool_index += symbols [i].register_count;
						break;

					case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4:
						symbols [i].register_index = float4_index;
						float4_index += symbols[i].register_count;
						break;

					case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_INT4:
						symbols [i].register_index = int4_index;
						int4_index += symbols [i].register_count;
						break;
					}
				}
			}

			// Get the samplers.
			var samplers = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_sampler> (
					parseData.samplers, parseData.sampler_count);
			dxshader._samplers = new Sampler[samplers.Length];
			for (var i = 0; i < samplers.Length; i++) {

				//sampler mapping to parameter is unknown atm
				dxshader._samplers [i].parameter = -1;

				// GLSL needs the sampler name.
				dxshader._samplers [i].samplerName = samplers [i].name;

				// We need the parameter name for creating the parameter
				// listing for the effect... look for that in the symbols.
				dxshader._samplers [i].parameterName =
					symbols.First (e => e.register_set == MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_SAMPLER &&
					e.register_index == samplers [i].index
				).name;

				SamplerState state = null;
				samplerStates.TryGetValue(dxshader._samplers[i].parameterName, out state);
				dxshader._samplers[i].state = state;

				// Set the rest of the sampler info.
				dxshader._samplers [i].type = samplers [i].type;
				dxshader._samplers [i].index = samplers [i].index;					
			}

			// Gather all the parameters used by this shader.
			var symbol_types = new [] { 
				new { name = dxshader.IsVertexShader ? "vs_uniforms_bool" : "ps_uniforms_bool", set = MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL, },
				new { name = dxshader.IsVertexShader ? "vs_uniforms_ivec4" : "ps_uniforms_ivec4", set = MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_INT4, },
				new { name = dxshader.IsVertexShader ? "vs_uniforms_vec4" : "ps_uniforms_vec4", set = MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4, },
			};

			var cbuffer_index = new List<int> ();
			for (var i = 0; i < symbol_types.Length; i++) {
				var cbuffer = new DXConstantBufferData (symbol_types [i].name,
													   symbol_types [i].set,
													   symbols);
				if (cbuffer.Size == 0)
					continue;

				var match = cbuffers.FindIndex (e => e.SameAs (cbuffer));
				if (match == -1) {
					cbuffer_index.Add (cbuffers.Count);
					cbuffers.Add (cbuffer);
				} else
					cbuffer_index.Add (match);
			}
			dxshader._cbuffers = cbuffer_index.ToArray ();

			var glslCode = parseData.output;

#if GLSLOPTIMIZER
			//glslCode = GLSLOptimizer.Optimize(glslCode, ShaderType);
#endif

			// TODO: This sort of sucks... why does MojoShader not produce
			// code valid for GLES out of the box?

			// GLES platforms do not like this.
			glslCode = glslCode.Replace ("#version 110", "");

			// Add the required precision specifiers for GLES.
			glslCode = "#ifdef GL_ES\r\n" +
				"precision highp float;\r\n" +
				"precision mediump int;\r\n" +
				"#endif\r\n" +
				glslCode;

			// Store the code for serialization.
			dxshader.ShaderCode = Encoding.ASCII.GetBytes (glslCode);

			return dxshader;
		}

		public void SetSamplerParameters (Dictionary<string, DXEffectObject.d3dx_parameter> samplers,
										 List<DXEffectObject.d3dx_parameter> parameters)
		{
			for (int i=0; i<_samplers.Length; i++) {
				DXEffectObject.d3dx_parameter param;
				if (samplers.TryGetValue (_samplers[i].parameterName, out param)) {
					var samplerState = (DXEffectObject.d3dx_sampler)param.data;
					if (samplerState != null && samplerState.state_count > 0) {
						var textureName = samplerState.states [0].parameter.name;
						var index = parameters.FindIndex (e => e.name == textureName);
						if (index != -1)
							_samplers[i].parameter = index;
					}
				}
			}
		}
	}
}