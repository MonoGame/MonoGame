using System;
using System.IO;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using MonoMac.Foundation;
using MonoMac.AppKit;

namespace SpriteEffects
{
	public class DesaturateEffect : Effect
	{
		public DesaturateEffect (GraphicsDevice graphicsDevice) : base (graphicsDevice)
		{
			LoadShaderFromFile ("desaturate.fsh");
			DefineTechnique ("Desaturate", "Pass1", 0, 0);
			CurrentTechnique = Techniques ["Desaturate"];
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

