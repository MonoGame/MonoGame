﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.ES20;
using OpenTK.Graphics.ES11;
using GL11 = OpenTK.Graphics.ES11.GL;
using GL20 = OpenTK.Graphics.ES20.GL;
using All11 = OpenTK.Graphics.ES11.All;
using All20 = OpenTK.Graphics.ES20.All;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectPass
    {
        EffectTechnique _technique = null;
		internal int shaderProgram;

        public void Apply()
        {
			// Tell the GL Context to use the program
			//GL20.UseProgram (shaderProgram);
			
            _technique._effect.Apply();
        }

        public EffectPass(EffectTechnique technique)
        {
            _technique = technique;
        }
		
		internal void ApplyPass ()
		{
			
			// Create a Program object
			shaderProgram = GL20.CreateProgram ();

			// Attach our compiled shaders
			if ( VertexIndex < _technique._effect.vertexShaders.Count)
				GL20.AttachShader (shaderProgram, _technique._effect.vertexShaders[VertexIndex]);
			if ( FragmentIndex < _technique._effect.fragmentShaders.Count)			
				GL20.AttachShader (shaderProgram, _technique._effect.fragmentShaders[FragmentIndex]);

			// Set the parameters
			// TODO GL20.ProgramParameter (shaderProgram, AssemblyProgramParameterArb.GeometryInputType, (int)All.Lines);	
			// TODO GL20.ProgramParameter (shaderProgram, AssemblyProgramParameterArb.GeometryOutputType, (int)All.Line);
			
			// Set the max vertices
			int maxVertices;
			// TODO GL20.GetInteger (GetPName.MaxGeometryOutputVertices, out maxVertices);
			// TODO GL20.ProgramParameter (shaderProgram, AssemblyProgramParameterArb.GeometryVerticesOut, maxVertices);

			// Link the program
			GL20.LinkProgram (shaderProgram);
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

			// TODO GL.GetProgram (obj, ProgramParameter.InfoLogLength, out infoLogLen);

			/* TODO if (infoLogLen > 0)
				infoLog = GL.GetProgramInfoLog (obj); */
#if DEBUG
			Console.WriteLine ("{0} {1}", whichObj, infoLog);
#endif
		}	
    }
}
