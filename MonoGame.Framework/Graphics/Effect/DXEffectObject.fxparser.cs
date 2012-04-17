using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class DXEffectObject
	{


		public DXEffectObject(byte[] effectCode)
		{
			uint baseOffset = 8;
			uint tag = BitConverter.ToUInt32(effectCode, 0);
			if (tag == 0xBCF00BCF) {
				//handling this extra stuff should really be in the Effect class
				baseOffset += BitConverter.ToUInt32(effectCode, 4);
			} else if (tag != 0xFEFF0901) {
				//effect too old or too new, or ascii which we can't compile atm
				throw new NotImplementedException();
			}


			uint startoffset = BitConverter.ToUInt32(effectCode, (int)(baseOffset-4));

			effectStream = new MemoryStream(effectCode,
				(int)baseOffset, (int)(effectCode.Length-baseOffset));
			effectReader = new BinaryReader(effectStream);

			effectStream.Seek(startoffset, SeekOrigin.Current);
			
			uint parameterCount = effectReader.ReadUInt32();
			uint techniqueCount = effectReader.ReadUInt32();
			effectReader.ReadUInt32(); //unkn
			uint objectCount = effectReader.ReadUInt32();
			
			objects = new d3dx_parameter[objectCount];
			
			parameter_handles = new d3dx_parameter[parameterCount];
			for (int i=0; i<parameterCount; i++) {
				parameter_handles[i] = parse_effect_parameter();
			}
			
			technique_handles = new d3dx_technique[techniqueCount];
			for (int i=0; i<techniqueCount; i++) {
				technique_handles[i] = parse_effect_technique();
			}
			
			uint stringCount = effectReader.ReadUInt32 ();
			uint resourceCount = effectReader.ReadUInt32 ();
			
			for (int i=0; i<stringCount; i++) {
				uint id = effectReader.ReadUInt32 ();
				parse_data(objects[id]);
			}
			
			for (int i=0; i<resourceCount; i++) {
				parse_resource();
			}
			
			effectReader.Close();
			effectStream.Close();
		}
		
		private string parse_name(long offset)
		{
			long oldPos = effectStream.Position; effectStream.Seek (offset, SeekOrigin.Begin);
			
			byte[] rb = effectReader.ReadBytes(effectReader.ReadInt32());
			string r = System.Text.ASCIIEncoding.ASCII.GetString (rb);
			r = r.Replace ("\0", "");
			
			effectStream.Seek (oldPos, SeekOrigin.Begin);
			return r;
		}
		
		private void parse_data(d3dx_parameter param)
		{
			uint size = effectReader.ReadUInt32 ();
			switch (param.type)
			{
			case D3DXPARAMETER_TYPE.STRING:
				param.data = parse_name(effectStream.Position-4);
				effectReader.ReadBytes((int)((size+3) & ~3));	break;
			case D3DXPARAMETER_TYPE.VERTEXSHADER:
				param.data = new DXShader(effectReader.ReadBytes((int)((size+3) & ~3)));
				break;
			case D3DXPARAMETER_TYPE.PIXELSHADER:
				param.data = new DXShader(effectReader.ReadBytes((int)((size+3) & ~3)));
				break;
			}
			

		}
		
		private byte[] copy_data()
		{
			uint size = effectReader.ReadUInt32 ();
			return effectReader.ReadBytes((int)((size+3) & ~3));
		}
		
		private void parse_resource()
		{
			d3dx_state state;
			
			uint technique_index = effectReader.ReadUInt32 ();
			uint index = effectReader.ReadUInt32 ();
			uint element_index = effectReader.ReadUInt32 ();
			uint state_index = effectReader.ReadUInt32 ();
			uint usage = effectReader.ReadUInt32 ();
			
			if (technique_index == 0xffffffff) {
				d3dx_parameter parameter = parameter_handles[index];
				
				if (element_index != 0xffffffff) {
					if (parameter.element_count != 0) {
						parameter = parameter.member_handles[element_index];
					}
				}
				d3dx_sampler sampler = (d3dx_sampler)parameter.data;
				
				state = sampler.states[state_index];
			} else {
				d3dx_technique technique = technique_handles[technique_index];
				d3dx_pass pass = technique.pass_handles[index];
				
				state = pass.states[state_index];
			}
			
			//parameter assignment
			d3dx_parameter param = state.parameter;
			Console.WriteLine ("resource usage="+usage.ToString());
			switch (usage)
			{
			case 0:
				switch (param.type)
				{
				case D3DXPARAMETER_TYPE.VERTEXSHADER:
				case D3DXPARAMETER_TYPE.PIXELSHADER:
					state.type = STATE_TYPE.CONSTANT;
					parse_data(param);
					break;
				case D3DXPARAMETER_TYPE.BOOL:
				case D3DXPARAMETER_TYPE.INT:
				case D3DXPARAMETER_TYPE.FLOAT:
				case D3DXPARAMETER_TYPE.STRING:
					//assignment by FXLVM expression
					state.type = STATE_TYPE.EXPRESSION;
					param.data = copy_data();
					break;
				
				default:
					throw new NotImplementedException();
				}
				break;
			case 1:
				state.type = STATE_TYPE.PARAMETER;
				//the state's parameter is another parameter
				//the we are given its name
				
				uint nameLength_ = effectReader.ReadUInt32 ();
				string name = parse_name (effectStream.Position-4);
				effectStream.Seek ((nameLength_+3) & ~3, SeekOrigin.Current);
				
				foreach (d3dx_parameter findParam in parameter_handles) {
					if (findParam.name == name) {
						param.data = findParam.data;
						param.name = findParam.name;
						//todo: copy other stuff
						break;
					}
				}
				break;
			case 2:
				//Array index by FXLVM expression
				state.type = STATE_TYPE.EXPRESSIONINDEX;
				//preceded by array name
				
				//annoying hax to extract the name
				uint length = effectReader.ReadUInt32 ();
				uint nameLength = effectReader.ReadUInt32 ();
				string paramName = parse_name (effectStream.Position-4);
				effectStream.Seek ( (nameLength+3) & ~3, SeekOrigin.Current);
				byte[] expressionData = effectReader.ReadBytes ((int)(length-4-nameLength));
				
				param.data = new DXExpression(paramName, expressionData);
				break;
			default:
				Console.WriteLine ("Unknown usage "+usage.ToString());
				break;
			}
			
		}
		
		private void parse_effect_typedef(long offset, d3dx_parameter param, d3dx_parameter parent, uint flags)
		{
			long oldPos = effectStream.Position; effectStream.Seek (offset, SeekOrigin.Begin);
			
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
				param.type = (D3DXPARAMETER_TYPE)effectReader.ReadUInt32();
				param.class_ = (D3DXPARAMETER_CLASS)effectReader.ReadUInt32();
				param.name = parse_name(effectReader.ReadUInt32 ());
				param.semantic = parse_name(effectReader.ReadUInt32());
				param.element_count = effectReader.ReadUInt32();
				
				switch (param.class_)
				{
				case D3DXPARAMETER_CLASS.VECTOR:
					param.columns = effectReader.ReadUInt32();
					param.rows = effectReader.ReadUInt32();
					param.bytes = 4 * param.rows * param.columns;
					break;
				
				case D3DXPARAMETER_CLASS.SCALAR:
				case D3DXPARAMETER_CLASS.MATRIX_ROWS:
				case D3DXPARAMETER_CLASS.MATRIX_COLUMNS:
					param.rows = effectReader.ReadUInt32();
					param.columns = effectReader.ReadUInt32();
					param.bytes = 4 * param.rows * param.columns;
					break;
				
				case D3DXPARAMETER_CLASS.STRUCT:
					param.member_count = effectReader.ReadUInt32 ();
					break;
					
				case D3DXPARAMETER_CLASS.OBJECT:
					switch (param.type)
					{
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
						throw new NotImplementedException();
					}
					break;
				
				default:
					throw new NotImplementedException();
					
				}
			}
			
			if (param.element_count > 0) {
				uint param_bytes = 0;
				param.member_handles = new d3dx_parameter[param.element_count];
				for (int i=0; i<param.element_count; i++) {
					param.member_handles[i] = new d3dx_parameter();

					//we read the same typedef over and over...
					parse_effect_typedef (effectStream.Position, param.member_handles[i], param, flags);
					param_bytes += param.member_handles[i].bytes;
				}
				param.bytes = param_bytes;
			} else if (param.member_count > 0) {
				param.member_handles = new d3dx_parameter[param.member_count];
				for (int i=0; i<param.member_count; i++) {
					param.member_handles[i] = new d3dx_parameter();

					parse_effect_typedef(effectStream.Position, param.member_handles[i], null, flags);
					effectStream.Seek(param.member_handles[i].bytes, SeekOrigin.Current);
					param.bytes += param.member_handles[i].bytes;
				}
			}
			
			effectStream.Seek (oldPos, SeekOrigin.Begin);
		}
		
		private d3dx_sampler parse_sampler()
		{
			d3dx_sampler ret = new d3dx_sampler();
			
			ret.state_count = effectReader.ReadUInt32 ();
			if (ret.state_count > 0) {
				ret.states = new d3dx_state[ret.state_count];
				for (int i=0; i<ret.state_count; i++) {
					ret.states[i] = parse_state();
				}
			}
			
			return ret;
		}

		private byte[] sliceBytes(byte[] data, uint start, uint stop) {
			byte[] ret = new byte[stop-start];
			for (uint i=start; i<stop; i++) {
				ret[i-start] = data[i];
			}
			return ret;
		}
		
		private void parse_value(d3dx_parameter param, byte[] data)
		{
			if (param.element_count != 0) {
				param.data = data;
				uint curOffset = 0;
				for (int i=0; i<param.element_count; i++) {
					parse_value(param.member_handles[i],
						sliceBytes(data, curOffset, curOffset+param.member_handles[i].bytes));
					curOffset += param.member_handles[i].bytes;
				}
			} else {
				switch (param.class_)
				{
				case D3DXPARAMETER_CLASS.SCALAR:
				case D3DXPARAMETER_CLASS.VECTOR:
				case D3DXPARAMETER_CLASS.MATRIX_ROWS:
				case D3DXPARAMETER_CLASS.MATRIX_COLUMNS:
					param.data = data;
					break;
				
				case D3DXPARAMETER_CLASS.STRUCT:
					param.data = data;
					uint curOffset = 0;
					for (int i=0; i<param.member_count; i++) {
						parse_value(param.member_handles[i],
							sliceBytes(data, curOffset, curOffset+param.member_handles[i].bytes));
						curOffset += param.member_handles[i].bytes;
					}
					break;
				
				case D3DXPARAMETER_CLASS.OBJECT:
					
					switch (param.type)
					{
					case D3DXPARAMETER_TYPE.STRING:
					case D3DXPARAMETER_TYPE.TEXTURE:
					case D3DXPARAMETER_TYPE.TEXTURE1D:
					case D3DXPARAMETER_TYPE.TEXTURE2D:
					case D3DXPARAMETER_TYPE.TEXTURE3D:
					case D3DXPARAMETER_TYPE.TEXTURECUBE:
					case D3DXPARAMETER_TYPE.PIXELSHADER:
					case D3DXPARAMETER_TYPE.VERTEXSHADER:
						uint id = effectReader.ReadUInt32 ();
						objects[id] = param;
						param.data = data;
						break;
					
					case D3DXPARAMETER_TYPE.SAMPLER:
					case D3DXPARAMETER_TYPE.SAMPLER1D:
					case D3DXPARAMETER_TYPE.SAMPLER2D:
					case D3DXPARAMETER_TYPE.SAMPLER3D:
					case D3DXPARAMETER_TYPE.SAMPLERCUBE:
						param.data = parse_sampler();
						break;
					}
					
					break;
				}
			}

		}

		private void parse_init_value(long offset, d3dx_parameter param)
		{
			long oldPos = effectStream.Position; effectStream.Seek (offset, SeekOrigin.Begin);
			
			byte[] data = effectReader.ReadBytes((int)param.bytes);
			effectStream.Seek (offset, SeekOrigin.Begin);
			parse_value(param, data);
			
			effectStream.Seek (oldPos, SeekOrigin.Begin);
		}

		private d3dx_parameter parse_effect_annotation()
		{
			d3dx_parameter ret = new d3dx_parameter();
			
			ret.flags = D3DX_PARAMETER_ANNOTATION;
			
			long typedefOffset = effectReader.ReadInt32 ();
			parse_effect_typedef(typedefOffset, ret, null, D3DX_PARAMETER_ANNOTATION);
			
			long valueOffset = effectReader.ReadInt32 ();
			parse_init_value(valueOffset, ret);
			
			return ret;
		}
		
		private d3dx_parameter parse_effect_parameter()
		{
			d3dx_parameter ret = new d3dx_parameter();
			
			long typedefOffset = effectReader.ReadInt32 ();
			long valueOffset = effectReader.ReadInt32 ();
			ret.flags = effectReader.ReadUInt32 ();
			ret.annotation_count = effectReader.ReadUInt32 ();
			parse_effect_typedef(typedefOffset, ret, null, ret.flags);
			
			parse_init_value(valueOffset, ret);

			if (ret.annotation_count > 0) {
				ret.annotation_handles = new d3dx_parameter[ret.annotation_count];
				for (int i=0; i<ret.annotation_count; i++) {
					ret.annotation_handles[i] = parse_effect_annotation();
				}
			}
			
			return ret;
		}
		
		private d3dx_state parse_state()
		{
			d3dx_state ret = new d3dx_state();
			ret.parameter = new d3dx_parameter();

			ret.type = STATE_TYPE.CONSTANT;
			ret.operation = state_table[effectReader.ReadUInt32 ()];
			ret.index = effectReader.ReadUInt32 ();
			
			long typedefOffset = effectReader.ReadInt32 ();
			parse_effect_typedef(typedefOffset, ret.parameter, null, 0);

			long valueOffset = effectReader.ReadInt32 ();
			parse_init_value(valueOffset, ret.parameter);
			
			if (ret.operation.class_ == STATE_CLASS.RENDERSTATE) {
				//parse the render parameter
				switch (ret.operation.op) {
				case (uint)D3DRENDERSTATETYPE.STENCILENABLE:
				case (uint)D3DRENDERSTATETYPE.ALPHABLENDENABLE:
				case (uint)D3DRENDERSTATETYPE.SCISSORTESTENABLE:
					ret.parameter.data = BitConverter.ToInt32 ((byte[])ret.parameter.data, 0) != 0;
					break;
				case (uint)D3DRENDERSTATETYPE.COLORWRITEENABLE:
					ret.parameter.data = (ColorWriteChannels)BitConverter.ToInt32 ((byte[])ret.parameter.data, 0);
					break;
				case (uint)D3DRENDERSTATETYPE.BLENDOP:
					switch (BitConverter.ToInt32((byte[])ret.parameter.data, 0)) {
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
					switch (BitConverter.ToInt32((byte[])ret.parameter.data, 0)) {
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
					switch (BitConverter.ToInt32 ((byte[])ret.parameter.data, 0)) {
					case 1: ret.parameter.data = CullMode.None; break;
					case 2: ret.parameter.data = CullMode.CullClockwiseFace; break;
					case 3: ret.parameter.data = CullMode.CullCounterClockwiseFace; break;
					default:
						throw new NotSupportedException();
					}
					break;
				case (uint)D3DRENDERSTATETYPE.STENCILFUNC:
					switch (BitConverter.ToInt32 ((byte[])ret.parameter.data, 0)) {
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
					switch (BitConverter.ToInt32 ((byte[])ret.parameter.data, 0)) {
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
					throw new NotImplementedException();
				}
			}
			
			
			
			return ret;
		}

		private d3dx_pass parse_effect_pass()
		{
			d3dx_pass ret = new d3dx_pass();
			
			ret.name = parse_name (effectReader.ReadUInt32());
			ret.annotation_count = effectReader.ReadUInt32 ();
			ret.state_count = effectReader.ReadUInt32 ();

			if (ret.annotation_count > 0) {
				ret.annotation_handles = new d3dx_parameter[ret.annotation_count];
				for (int i=0; i<ret.annotation_count; i++) {
					ret.annotation_handles[i] = parse_effect_annotation();
				}
			}
			
			ret.states = new d3dx_state[ret.state_count];
			for (int i=0; i<ret.state_count; i++) {
				ret.states[i] = parse_state();
			}
			
			return ret;
		}
		
		private d3dx_technique parse_effect_technique()
		{
			d3dx_technique ret = new d3dx_technique();
			
			ret.name = parse_name (effectReader.ReadUInt32());
			ret.annotation_count = effectReader.ReadUInt32 ();
			ret.pass_count = effectReader.ReadUInt32 ();

			if (ret.annotation_count > 0) {
				ret.annotation_handles = new d3dx_parameter[ret.annotation_count];
				for (int i=0; i<ret.annotation_count; i++) {
					ret.annotation_handles[i] = parse_effect_annotation();
				}
			}

			if (ret.pass_count > 0) {
				ret.pass_handles = new d3dx_pass[ret.pass_count];
				for (int i=0; i<ret.pass_count; i++) {
					ret.pass_handles[i] = parse_effect_pass();
				}
			}
			
			return ret;
		}
		
	}
}

