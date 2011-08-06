using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectPass
    {
        EffectTechnique _technique = null;
		internal int shaderProgram;
		
		
		public void Apply ()
		{

			// Tell the GL Context to use the program
			GL.UseProgram (shaderProgram);	

			//_technique._effect.Apply();

		}

        public EffectPass(EffectTechnique technique)
        {
            _technique = technique;
		
        }
		
		internal void ApplyPass ()
		{
			
			// Create a Program object
			shaderProgram = GL.CreateProgram ();

			// Attach our compiled shaders
			if ( VertexIndex < _technique._effect.vertexShaders.Count)
				GL.AttachShader (shaderProgram, _technique._effect.vertexShaders[VertexIndex]);
			if ( FragmentIndex < _technique._effect.fragmentShaders.Count)			
				GL.AttachShader (shaderProgram, _technique._effect.fragmentShaders[FragmentIndex]);
			
			// TODO GL.ProgramParameter gives no entry point exception
			// this issue is reported here http://www.opentk.com/node/1637
			// I've replaced GL.ProgramParameter for GL.Arb.ProgramParameter
			// I don't know how critical is this, this will need review latter
			
			// Set the parameters
//			GL.ProgramParameter (shaderProgram, Version32.GeometryInputType, (int)All.Lines);	
//			GL.ProgramParameter (shaderProgram, Version32.GeometryOutputType, (int)All.Line);
			
			GL.Arb.ProgramParameter (shaderProgram, ArbGeometryShader4.GeometryInputTypeArb, (int)All.Lines);	
			GL.Arb.ProgramParameter (shaderProgram, ArbGeometryShader4.GeometryOutputTypeArb, (int)All.Line);
			
			// Set the max vertices
			int maxVertices;
			GL.GetInteger (GetPName.MaxGeometryOutputVertices, out maxVertices);
			//GL.ProgramParameter (shaderProgram, Version32.GeometryVerticesOut, maxVertices);
			GL.Arb.ProgramParameter (shaderProgram, ArbGeometryShader4.GeometryVerticesOutArb, maxVertices);

			// Link the program
			GL.LinkProgram (shaderProgram);
			string name = String.Format("Technique {0} - Pass {1}: ",_technique.Name, Name);
			ShaderLog(name,shaderProgram);				
			
		}
		
		public string Name { get; set; }
		
		// internal for now until I figure out what I can do with this mess
		internal int VertexIndex { get; set; }
		internal int FragmentIndex { get; set; }
		
		// Output the log of an object
		private void ShaderLog (string whichObj, int obj)
		{
			int infoLogLen = 0;
			var infoLog = "Is good to go.";

			GL.GetProgram (obj, ProgramParameter.InfoLogLength, out infoLogLen);

			if (infoLogLen > 0)
				infoLog = GL.GetProgramInfoLog (obj);

			Console.WriteLine ("{0} {1}", whichObj, infoLog);

		}		
    }
}
