// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Tests.Components {
	class Simple3DCubeComponent : VisualTestDrawableGameComponent {
		BasicEffect basicEffect;

		Matrix worldMatrix, viewMatrix, projectionMatrix;

        public Vector3 CubePosition { get; set; }
	    public Color CubeColor { get; set; }

        public Simple3DCubeComponent(Game game)
            : base(game)
		{
		}

		protected override void LoadContent ()
		{
			base.LoadContent();

			// setup our graphics scene matrices
			worldMatrix = Matrix.Identity;
			viewMatrix = Matrix.CreateLookAt (new Vector3 (0, 0, 5), Vector3.Zero, Vector3.Up);
			projectionMatrix = Matrix.CreatePerspectiveFieldOfView (MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 10);

			worldMatrix *= Matrix.CreateRotationX (-0.05f * 30f);
			worldMatrix *= Matrix.CreateRotationY (-0.05f * 20f);

			// Setup our basic effect
			basicEffect = new BasicEffect (GraphicsDevice);
			basicEffect.World = worldMatrix;
			basicEffect.View = viewMatrix;
			basicEffect.Projection = projectionMatrix;

			CreateCubeVertexBuffer ();
			CreateCubeIndexBuffer ();
		}

		protected override void UnloadContent ()
		{
			indices.Dispose ();
			indices = null;
			vertices.Dispose ();
			vertices = null;

			base.UnloadContent ();
		}

		public override void Draw (GameTime gameTime)
		{
			GraphicsDevice.SetVertexBuffer (vertices);
			GraphicsDevice.Indices = indices;

			basicEffect.World = worldMatrix * Matrix.CreateTranslation(CubePosition);
            basicEffect.DiffuseColor = CubeColor.ToVector3();

			foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) {
                pass.Apply();

				GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, number_of_vertices, 0, number_of_indices / 3);
			}

			base.Draw (gameTime);
		}

		const int number_of_vertices = 8;
		const int number_of_indices = 36;
		VertexBuffer vertices;

		void CreateCubeVertexBuffer ()
		{
            VertexPositionTexture[] cubeVertices = new VertexPositionTexture[number_of_vertices];

			cubeVertices [0].Position = new Vector3 (-1, -1, -1);
			cubeVertices [1].Position = new Vector3 (-1, -1, 1);
			cubeVertices [2].Position = new Vector3 (1, -1, 1);
			cubeVertices [3].Position = new Vector3 (1, -1, -1);
			cubeVertices [4].Position = new Vector3 (-1, 1, -1);
			cubeVertices [5].Position = new Vector3 (-1, 1, 1);
			cubeVertices [6].Position = new Vector3 (1, 1, 1);
			cubeVertices [7].Position = new Vector3 (1, 1, -1);

            vertices = new VertexBuffer(GraphicsDevice, VertexPositionTexture.VertexDeclaration, number_of_vertices, BufferUsage.WriteOnly);
			vertices.SetData(cubeVertices);
		}

		IndexBuffer indices;

		void CreateCubeIndexBuffer ()
		{
			UInt16[] cubeIndices = new UInt16[number_of_indices];

			//bottom face
			cubeIndices [0] = 0;
			cubeIndices [1] = 2;
			cubeIndices [2] = 3;
			cubeIndices [3] = 0;
			cubeIndices [4] = 1;
			cubeIndices [5] = 2;

			//top face
			cubeIndices [6] = 4;
			cubeIndices [7] = 6;
			cubeIndices [8] = 5;
			cubeIndices [9] = 4;
			cubeIndices [10] = 7;
			cubeIndices [11] = 6;

			//front face
			cubeIndices [12] = 5;
			cubeIndices [13] = 2;
			cubeIndices [14] = 1;
			cubeIndices [15] = 5;
			cubeIndices [16] = 6;
			cubeIndices [17] = 2;

			//back face
			cubeIndices [18] = 0;
			cubeIndices [19] = 7;
			cubeIndices [20] = 4;
			cubeIndices [21] = 0;
			cubeIndices [22] = 3;
			cubeIndices [23] = 7;

			//left face
			cubeIndices [24] = 0;
			cubeIndices [25] = 4;
			cubeIndices [26] = 1;
			cubeIndices [27] = 1;
			cubeIndices [28] = 4;
			cubeIndices [29] = 5;

			//right face
			cubeIndices [30] = 2;
			cubeIndices [31] = 6;
			cubeIndices [32] = 3;
			cubeIndices [33] = 3;
			cubeIndices [34] = 6;
			cubeIndices [35] = 7;

			indices = new IndexBuffer (GraphicsDevice, IndexElementSize.SixteenBits, number_of_indices, BufferUsage.WriteOnly);
			indices.SetData<UInt16> (cubeIndices);

		}
	}
}
