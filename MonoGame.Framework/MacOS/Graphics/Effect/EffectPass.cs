using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoMac.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectPass
    {
        EffectTechnique _technique = null;
		string name;
		int shaderProgram = 0;
		DXEffectObject.d3dx_state[] states;
		DXShader pixelShader;
		DXShader vertexShader;
		
		bool needPixelShader = false;
		bool needVertexShader = false;
		
		void makeProgram() {
			if (shaderProgram > 0) {
				GL.DeleteProgram (1, ref shaderProgram);
			}
			
			shaderProgram = GL.CreateProgram ();
			
			if (needPixelShader) {
				GL.AttachShader (shaderProgram, pixelShader.gl_shader);
			}
			if (needVertexShader) {
				GL.AttachShader (shaderProgram, vertexShader.gl_shader);
			}
			
			// Set the parameters
			//is this nesesary, or just for VR?
			/*GL.ProgramParameter (shaderProgram,
				AssemblyProgramParameterArb.GeometryInputType,(int)All.Lines);
			GL.ProgramParameter (shaderProgram,
				AssemblyProgramParameterArb.GeometryOutputType, (int)All.Line);*/
			
			// Set the max vertices
			int maxVertices;
			GL.GetInteger (GetPName.MaxGeometryOutputVertices, out maxVertices);
			GL.ProgramParameter (shaderProgram,
				AssemblyProgramParameterArb.GeometryVerticesOut, maxVertices);
			
			// Link the program
			GL.LinkProgram (shaderProgram);
		}
		
		public void Apply ()
		{
			_technique._effect.OnApply();
			
			foreach ( DXEffectObject.d3dx_state state in states) {
				
				//constants handled on init
				if (state.type == DXEffectObject.STATE_TYPE.CONSTANT) continue;
				
				if (state.operation.class_ == DXEffectObject.STATE_CLASS.PIXELSHADER ||
					state.operation.class_ == DXEffectObject.STATE_CLASS.VERTEXSHADER) {
					
					switch (state.type) {
					case DXEffectObject.STATE_TYPE.PARAMETER:
						//should be easy
					case DXEffectObject.STATE_TYPE.EXPRESSIONINDEX:
						//hmm
					default:
						throw new NotImplementedException();
					}
					
				}
				
			}
			
			GL.UseProgram (shaderProgram);
			

		}

        public EffectPass(EffectTechnique technique, DXEffectObject.d3dx_pass pass)
        {
            _technique = technique;
			
			name = pass.name;
			states = pass.states;
			
			foreach ( DXEffectObject.d3dx_state state in states) {
				if (state.operation.class_ == DXEffectObject.STATE_CLASS.PIXELSHADER) {
					needPixelShader = true;
					if (state.type == DXEffectObject.STATE_TYPE.CONSTANT) {
						pixelShader = new DXShader((byte[])state.parameter.data);
					}
				} else if (state.operation.class_ == DXEffectObject.STATE_CLASS.VERTEXSHADER) {
					needVertexShader = true;
					if (state.type == DXEffectObject.STATE_TYPE.CONSTANT) {
						vertexShader = new DXShader((byte[])state.parameter.data);
					}
				}
			}
			
			//If we have what we need make the program now, why not
			
			if ( (needPixelShader == (pixelShader != null)) &&
				 (needVertexShader == (vertexShader != null))) {
				makeProgram();
			}
			
        }
		
		public string Name { get { return name; } }
		
    }
}
