using System;
using System.IO;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

#if !LINUX

using MonoMac.Foundation;
using MonoMac.AppKit;

#endif

namespace SpriteEffects
{
	public class DisappearEffect : Effect
	{
		public DisappearEffect (GraphicsDevice graphicsDevice) : base (graphicsDevice)
		{
			// We do not need this but here for test
//			CreateVertexShaderFromSource("void main(void) { " +
//				"gl_FrontColor = gl_Color; " +
//				"gl_TexCoord[0] = gl_MultiTexCoord0;" +
//				"gl_TexCoord[1] = gl_MultiTexCoord1;" +
//				"gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;" +
//				"}");
			LoadShaderFromFile ("disappear.fsh");
			
			DefineTechnique ("Disappear", "Pass1", 0, 0);
			CurrentTechnique = Techniques ["Disappear"];
		}

		protected void LoadShaderFromFile (string sourceFile)
		{
			string path;
#if !LINUX
			path = Path.Combine (NSBundle.MainBundle.ResourcePath, "Content");
#else
			path = "Content";
#endif
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
		
//void main(void){	
//	gl_TexCoord[0] = gl_MultiTexCoord0;	
//	gl_TexCoord[1] = gl_MultiTexCoord1;		
//	gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex; 
//}
//
//uniform sampler2D keke;
//uniform sampler2D smee;
//
//void main (void){	
//vec2 src = vec2(gl_TexCoord[0]);	
//vec4 dest = texture2D(keke, src);		
//vec2 src2 = vec2(gl_TexCoord[1]);	
//vec4 dest2 = texture2D(smee, src2);			
//gl_FragColor = 0.5 * (dest + dest2);  
//}
//
//ARBShaderObjects.glUseProgramObjectARB(shader.getProgramID());		
//int location = shader.getUniformLocation(shader.getProgramID(), "keke");
//int location2 = shader.getUniformLocation(shader.getProgramID(), "smee");
//ARBMultitexture.glActiveTextureARB(ARBMultitexture.GL_TEXTURE0_ARB);
//GL11.glBindTexture(GL11.GL_TEXTURE_2D, TestEngine.cID);
//GL11.glEnable(GL11.GL_TEXTURE_2D);
//ARBShaderObjects.glUniform1iARB(location, 0);
//ARBMultitexture.glActiveTextureARB(ARBMultitexture.GL_TEXTURE1_ARB);
//GL11.glBindTexture(GL11.GL_TEXTURE_2D, sprite.getTextureID());
//GL11.glEnable(GL11.GL_TEXTURE_2D);	
//ARBShaderObjects.glUniform1iARB(location2, 1);
//EngineFunc.translate(origin.x, origin.y);
//EngineFunc.drawQuads(sprite.getPoints());		
//ARBShaderObjects.glUseProgramObjectARB(0);		
//ARBMultitexture.glActiveTextureARB(ARBMultitexture.GL_TEXTURE1_ARB);
//GL11.glDisable(GL11.GL_TEXTURE_2D);		
//ARBMultitexture.glActiveTextureARB(ARBMultitexture.GL_TEXTURE0_ARB);
//GL11.glDisable(GL11.GL_TEXTURE_2D);
		

	}
}

