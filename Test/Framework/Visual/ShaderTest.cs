#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NUnit.Framework;

namespace MonoGame.Tests.Visual {
	[TestFixture]
	class ShaderTest : VisualTestFixtureBase {
		[TestCase ("NoEffect")]
		[TestCase ("HighContrast")]
		[TestCase ("Bevels")]
		[TestCase ("Grayscale")]
		[TestCase ("ColorFlip")]
		[TestCase ("Invert")]
		[TestCase ("BlackOut")]
		[TestCase ("RainbowH")]
		public void Shader (string effectName)
		{
			SpriteBatch spriteBatch = null;
			Effect effect = null;

			// A background texture to test that the effect doesn't
			// mess up other textures
			Texture2D background = null;

			// The texture to apply the effect to
			Texture2D surge = null;

#if XNA
            effectName = Path.Combine("XNA", effectName);
#elif WINDOWS
            effectName = Path.Combine("DirectX", effectName);
#endif

			Game.LoadContentWith += (sender, e) => {
				spriteBatch = new SpriteBatch (Game.GraphicsDevice);
				effect = Game.Content.Load<Effect> (Paths.Effect (effectName));
				background = Game.Content.Load<Texture2D> (Paths.Texture ("fun-background"));
				surge = Game.Content.Load<Texture2D> (Paths.Texture ("Surge"));
			};

			Game.UnloadContentWith += (sender, e) => {
				spriteBatch.Dispose ();
			};

			Game.DrawWith += (sender, e) => {
				spriteBatch.Begin (SpriteSortMode.Immediate, BlendState.AlphaBlend);
				spriteBatch.Draw (background, Vector2.Zero, Color.White);

				effect.CurrentTechnique.Passes [0].Apply ();
				spriteBatch.Draw (
					surge, new Vector2 (300, 200), null, Color.White,
					0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0f);
				spriteBatch.End ();
			};

			RunSingleFrameTest ();
		}
	}
}
