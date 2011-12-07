using System;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
	internal class DXPreshader
	{
		
		MojoShader.MOJOSHADER_preshader preshader;
		MojoShader.MOJOSHADER_symbol[] symbols;
		
		float[] inRegs;
		float[] outRegs;
		
		//TODO: Fix mojoshader to handle output registers properly
		
		public DXPreshader (IntPtr preshaderPtr, int output_count)
		{
			preshader = (MojoShader.MOJOSHADER_preshader)Marshal.PtrToStructure(
					preshaderPtr,
					typeof(MojoShader.MOJOSHADER_preshader));
			
			symbols = DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_symbol>(
					preshader.symbols, (int)preshader.symbol_count);
			
			int input_count = 0;
			foreach (MojoShader.MOJOSHADER_symbol symbol in symbols) {
				input_count = Math.Max (
					input_count, (int)(symbol.register_index+symbol.register_count));
			}
			
			inRegs = new float[4*input_count];
			outRegs = new float[4*output_count];
		}
		
		public float[] Run(EffectParameterCollection parameters) {
			//todo: cleanup and merge with DXShader :/
			
			//only populate modified stuff?
			foreach (MojoShader.MOJOSHADER_symbol symbol in symbols) {
				if (symbol.register_set != MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4) {
					throw new NotImplementedException();
				}
				//todo: support array parameters
				EffectParameter parameter = parameters[symbol.name];
				
				if (parameter.Elements.Count > 0) {
					for (int i=0; i<parameter.Elements.Count; i++) {
						EffectParameter element = parameter.Elements[i];
						
						switch (element.ParameterClass) {
						case EffectParameterClass.Scalar:
							switch (element.ParameterType) {
							case EffectParameterType.Single:
								inRegs[(symbol.register_index+i)*4] = (float)element.data;
								break;
							case EffectParameterType.Int32:
								inRegs[(symbol.register_index+i)*4] = (float)(int)element.data;
								break;
							default:
								throw new NotImplementedException();
							}
							
							break;
						case EffectParameterClass.Vector:
						case EffectParameterClass.Matrix:
							for (int j=0; j<element.RowCount*parameter.ColumnCount; j++) {
								inRegs[(symbol.register_index+i)*4+j] = ((float[])parameter.data)[i];
							}
							break;
						default:
							throw new NotImplementedException();
							//break;
						}
					}
					continue;
				}
				
				switch (parameter.ParameterClass) {
				case EffectParameterClass.Scalar:
					switch (parameter.ParameterType) {
					case EffectParameterType.Single:
						inRegs[symbol.register_index*4] = (float)parameter.data;
						break;
					case EffectParameterType.Int32:
						inRegs[symbol.register_index*4] = (float)(int)parameter.data;
						break;
					default:
						throw new NotImplementedException();
					}
					
					break;
				case EffectParameterClass.Vector:
				case EffectParameterClass.Matrix:
					for (int i=0; i<parameter.RowCount*parameter.ColumnCount; i++) {
						inRegs[symbol.register_index*4+i] = ((float[])parameter.data)[i];
					}
					break;
				default:
					throw new NotImplementedException();
					//break;
				}
			}
			
			MojoShader.NativeMethods.MOJOSHADER_runPreshader(ref preshader, inRegs, outRegs);
			
			return outRegs;
		}
	}
}

