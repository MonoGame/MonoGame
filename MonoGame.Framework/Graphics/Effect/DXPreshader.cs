using System;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
	internal class DXPreshader
	{
		
		MojoShader.MOJOSHADER_preshader preshader;
		MojoShader.MOJOSHADER_symbol[] symbols;
		
		float[] inRegs;
		
		//TODO: Fix mojoshader to handle output registers properly
		
		public DXPreshader (IntPtr preshaderPtr)
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
		}
		
		public void Run(EffectParameterCollection parameters, float[] outRegs) {
			//todo: cleanup and merge with DXShader :/
			
			//only populate modified stuff?
			foreach (MojoShader.MOJOSHADER_symbol symbol in symbols) {
				if (symbol.register_set != MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4) {
					throw new NotImplementedException();
				}
				EffectParameter parameter = parameters[symbol.name];
				Single[] data = parameter.GetValueSingleArray();
				
				switch (parameter.ParameterClass) {
				case EffectParameterClass.Scalar:
					for (int i=0; i<data.Length; i++) {
						//preshader scalar arrays map one to each vec4 register
						inRegs[symbol.register_index*4+i*4] = (float)data[i];
					}
					break;
				case EffectParameterClass.Vector:
				case EffectParameterClass.Matrix:
					if (parameter.Elements.Count > 0) {
						throw new NotImplementedException();
					}
					for (int y=0; y<Math.Min (symbol.register_count, parameter.RowCount); y++) {
						for (int x=0; x<parameter.ColumnCount; x++) {
							inRegs[(symbol.register_index+y)*4+x] = (float)data[y*4+x];
						}
					}
					break;
				default:
					throw new NotImplementedException();
				}
			}
			
			MojoShader.NativeMethods.MOJOSHADER_runPreshader(ref preshader, inRegs, outRegs);
		}
	}
}

