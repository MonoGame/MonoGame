using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
	// This file includes only the stuff needed for parsing from 
	// Microsoft D3DX Effect files at content build time.
	//
	// See the DXEffectObject.cs for the runtime code.

	internal partial class DXEffectObject
	{
		/// <summary>
		/// Returns an effect from a compiled Microsoft D3DX Effect file.
		/// </summary>
		/// <remarks>
		/// Note that we only support profile fx_2_0 effects at this time.
		/// </remarks>
		/// <param name="effectCode">The content of a compiled Microsoft Effect file.</param>
		/// <returns>A DXEffectObject for use at runtime.</returns>
		public static DXEffectObject FromCompiledD3DXEffect (byte[] effectCode)
		{
			var effect = new DXEffectObject (effectCode);

			// Do some fixups and cleanup.
						
			// Remove unsupported types from the parameter lists!
			var parameters = new List<d3dx_parameter> ();
			var samplerNameLookup = new Dictionary<string,d3dx_parameter> ();
			foreach (var param in effect.Parameters) {
				if (param.type > D3DXPARAMETER_TYPE.TEXTURECUBE) {
					// Store the samplers so we can fix up the shaders.
					if (param.type >= D3DXPARAMETER_TYPE.SAMPLER &&
						param.type <= D3DXPARAMETER_TYPE.SAMPLERCUBE)
						samplerNameLookup.Add (param.name, param);

					// These extended types we do not
					// store as parameters!
					continue;
				}

				parameters.Add (param);
			}
			effect.Parameters = parameters.ToArray ();

			// Fix up the samplers and constant buffers.
			foreach (var shader in effect.Shaders) {
				shader.SetSamplerParameters (samplerNameLookup, parameters);
			//	shader.CreateConstantBuffer(parameters, effect.ConstantBuffers);

			}

			return effect;
		}

		private DXEffectObject (byte[] effectCode)
		{
			uint baseOffset = 8;
			var tag = BitConverter.ToUInt32 (effectCode, 0);
			if (tag == 0xBCF00BCF) {
				// TODO: What are we skipping here?
				baseOffset += BitConverter.ToUInt32 (effectCode, 4);
			} else if (tag != 0xFEFF0901) {
				// The effect format is too old, too new, or 
				// ascii which we can't compile atm!
				throw new NotImplementedException ("Unsupported effect file format!");
			}

			var startoffset = BitConverter.ToUInt32 (effectCode, (int)(baseOffset - 4));

			// Initialize the list of unique shaders.
			Shaders = new List<DXShaderData> ();

			ConstantBuffers = new List<DXConstantBufferData> ();

			using (var stream = new MemoryStream(effectCode, (int)baseOffset, (int)(effectCode.Length-baseOffset)))
			using (var reader = new BinaryReader(stream)) {
				// Move to the start of the effect data.
				reader.BaseStream.Seek (startoffset, SeekOrigin.Current);

				var parameterCount = reader.ReadUInt32 ();
				var techniqueCount = reader.ReadUInt32 ();
				reader.ReadUInt32 (); //unkn

				var objectCount = reader.ReadUInt32 ();
				Objects = new d3dx_parameter[objectCount];

				Parameters = new d3dx_parameter[parameterCount];
				for (var i = 0; i < parameterCount; i++)
					Parameters [i] = parse_effect_parameter (reader);

				Techniques = new d3dx_technique[techniqueCount];
				for (var i = 0; i < techniqueCount; i++)
					Techniques [i] = parse_effect_technique (reader);

				var stringCount = reader.ReadUInt32 ();
				var resourceCount = reader.ReadUInt32 ();

				for (var i = 0; i < stringCount; i++) {
					var id = reader.ReadUInt32 ();
					parse_data (reader, Objects [id]);
				}

				for (var i = 0; i < resourceCount; i++)
					parse_resource (reader);
			}
		}

		private static string parse_name (BinaryReader reader, long offset)
		{
			var oldPos = reader.BaseStream.Position;
			reader.BaseStream.Seek (offset, SeekOrigin.Begin);

			var length = reader.ReadInt32 ();
			var rb = reader.ReadBytes (length);
			var r = System.Text.ASCIIEncoding.ASCII.GetString (rb);
			r = r.Replace ("\0", "");

			reader.BaseStream.Seek (oldPos, SeekOrigin.Begin);
			return r;
		}
		
		private void parse_data (BinaryReader reader, d3dx_parameter param)
		{
			switch (param.type) {
			default:
				var dummy = reader.ReadUInt32 ();
				Debug.Assert (dummy == 0, "Got bad dummy data!");
				break;

			case D3DXPARAMETER_TYPE.STRING:
				var offset = reader.ReadUInt32 ();
				param.data = parse_name (reader, offset);
				break;

			case D3DXPARAMETER_TYPE.VERTEXSHADER:
			case D3DXPARAMETER_TYPE.PIXELSHADER:
				var size = (int)reader.ReadUInt32 ();
				var bytecode = reader.ReadBytes((size + 3) & ~3); // DWORD aligned!
				var index = CreateShader(bytecode);
				param.data = index;
				break;
			}
		}

		private int CreateShader(byte[] bytecode)
		{
			// First look to see if we already created this same shader.
			foreach (var shader in Shaders)
			{
				if (bytecode.SequenceEqual(shader.Bytecode))
					return shader.SharedIndex;
			}

			// Create a new shader.
			var dxShader = DXShaderData.CreateGLSL(bytecode, ConstantBuffers, Shaders.Count);
			Shaders.Add(dxShader);
			return dxShader.SharedIndex;
		}

		private void parse_resource (BinaryReader reader)
		{
			d3dx_state state;

			var technique_index = reader.ReadUInt32 ();
			var index = reader.ReadUInt32 ();
			var element_index = reader.ReadUInt32 ();
			var state_index = reader.ReadUInt32 ();
			var usage = reader.ReadUInt32 ();
			
			if (technique_index == 0xffffffff) {
				var parameter = Parameters [index];
				
				if (element_index != 0xffffffff && parameter.element_count != 0) 
					parameter = parameter.member_handles [element_index];

				var sampler = (d3dx_sampler)parameter.data;
				
				state = sampler.states [state_index];
			} else {
				var technique = Techniques [technique_index];
				var pass = technique.pass_handles [index];
				
				state = pass.states [state_index];
			}
			
			//parameter assignment
			var param = state.parameter;
			Debug.WriteLine ("resource usage=" + usage.ToString ());
			switch (usage) {
			case 0:
				switch (param.type) {
				case D3DXPARAMETER_TYPE.VERTEXSHADER:
				case D3DXPARAMETER_TYPE.PIXELSHADER:
					state.type = STATE_TYPE.CONSTANT;
					parse_data (reader, param);
					break;
				case D3DXPARAMETER_TYPE.BOOL:
				case D3DXPARAMETER_TYPE.INT:
				case D3DXPARAMETER_TYPE.FLOAT:
				case D3DXPARAMETER_TYPE.STRING:
					//assignment by FXLVM expression
					state.type = STATE_TYPE.EXPRESSION;
					var size = reader.ReadUInt32 ();
					param.data = reader.ReadBytes ((int)((size + 3) & ~3)); // DWORD aligned!
					break;
				
				default:
					throw new NotImplementedException ();
				}
				break;

			case 1:
				state.type = STATE_TYPE.PARAMETER;
				//the state's parameter is another parameter
				//the we are given its name

				var nameLength_ = reader.ReadUInt32 ();
				var name = parse_name (reader, reader.BaseStream.Position - 4);
				reader.BaseStream.Seek ((nameLength_ + 3) & ~3, SeekOrigin.Current); // DWORD aligned!
				
				foreach (var findParam in Parameters) {
					if (findParam.name == name) {
						param.data = findParam.data;
						param.name = findParam.name;
						//todo: copy other stuff
						break;
					}
				}
				break;

			case 2:
				throw new NotImplementedException ();
				/*
				//Array index by FXLVM expression
				state.type = STATE_TYPE.EXPRESSIONINDEX;
				//preceded by array name
				
				//annoying hax to extract the name
				var length = reader.ReadUInt32();
				var nameLength = reader.ReadUInt32();
				var paramName = parse_name(reader, reader.BaseStream.Position - 4);
				reader.BaseStream.Seek((nameLength + 3) & ~3, SeekOrigin.Current); // DWORD aligned!

				var expressionData = reader.ReadBytes((int)(length - 4 - nameLength));				
				param.data = new DXExpression(paramName, expressionData);*/
				break;

			default:
				Debug.WriteLine ("Unknown usage " + usage.ToString ());
				break;
			}
			
		}
		
		private static void parse_effect_typedef (BinaryReader reader, long offset, d3dx_parameter param, d3dx_parameter parent, uint flags)
		{
			var oldPos = reader.BaseStream.Position;
			reader.BaseStream.Seek (offset, SeekOrigin.Begin);
			
			param.flags = flags;
			
			if (parent != null) {
				/* elements */
				//we evaluate recursively because elements may have members
				param.type = parent.type;
				param.class_ = parent.class_;
				param.name = parent.name;
				param.semantic = parent.semantic;
				param.element_count = 0;
				param.annotation_count = 0;
				param.member_count = parent.member_count;
				param.bytes = parent.bytes;
				param.rows = parent.rows;
				param.columns = parent.columns;
			} else {
				param.type = (D3DXPARAMETER_TYPE)reader.ReadUInt32 ();
				param.class_ = (D3DXPARAMETER_CLASS)reader.ReadUInt32 ();
				param.name = parse_name (reader, reader.ReadUInt32 ());
				param.semantic = parse_name (reader, reader.ReadUInt32 ());
				param.element_count = reader.ReadUInt32 ();
				
				switch (param.class_) {
				case D3DXPARAMETER_CLASS.VECTOR:
					param.columns = reader.ReadUInt32 ();
					param.rows = reader.ReadUInt32 ();
					param.bytes = 4 * param.rows * param.columns;
					break;
				
				case D3DXPARAMETER_CLASS.SCALAR:
				case D3DXPARAMETER_CLASS.MATRIX_ROWS:
				case D3DXPARAMETER_CLASS.MATRIX_COLUMNS:
					param.rows = reader.ReadUInt32 ();
					param.columns = reader.ReadUInt32 ();
					param.bytes = 4 * param.rows * param.columns;
					break;
				
				case D3DXPARAMETER_CLASS.STRUCT:
					param.member_count = reader.ReadUInt32 ();
					break;
					
				case D3DXPARAMETER_CLASS.OBJECT:
					switch (param.type) {
					case D3DXPARAMETER_TYPE.STRING:
						param.bytes = 4; //sizeof(LPCSTR)
						break;
					
					case D3DXPARAMETER_TYPE.PIXELSHADER:
						param.bytes = 4; //sizeof(LPDIRECT3DPIXELSHADER9)
						break;
					
					case D3DXPARAMETER_TYPE.VERTEXSHADER:
						param.bytes = 4; //sizeof(LPDIRECT3DVERTEXSHADER9)
						break;
					
					case D3DXPARAMETER_TYPE.TEXTURE:
					case D3DXPARAMETER_TYPE.TEXTURE1D:
					case D3DXPARAMETER_TYPE.TEXTURE2D:
					case D3DXPARAMETER_TYPE.TEXTURE3D:
					case D3DXPARAMETER_TYPE.TEXTURECUBE:
						param.bytes = 4; //sizeof(LPDIRECT3DBASETEXTURE9)
						break;
					
					case D3DXPARAMETER_TYPE.SAMPLER:
					case D3DXPARAMETER_TYPE.SAMPLER1D:
					case D3DXPARAMETER_TYPE.SAMPLER2D:
					case D3DXPARAMETER_TYPE.SAMPLER3D:
					case D3DXPARAMETER_TYPE.SAMPLERCUBE:
						param.bytes = 0;
						break;
					
					default:
						throw new NotImplementedException ();
					}
					break;
				
				default:
					throw new NotImplementedException ();
					
				}
			}
			
			if (param.element_count > 0) {
				uint param_bytes = 0;
				param.member_handles = new d3dx_parameter[param.element_count];
				for (var i = 0; i < param.element_count; i++) {
					param.member_handles [i] = new d3dx_parameter ();

					//we read the same typedef over and over...
					parse_effect_typedef (reader, reader.BaseStream.Position, param.member_handles [i], param, flags);
					param_bytes += param.member_handles [i].bytes;
				}
				param.bytes = param_bytes;
			} else if (param.member_count > 0) {
				param.member_handles = new d3dx_parameter[param.member_count];
				for (var i = 0; i < param.member_count; i++) {
					param.member_handles [i] = new d3dx_parameter ();

					parse_effect_typedef (reader, reader.BaseStream.Position, param.member_handles [i], null, flags);
					reader.BaseStream.Seek (param.member_handles [i].bytes, SeekOrigin.Current);
					param.bytes += param.member_handles [i].bytes;
				}
			}

			reader.BaseStream.Seek (oldPos, SeekOrigin.Begin);
		}

		private d3dx_sampler parse_sampler (BinaryReader reader)
		{
			var ret = new d3dx_sampler ();

			ret.state_count = reader.ReadUInt32 ();
			if (ret.state_count > 0) {
				ret.states = new d3dx_state[ret.state_count];
				for (int i=0; i<ret.state_count; i++)
					ret.states [i] = parse_state (reader);
			}
			
			return ret;
		}

		private static byte[] sliceBytes (byte[] data, uint start, uint stop)
		{
			var ret = new byte[stop - start];
			for (uint i=start; i<stop; i++)
				ret [i - start] = data [i];

			return ret;
		}
		
		private void parse_value (BinaryReader reader, d3dx_parameter param, byte[] data)
		{
			if (param.element_count != 0) {
				param.data = data;
				uint curOffset = 0;
				for (var i = 0; i < param.element_count; i++) {
					parse_value (reader, param.member_handles [i],
						sliceBytes (data, curOffset, curOffset + param.member_handles [i].bytes));
					curOffset += param.member_handles [i].bytes;
				}
			} else {
				switch (param.class_) {
				case D3DXPARAMETER_CLASS.SCALAR:
				case D3DXPARAMETER_CLASS.VECTOR:
				case D3DXPARAMETER_CLASS.MATRIX_ROWS:
				case D3DXPARAMETER_CLASS.MATRIX_COLUMNS:
					param.data = data;
					break;
				
				case D3DXPARAMETER_CLASS.STRUCT:
					param.data = data;
					uint curOffset = 0;
					for (var i = 0; i < param.member_count; i++) {
						parse_value (reader, param.member_handles [i],
							sliceBytes (data, curOffset, curOffset + param.member_handles [i].bytes));
						curOffset += param.member_handles [i].bytes;
					}
					break;
				
				case D3DXPARAMETER_CLASS.OBJECT:
					
					switch (param.type) {
					case D3DXPARAMETER_TYPE.STRING:
					case D3DXPARAMETER_TYPE.TEXTURE:
					case D3DXPARAMETER_TYPE.TEXTURE1D:
					case D3DXPARAMETER_TYPE.TEXTURE2D:
					case D3DXPARAMETER_TYPE.TEXTURE3D:
					case D3DXPARAMETER_TYPE.TEXTURECUBE:
					case D3DXPARAMETER_TYPE.PIXELSHADER:
					case D3DXPARAMETER_TYPE.VERTEXSHADER:
						var id = reader.ReadUInt32 ();
						Debug.Assert (Objects [id] == null, "Object id collision!");
						Objects [id] = param;
						param.data = data;
						break;
					
					case D3DXPARAMETER_TYPE.SAMPLER:
					case D3DXPARAMETER_TYPE.SAMPLER1D:
					case D3DXPARAMETER_TYPE.SAMPLER2D:
					case D3DXPARAMETER_TYPE.SAMPLER3D:
					case D3DXPARAMETER_TYPE.SAMPLERCUBE:
						param.data = parse_sampler (reader);
						break;
					}
					
					break;
				}
			}
		}

		private void parse_init_value (BinaryReader reader, long offset, d3dx_parameter param)
		{
			var oldPos = reader.BaseStream.Position;
			reader.BaseStream.Seek (offset, SeekOrigin.Begin);

			var data = reader.ReadBytes ((int)param.bytes);
			reader.BaseStream.Seek (offset, SeekOrigin.Begin);
			parse_value (reader, param, data);

			reader.BaseStream.Seek (oldPos, SeekOrigin.Begin);
		}

		private d3dx_parameter parse_effect_annotation (BinaryReader reader)
		{
			var ret = new d3dx_parameter ();
			
			ret.flags = D3DX_PARAMETER_ANNOTATION;

			var typedefOffset = reader.ReadInt32 ();
			parse_effect_typedef (reader, typedefOffset, ret, null, D3DX_PARAMETER_ANNOTATION);

			var valueOffset = reader.ReadInt32 ();
			parse_init_value (reader, valueOffset, ret);
			
			return ret;
		}

		private d3dx_parameter parse_effect_parameter (BinaryReader reader)
		{
			var ret = new d3dx_parameter ();

			var typedefOffset = reader.ReadInt32 ();
			var valueOffset = reader.ReadInt32 ();
			ret.flags = reader.ReadUInt32 ();
			ret.annotation_count = reader.ReadUInt32 ();
			parse_effect_typedef (reader, typedefOffset, ret, null, ret.flags);

			parse_init_value (reader, valueOffset, ret);

			if (ret.annotation_count > 0) {
				ret.annotation_handles = new d3dx_parameter[ret.annotation_count];
				for (var i = 0; i < ret.annotation_count; i++)
					ret.annotation_handles [i] = parse_effect_annotation (reader);
			}
			
			return ret;
		}

		private d3dx_state parse_state(BinaryReader reader)
		{
			var ret = new d3dx_state();
			ret.parameter = new d3dx_parameter();

			ret.type = STATE_TYPE.CONSTANT;
			ret.operation = reader.ReadUInt32();
			ret.index = reader.ReadUInt32();

			var typedefOffset = reader.ReadInt32();
			parse_effect_typedef(reader, typedefOffset, ret.parameter, null, 0);

			var valueOffset = reader.ReadInt32();
			parse_init_value(reader, valueOffset, ret.parameter);

			var operation = state_table[ret.operation];
			if (operation.class_ == STATE_CLASS.RENDERSTATE) 
			{
				//parse the render parameter
				switch (operation.op) 
				{
				case (uint)D3DRENDERSTATETYPE.STENCILENABLE:
				case (uint)D3DRENDERSTATETYPE.ALPHABLENDENABLE:
				case (uint)D3DRENDERSTATETYPE.SCISSORTESTENABLE:
					ret.parameter.data = BitConverter.ToInt32 ((byte[])ret.parameter.data, 0) != 0;
					break;
				case (uint)D3DRENDERSTATETYPE.COLORWRITEENABLE:
					ret.parameter.data = (ColorWriteChannels)BitConverter.ToInt32 ((byte[])ret.parameter.data, 0);
					break;
				case (uint)D3DRENDERSTATETYPE.BLENDOP:
					switch (BitConverter.ToInt32((byte[])ret.parameter.data, 0)) 
					{
					case 1: ret.parameter.data = BlendFunction.Add; break;
					case 2: ret.parameter.data = BlendFunction.Subtract; break;
					case 3: ret.parameter.data = BlendFunction.ReverseSubtract; break;
					case 4: ret.parameter.data = BlendFunction.Min; break;
					case 5: ret.parameter.data = BlendFunction.Max; break;
					default:
						throw new NotSupportedException();
					}
					break;
				case (uint)D3DRENDERSTATETYPE.SRCBLEND:
				case (uint)D3DRENDERSTATETYPE.DESTBLEND:
					switch (BitConverter.ToInt32((byte[])ret.parameter.data, 0)) 
					{
					case 1: ret.parameter.data = Blend.Zero; break;
					case 2: ret.parameter.data = Blend.One; break;
					case 3: ret.parameter.data = Blend.SourceColor; break;
					case 4: ret.parameter.data = Blend.InverseSourceColor; break;
					case 5: ret.parameter.data = Blend.SourceAlpha; break;
					case 6: ret.parameter.data = Blend.InverseSourceAlpha; break;
					case 7: ret.parameter.data = Blend.DestinationAlpha; break;
					case 8: ret.parameter.data = Blend.InverseDestinationAlpha; break;
					case 9: ret.parameter.data = Blend.DestinationColor; break;
					case 10: ret.parameter.data = Blend.InverseDestinationColor; break;
					case 11: ret.parameter.data = Blend.SourceAlphaSaturation; break;
					case 14: ret.parameter.data = Blend.BlendFactor; break;
					case 15: ret.parameter.data = Blend.InverseDestinationAlpha; break;
					default:
						throw new NotSupportedException();
					}
					break;
				case (uint)D3DRENDERSTATETYPE.CULLMODE:
					switch (BitConverter.ToInt32 ((byte[])ret.parameter.data, 0)) 
					{
					case 1: ret.parameter.data = CullMode.None; break;
					case 2: ret.parameter.data = CullMode.CullClockwiseFace; break;
					case 3: ret.parameter.data = CullMode.CullCounterClockwiseFace; break;
					default:
						throw new NotSupportedException();
					}
					break;
				case (uint)D3DRENDERSTATETYPE.STENCILFUNC:
					switch (BitConverter.ToInt32 ((byte[])ret.parameter.data, 0)) 
					{
					case 1: ret.parameter.data = CompareFunction.Never; break;
					case 2: ret.parameter.data = CompareFunction.Less; break;
					case 3: ret.parameter.data = CompareFunction.Equal; break;
					case 4: ret.parameter.data = CompareFunction.LessEqual; break;
					case 5: ret.parameter.data = CompareFunction.Greater; break;
					case 6: ret.parameter.data = CompareFunction.NotEqual; break;
					case 7: ret.parameter.data = CompareFunction.GreaterEqual; break;
					case 8: ret.parameter.data = CompareFunction.Always; break;
					default:
						throw new NotSupportedException();
					}
					break;
				case (uint)D3DRENDERSTATETYPE.STENCILFAIL:
				case (uint)D3DRENDERSTATETYPE.STENCILPASS:
					switch (BitConverter.ToInt32 ((byte[])ret.parameter.data, 0)) 
					{
					case 1: ret.parameter.data = StencilOperation.Keep; break;
					case 2: ret.parameter.data = StencilOperation.Zero; break;
					case 3: ret.parameter.data = StencilOperation.Replace; break;
					case 4: ret.parameter.data = StencilOperation.IncrementSaturation; break;
					case 5: ret.parameter.data = StencilOperation.DecrementSaturation; break;
					case 6: ret.parameter.data = StencilOperation.Invert; break;
					case 7: ret.parameter.data = StencilOperation.Increment; break;
					case 8: ret.parameter.data = StencilOperation.Decrement; break;
					default:
						throw new NotSupportedException();
					}
					break;
				case (uint)D3DRENDERSTATETYPE.STENCILREF:
					ret.parameter.data = BitConverter.ToInt32 ((byte[])ret.parameter.data, 0);
					break;
				default:
					throw new NotImplementedException()
				}
			}
						
			return ret;
		}

		private d3dx_pass parse_effect_pass (BinaryReader reader)
		{
			var ret = new d3dx_pass ();

			ret.name = parse_name (reader, reader.ReadUInt32 ());
			ret.annotation_count = reader.ReadUInt32 ();
			ret.state_count = reader.ReadUInt32 ();

			if (ret.annotation_count > 0) {
				ret.annotation_handles = new d3dx_parameter[ret.annotation_count];
				for (var i = 0; i < ret.annotation_count; i++)
					ret.annotation_handles [i] = parse_effect_annotation (reader);
			}
			
			ret.states = new d3dx_state[ret.state_count];
			for (var i = 0; i < ret.state_count; i++)
				ret.states [i] = parse_state (reader);
			
			return ret;
		}

		private d3dx_technique parse_effect_technique (BinaryReader reader)
		{
			var ret = new d3dx_technique ();

			ret.name = parse_name (reader, reader.ReadUInt32 ());
			ret.annotation_count = reader.ReadUInt32 ();
			ret.pass_count = reader.ReadUInt32 ();

			if (ret.annotation_count > 0) {
				ret.annotation_handles = new d3dx_parameter[ret.annotation_count];
				for (var i = 0; i < ret.annotation_count; i++)
					ret.annotation_handles [i] = parse_effect_annotation (reader);
			}

			if (ret.pass_count > 0) {
				ret.pass_handles = new d3dx_pass[ret.pass_count];
				for (var i = 0; i < ret.pass_count; i++)
					ret.pass_handles [i] = parse_effect_pass (reader);
			}
			
			return ret;
		}
	}
}

