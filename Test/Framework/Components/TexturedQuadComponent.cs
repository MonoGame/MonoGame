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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Tests.Components
{
	class TexturedQuadComponent : VisualTestDrawableGameComponent {

		struct Quad {
			public Vector3 Origin;
			public Vector3 UpperLeft;
			public Vector3 LowerLeft;
			public Vector3 UpperRight;
			public Vector3 LowerRight;
			public Vector3 Normal;
			public Vector3 Up;
			public Vector3 Left;
	
			public VertexPositionNormalTexture[] Vertices;
			public short[] Indexes;
	
	
			public Quad( Vector3 origin, Vector3 normal, Vector3 up, 
				float width, float height )
			{
				Vertices = new VertexPositionNormalTexture[4];
				Indexes = new short[6];
				Origin = origin;
				Normal = normal;
				Up = up;
	
				// Calculate the quad corners
				Left = Vector3.Cross( normal, Up );
				Vector3 uppercenter = (Up * height / 2) + origin;
				UpperLeft = uppercenter + (Left * width / 2);
				UpperRight = uppercenter - (Left * width / 2);
				LowerLeft = UpperLeft - (Up * height);
				LowerRight = UpperRight - (Up * height);
	
				FillVertices();
			}
			
			private void FillVertices()
			{
				// Fill in texture coordinates to display full texture
				// on quad
				Vector2 textureUpperLeft = new Vector2( 0.0f, 0.0f );
				Vector2 textureUpperRight = new Vector2( 1.0f, 0.0f );
				Vector2 textureLowerLeft = new Vector2( 0.0f, 1.0f );
				Vector2 textureLowerRight = new Vector2( 1.0f, 1.0f );
	
				// Provide a normal for each vertex
				for (int i = 0; i < Vertices.Length; i++)
				{
					Vertices[i].Normal = Normal;
				}
	
				// Set the position and texture coordinate for each
				// vertex
				Vertices[0].Position = LowerLeft;
				Vertices[0].TextureCoordinate = textureLowerLeft;
				Vertices[1].Position = UpperLeft;
				Vertices[1].TextureCoordinate = textureUpperLeft;
				Vertices[2].Position = LowerRight;
				Vertices[2].TextureCoordinate = textureLowerRight;
				Vertices[3].Position = UpperRight;
				Vertices[3].TextureCoordinate = textureUpperRight;
	
				// Set the index buffer for each vertex, using
				// clockwise winding
				Indexes[0] = 0;
				Indexes[1] = 1;
				Indexes[2] = 2;
				Indexes[3] = 2;
				Indexes[4] = 1;
				Indexes[5] = 3;
			}
		}


		Quad quad;
		Matrix View, Projection;

		BasicEffect quadEffect;

		Texture2D texture;

		bool enableLighting;

		public TexturedQuadComponent (Game game, bool enableLighting) : base(game)
		{
			this.enableLighting = enableLighting;
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			quad = new Quad(Vector3.Zero, Vector3.Backward, Vector3.Up, 1, 1);
			View = Matrix.CreateLookAt(new Vector3(0, 0, 2), Vector3.Zero, Vector3.Up);
			Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 4.0f / 3.0f, 1, 500);

			texture = Game.Content.Load<Texture2D>(Paths.Texture("GlassPane"));
			quadEffect = new BasicEffect(GraphicsDevice);

			if (enableLighting)
				quadEffect.EnableDefaultLighting();

			quadEffect.World = Matrix.Identity;
			quadEffect.View = View;
			quadEffect.Projection = Projection;
			quadEffect.TextureEnabled = true;
			quadEffect.Texture = texture;

		}

		protected override void UnloadContent()
		{
			texture.Dispose ();
			texture = null;
			quadEffect.Dispose ();
			quadEffect = null;
		}

		public override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			foreach (EffectPass pass in quadEffect.CurrentTechnique.Passes)
			{
				pass.Apply();


				GraphicsDevice.DrawUserIndexedPrimitives
					<VertexPositionNormalTexture>(
					PrimitiveType.TriangleList,
					quad.Vertices, 0, 4,
					quad.Indexes, 0, 2);
			}

			base.Draw(gameTime);
		}

	}
}

