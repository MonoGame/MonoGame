using System;
using System.IO;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using MonoMac.Foundation;
using MonoMac.AppKit;

namespace SpriteEffects
{
	public class RefractionEffect : Effect
	{
		public RefractionEffect (GraphicsDevice graphicsDevice) : base (graphicsDevice)
		{
//			CreateVertexShaderFromSource("void main(void) { " +
//				"gl_FrontColor = gl_Color; " +
//				"gl_TexCoord[0] = gl_MultiTexCoord0;" +
//				"gl_TexCoord[1] = gl_MultiTexCoord1;" +
//				"gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;" +
//				"}");
			LoadShaderFromFile ("refraction.fsh");
			
			DefineTechnique ("Refraction", "Pass1", 0, 0);
			CurrentTechnique = Techniques ["Refraction"];
		}

		protected void LoadShaderFromFile (string sourceFile)
		{
			var path = Path.Combine (NSBundle.MainBundle.ResourcePath, "Content");
			sourceFile = Path.Combine (path, sourceFile);

			// Load the source into a string
			string shaderSource = LoadShaderSource (sourceFile);

			CreateFragmentShaderFromSource (shaderSource);				

		}

		// Load the source code of a GLSL program from the content
		private string LoadShaderSource (string name)
		{

			StreamReader streamReader = new StreamReader (name);
			string text = streamReader.ReadToEnd ();
			streamReader.Close ();

			return text;

		}
	}
}

