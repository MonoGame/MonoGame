using System;
using System.IO;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

#if !LINUX

using MonoMac.Foundation;
using MonoMac.AppKit;

#endif
namespace NetRumble
{
	public class BloomExtractEffect : Effect
	{
		public BloomExtractEffect (GraphicsDevice graphicsDevice) : base (graphicsDevice)
		{
			// We do not need this but here for test
			LoadShaderFromFile ("BloomPostprocess/Effects/BloomExtract.fsh");
			
			DefineTechnique ("Technique1", "Pass1", 0, 0);
			CurrentTechnique = Techniques ["Technique1"];
		}

		protected void LoadShaderFromFile (string sourceFile)
		{
			string path;
#if !LINUX
			path = Path.Combine (NSBundle.MainBundle.ResourcePath, "Content");
#else
			path = "Content";
#endif
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

