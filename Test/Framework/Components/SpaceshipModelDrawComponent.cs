#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team
Copyright © Microsoft Corporation.

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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Tests.Components
{
	class SpaceshipModelDrawComponent : VisualTestDrawableGameComponent
	{

		public class Spaceship
		{
			#region Fields
			// The XNA framework Model object that we are going to display.
			Model spaceshipModel;
	
			// Array holding all the bone transform matrices for the entire model.
			// We could just allocate this locally inside the Draw method, but it
			// is more efficient to reuse a single array, as this avoids creating
			// unnecessary garbage.
			Matrix[] boneTransforms;
	
			// Spaceship drawing parameters
			private Matrix projection;
			private Matrix rotation;
			private Matrix view;
			private bool[] lights;
			bool isTextureEnabled;
			bool isPerPixelLightingEnabled;
			#endregion
	
			#region Initialization
			/// <summary>
			/// Loads the spaceship model.
			/// </summary>
			public void Load(ContentManager content)
			{
				// Load the spaceship model from the ContentManager.
				spaceshipModel = content.Load<Model>(Paths.Model("Spaceship"));
	
				// Allocate the transform matrix array.
				boneTransforms = new Matrix[spaceshipModel.Bones.Count];
			}
			#endregion
	
			#region Public accessors
			/// <summary>
			/// Gets or sets the projection matrix value.
			/// </summary>
			public Matrix Projection
			{
				get { return projection; }
				set { projection = value; }
			}
	
			/// <summary>
			/// Gets or sets the rotation matrix value.
			/// </summary>
			public Matrix Rotation
			{
				get { return rotation; }
				set { rotation = value; }
			}
	
			/// <summary>
			/// Gets or sets the rotation matrix value.
			/// </summary>
			public bool IsTextureEnabled
			{
				get { return isTextureEnabled; }
				set { isTextureEnabled = value; }
			}
	
			/// <summary>
			/// Gets or sets the view matrix value.
			/// </summary>
			public Matrix View
			{
				get { return view; }
				set { view = value; }
			}
	
			/// <summary>
			/// Gets or sets the lights states.
			/// </summary>
			public bool[] Lights
			{
				get { return lights; }
				set { lights = value; }
			}
	
			/// <summary>
			/// Gets or sets the per pixel lighting preferences
			/// </summary>
			public bool IsPerPixelLightingEnabled
			{
				get { return isPerPixelLightingEnabled; }
				set { isPerPixelLightingEnabled = value; }
			}
			#endregion
	
			#region Draw
			/// <summary>
			/// Draws the spaceship model, using the current drawing parameters.
			/// </summary>
			public void Draw()
			{
				// Set the world matrix as the root transform of the model.
				spaceshipModel.Root.Transform = Rotation;
	
				// Look up combined bone matrices for the entire model.
				spaceshipModel.CopyAbsoluteBoneTransformsTo(boneTransforms);
	
				// Draw the model.
				foreach (ModelMesh mesh in spaceshipModel.Meshes)
				{
					foreach (BasicEffect effect in mesh.Effects)
					{
						effect.World = boneTransforms[mesh.ParentBone.Index];
						effect.View = View;
						effect.Projection = Projection;
	
						SetEffectLights(effect, Lights);
						SetEffectPerPixelLightingEnabled(effect);
	
						effect.TextureEnabled = IsTextureEnabled;
					}
	
					mesh.Draw();
				}
			}
	
			/// <summary>
			/// Sets effect's per pixel lighting preference
			/// </summary>
			/// <param name="effect"></param>
			private void SetEffectPerPixelLightingEnabled(BasicEffect effect)
			{
				effect.PreferPerPixelLighting = isPerPixelLightingEnabled;
			}
	
			/// <summary>
			/// Sets effects lighting properties
			/// </summary>
			/// <param name="effect"></param>
			/// <param name="lights"></param>
			private void SetEffectLights(BasicEffect effect, bool[] lights)
			{
				effect.Alpha = 1.0f;
				effect.DiffuseColor = new Vector3(0.75f, 0.75f, 0.75f);
				effect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
				effect.SpecularPower = 5.0f;
				effect.AmbientLightColor = new Vector3(0.75f, 0.75f, 0.75f);
	
				effect.DirectionalLight0.Enabled = lights[0];
				effect.DirectionalLight0.DiffuseColor = Vector3.One;
				effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(1, -1, 0));
				effect.DirectionalLight0.SpecularColor = Vector3.One;
	
				effect.DirectionalLight1.Enabled = lights[1];
				effect.DirectionalLight1.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
				effect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(-1, -1, 0));
				effect.DirectionalLight1.SpecularColor = new Vector3(1f, 1f, 1f);
	
				effect.DirectionalLight2.Enabled = lights[2];
				effect.DirectionalLight2.DiffuseColor = new Vector3(0.3f, 0.3f, 0.3f);
				effect.DirectionalLight2.Direction = Vector3.Normalize(new Vector3(-1, -1, -1));
				effect.DirectionalLight2.SpecularColor = new Vector3(0.3f, 0.3f, 0.3f);
	
				effect.LightingEnabled = true;
			}
			#endregion
	
		}

		Spaceship spaceship;
		float cameraFOV = 45; // Initial camera FOV (serves as a zoom level)
		float rotationXAmount = 0.0f;
		float rotationYAmount = 0.0f;

		Texture2D background;

		SpriteBatch spriteBatch;


		public SpaceshipModelDrawComponent (Game game) : base(game)
		{
		}

		protected override void LoadContent ()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);

			background = Game.Content.Load<Texture2D>(Paths.Texture("fun-background"));

			spaceship = new Spaceship();
			spaceship.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(cameraFOV),
				GraphicsDevice.Viewport.AspectRatio, 10, 20000);
			spaceship.Load (Game.Content);


			base.LoadContent();
		}

		protected override void UnloadContent()
		{
			spriteBatch.Dispose ();
			spriteBatch = null;
			background.Dispose ();
			background = null;
		}

		protected override void UpdateOncePerDraw(GameTime gameTime)
		{
			var frameInfoSource = Game.Services.RequireService<IFrameInfoSource> ();
			var frameInfo = frameInfoSource.FrameInfo;

			spaceship.Rotation = Matrix.CreateWorld(new Vector3(0, 250, 0), Vector3.Forward, Vector3.Up) *
				Matrix.CreateFromYawPitchRoll((float)Math.PI + MathHelper.PiOver2 + rotationXAmount / 100, rotationYAmount / 100, 0);

			spaceship.View = Matrix.CreateLookAt (
				new Vector3(3500, 400, 0) + new Vector3(0, 250, 0),
				new Vector3(0, 250, 0),
				Vector3.Up);
			spaceship.IsTextureEnabled = true;
			spaceship.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(cameraFOV),
				GraphicsDevice.Viewport.AspectRatio, 10, 20000);


			int frameNum = frameInfo.DrawNumber-1;
			spaceship.Lights = new bool[] {
				(frameNum & 1) != 0,
				(frameNum & 2) != 0,
				(frameNum & 4) != 0
			};
			spaceship.IsPerPixelLightingEnabled = (frameNum & 8) != 0;

			rotationXAmount += 10;
			rotationYAmount += 12;
		}

		public override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			spriteBatch.Begin();
			spriteBatch.Draw (background, Vector2.Zero, Color.White);
			spriteBatch.End ();

			// Set render states.
			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

			spaceship.Draw ();

			base.Draw(gameTime);
		}
	}
}

