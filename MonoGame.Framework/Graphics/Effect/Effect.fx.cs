using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class Effect : GraphicsResource
	{
		private void ReadFXEffect (byte[] effectCode)
		{
			DXEffectObject effectObject = DXEffectObject.FromCompiledD3DXEffect (effectCode);

			ConstantBuffers = new ConstantBuffer[effectObject.ConstantBuffers.Count];
			for (int i=0; i<ConstantBuffers.Length; i++) {
				ConstantBuffers [i] = new ConstantBuffer (graphicsDevice,
				                                        effectObject.ConstantBuffers [i].Size,
				                                        effectObject.ConstantBuffers [i].ParameterIndex.ToArray (),
				                                        effectObject.ConstantBuffers [i].ParameterOffset.ToArray (),
				                                        effectObject.ConstantBuffers [i].Name);
			}


			Parameters = new EffectParameterCollection ();
			foreach (var parameter in effectObject.Parameters) {
				Parameters.Add (ReadXNAParameter (parameter));
			}

			var shaderList = new List<Shader> ();
			foreach (var shader in effectObject.Shaders) {
				shaderList.Add (new FXShader (graphicsDevice, shader));
			}

			Techniques = new EffectTechniqueCollection ();
			foreach (var technique in effectObject.Techniques) {
				var passes = new EffectPassCollection ();
				foreach (var pass in technique.pass_handles) {
					Shader vertexShader = null;
					Shader pixelShader = null;
					foreach (var state in pass.states) {
						if (state.type != DXEffectObject.STATE_TYPE.CONSTANT) {
							throw new NotImplementedException ();
						}
						var operation = DXEffectObject.state_table [state.operation];
						if (operation.class_ == DXEffectObject.STATE_CLASS.PIXELSHADER) {
							pixelShader = shaderList [(int)state.parameter.data];
						} else if (operation.class_ == DXEffectObject.STATE_CLASS.VERTEXSHADER) {
							vertexShader = shaderList [(int)state.parameter.data];
						} else {
							throw new NotImplementedException ();
						}
					}

					if (vertexShader == null) {
						vertexShader = new PassthroughVertexShader ();
					}
					if (pixelShader == null) {
						throw new NotImplementedException ();
					}

					passes.Add (new EffectPass (this,
					                          pass.name,
					                          vertexShader,
					                          pixelShader,
					                          null, null, null,
					                          new EffectAnnotationCollection ())
					);
				}

				Techniques.Add (new EffectTechnique (this, technique.name, passes, null));
			}

			CurrentTechnique = Techniques [0];
		}

		static private EffectParameter ReadXNAParameter (DXEffectObject.d3dx_parameter parameter)
		{
			var elements = new EffectParameterCollection ();
			for (int i=0; i<parameter.element_count; i++) {
				elements.Add (ReadXNAParameter (parameter.member_handles [i]));
			}
			var members = new EffectParameterCollection ();
			for (int i=0; i<parameter.member_count; i++) {
				members.Add (ReadXNAParameter (parameter.member_handles [i]));
			}
			var annotations = new EffectAnnotationCollection ();
			for (int i=0; i<parameter.annotation_count; i++) {
				annotations.Add (new EffectAnnotation (ReadXNAParameter (parameter.annotation_handles [i])));
        	}

        	return new EffectParameter(
        		DXEffectObject.ToXNAParameterClass(parameter.class_),
        		DXEffectObject.ToXNAParameterType(parameter.type),
        		parameter.name,
        		(int)parameter.rows,
        		(int)parameter.columns,
        		parameter.semantic,
        		annotations,
        		elements,
        		members,
        		parameter.data);
        }
	}
}

