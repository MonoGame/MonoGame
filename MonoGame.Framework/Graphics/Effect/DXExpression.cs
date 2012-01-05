using System;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
	internal class DXExpression
	{
		string indexName;
		float[] outRegs;
		DXPreshader preshader;
		
		public DXExpression (string indexName, byte[] expressionData)
		{
			this.indexName = indexName;
			
			IntPtr parseDataPtr = MojoShader.NativeMethods.MOJOSHADER_parseExpression(
				expressionData,
				expressionData.Length,
				IntPtr.Zero,
				IntPtr.Zero,
				IntPtr.Zero
			);
			
			MojoShader.MOJOSHADER_parseData parseData =
				(MojoShader.MOJOSHADER_parseData)Marshal.PtrToStructure(
					parseDataPtr,
					typeof(MojoShader.MOJOSHADER_parseData));
			
			if (parseData.error_count > 0) {
				MojoShader.MOJOSHADER_error[] errors =
					DXHelper.UnmarshalArray<MojoShader.MOJOSHADER_error>(
						parseData.errors, parseData.error_count);
				throw new Exception(errors[0].error);
			}
			
			preshader = new DXPreshader(parseData.preshader);
			outRegs = new float[4];
			
		}
		
		public object Evaluate(EffectParameterCollection parameters)
		{
			preshader.Run (parameters, outRegs);
			
			int index = (int)outRegs[0];
			
			return parameters[indexName].Elements[index].data;
		}
		
	}
}

