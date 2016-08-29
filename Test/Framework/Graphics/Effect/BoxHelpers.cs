// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameTest.Framework
{
    static class BoxHelpers
    {
        public static VertexBuffer GetColorUnitCube(GraphicsDevice device)
        {
            var vb = new VertexBuffer(device, VertexPositionColor.VertexDeclaration, 6 * 4, BufferUsage.None);
            var data = new []
            {

                // Front
                new VertexPositionColor(new Vector3(-0.5f, -0.5f, -0.5f), new Color(255, 0, 0)),
                new VertexPositionColor(new Vector3(0.5f, -0.5f, -0.5f), new Color(255, 0, 0)),
                new VertexPositionColor(new Vector3(0.5f, 0.5f, -0.5f), new Color(255, 0, 0)),
                new VertexPositionColor(new Vector3(-0.5f, 0.5f, -0.5f), new Color(255, 0, 0)),

                // Back
                new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0.5f), new Color(128, 0, 0)),
                new VertexPositionColor(new Vector3(0.5f, -0.5f, 0.5f), new Color(128, 0, 0)),
                new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.5f), new Color(128, 0, 0)),
                new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.5f), new Color(128, 0, 0)),

                // Top
                new VertexPositionColor(new Vector3(-0.5f, -0.5f, -0.5f), new Color(0, 255, 0)),
                new VertexPositionColor(new Vector3(0.5f, -0.5f, -0.5f), new Color(0, 255, 0)),
                new VertexPositionColor(new Vector3(0.5f, -0.5f, 0.5f), new Color(0, 255, 0)),
                new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0.5f), new Color(0, 255, 0)),

                // Bottom
                new VertexPositionColor(new Vector3(-0.5f, 0.5f, -0.5f), new Color(0, 128, 0)),
                new VertexPositionColor(new Vector3(0.5f, 0.5f, -0.5f), new Color(0, 128, 0)),
                new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.5f), new Color(0, 128, 0)),
                new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.5f), new Color(0, 128, 0)),

                // Left
                new VertexPositionColor(new Vector3(-0.5f, -0.5f, -0.5f), new Color(0, 0, 255)),
                new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0.5f), new Color(0, 0, 255)),
                new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.5f), new Color(0, 0, 255)),
                new VertexPositionColor(new Vector3(-0.5f, 0.5f, -0.5f), new Color(0, 0, 255)),

                // Right
                new VertexPositionColor(new Vector3(0.5f, -0.5f, -0.5f), new Color(0, 0, 128)),
                new VertexPositionColor(new Vector3(0.5f, -0.5f, 0.5f), new Color(0, 0, 128)),
                new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.5f), new Color(0, 0, 128)),
                new VertexPositionColor(new Vector3(0.5f, 0.5f, -0.5f), new Color(0, 0, 128)),
            };
            vb.SetData(data);

            return vb;
        }

        public static VertexBuffer GetTextureUnitCube(GraphicsDevice device)
        {
            var vb = new VertexBuffer(device, VertexPositionTexture.VertexDeclaration, 6 * 4, BufferUsage.None);
            var data = new [] {

		        // Front
		        new VertexPositionTexture(new Vector3(-0.5f, -0.5f,  -0.5f),  new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(0.5f, -0.5f,  -0.5f),   new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(0.5f,  0.5f,  -0.5f),   new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(-0.5f,  0.5f,  -0.5f),  new Vector2(1, 0)),

		        // Back
		        new VertexPositionTexture(new Vector3(-0.5f, -0.5f,  0.5f),  new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(0.5f, -0.5f,  0.5f),   new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(0.5f,  0.5f,  0.5f),   new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(-0.5f,  0.5f,  0.5f),  new Vector2(0, 0)),

		        // Top
		        new VertexPositionTexture(new Vector3(-0.5f, -0.5f, -0.5f),  new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(0.5f, -0.5f, -0.5f),   new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(0.5f, -0.5f,  0.5f),   new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(-0.5f, -0.5f,  0.5f),  new Vector2(0, 0)),

		        // Bottom
		        new VertexPositionTexture(new Vector3(-0.5f, 0.5f, -0.5f), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(0.5f, 0.5f, -0.5f),  new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(0.5f, 0.5f,  0.5f),  new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(-0.5f, 0.5f,  0.5f), new Vector2(0, 1)),

		        // Left
		        new VertexPositionTexture(new Vector3(-0.5f, -0.5f, -0.5f),  new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(-0.5f, -0.5f,  0.5f),  new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(-0.5f, 0.5f,  0.5f),   new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(-0.5f, 0.5f, -0.5f),   new Vector2(0, 0)),

		        // Right
		        new VertexPositionTexture(new Vector3(0.5f, -0.5f, -0.5f), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(0.5f, -0.5f,  0.5f), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(0.5f, 0.5f,  0.5f),  new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(0.5f, 0.5f, -0.5f),  new Vector2(1, 0)),
            };
            vb.SetData(data);

            return vb;
        }

        public static VertexBuffer GetColorTextureUnitCube(GraphicsDevice device)
        {
            var vb = new VertexBuffer(device, VertexPositionColorTexture.VertexDeclaration, 6 * 4, BufferUsage.WriteOnly);
            vb.SetData(new VertexPositionColorTexture[] {

		        // Front
		        new VertexPositionColorTexture(new Vector3(-0.5f, -0.5f,  -0.5f), new Color(255, 0, 0),  new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(0.5f, -0.5f,  -0.5f),  new Color(255, 0, 0),  new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(0.5f,  0.5f,  -0.5f),  new Color(255, 0, 0),  new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(-0.5f,  0.5f,  -0.5f), new Color(255, 0, 0),  new Vector2(1, 0)),

		        // Back
		        new VertexPositionColorTexture(new Vector3(-0.5f, -0.5f,  0.5f), new Color(128, 0, 0),  new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(0.5f, -0.5f,  0.5f),  new Color(128, 0, 0),  new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(0.5f,  0.5f,  0.5f),  new Color(128, 0, 0),  new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(-0.5f,  0.5f,  0.5f), new Color(128, 0, 0),  new Vector2(0, 0)),

		        // Top
		        new VertexPositionColorTexture(new Vector3(-0.5f, -0.5f, -0.5f), new Color(0, 255, 0),  new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(0.5f, -0.5f, -0.5f),  new Color(0, 255, 0),  new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(0.5f, -0.5f,  0.5f),  new Color(0, 255, 0),  new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(-0.5f, -0.5f,  0.5f), new Color(0, 255, 0),  new Vector2(0, 0)),

		        // Bottom
		        new VertexPositionColorTexture(new Vector3(-0.5f, 0.5f, -0.5f), new Color(0, 128, 0),  new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(0.5f, 0.5f, -0.5f),  new Color(0, 128, 0),  new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(0.5f, 0.5f,  0.5f),  new Color(0, 128, 0),  new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(-0.5f, 0.5f,  0.5f), new Color(0, 128, 0),  new Vector2(0, 1)),

		        // Left
		        new VertexPositionColorTexture(new Vector3(-0.5f, -0.5f, -0.5f), new Color(0, 0, 255),  new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(-0.5f, -0.5f,  0.5f), new Color(0, 0, 255),  new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(-0.5f, 0.5f,  0.5f),  new Color(0, 0, 255),  new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(-0.5f, 0.5f, -0.5f),  new Color(0, 0, 255),  new Vector2(0, 0)),

		        // Right
		        new VertexPositionColorTexture(new Vector3(0.5f, -0.5f, -0.5f), new Color(0, 0, 128),  new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(0.5f, -0.5f,  0.5f), new Color(0, 0, 128),  new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(0.5f, 0.5f,  0.5f),  new Color(0, 0, 128),  new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(0.5f, 0.5f, -0.5f),  new Color(0, 0, 128),  new Vector2(1, 0)),
            });

            return vb;
        }

        public static VertexBuffer GetNormalColorUnitCube(GraphicsDevice device)
        {
            var vb = new VertexBuffer(device, VertexPositionNormalColor.VertexDeclaration, 6 * 4, BufferUsage.WriteOnly);
            vb.SetData(new VertexPositionNormalColor[] {

		        // Front
		        new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f,  -0.5f), -Vector3.UnitZ,  new Color(255, 0, 0)),
                new VertexPositionNormalColor(new Vector3(0.5f, -0.5f,  -0.5f),  -Vector3.UnitZ, new Color(255, 0, 0)),
                new VertexPositionNormalColor(new Vector3(0.5f,  0.5f,  -0.5f),  -Vector3.UnitZ, new Color(255, 0, 0)),
                new VertexPositionNormalColor(new Vector3(-0.5f,  0.5f,  -0.5f), -Vector3.UnitZ,  new Color(255, 0, 0)),

		        // Back
		        new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f,  0.5f), Vector3.UnitZ,  new Color(128, 0, 0)),
                new VertexPositionNormalColor(new Vector3(0.5f, -0.5f,  0.5f),  Vector3.UnitZ, new Color(128, 0, 0)),
                new VertexPositionNormalColor(new Vector3(0.5f,  0.5f,  0.5f),  Vector3.UnitZ, new Color(128, 0, 0)),
                new VertexPositionNormalColor(new Vector3(-0.5f,  0.5f,  0.5f), Vector3.UnitZ,  new Color(128, 0, 0)),

		        // Top
		        new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f, -0.5f), -Vector3.UnitY,  new Color(0, 255, 0)),
                new VertexPositionNormalColor(new Vector3(0.5f, -0.5f, -0.5f),  -Vector3.UnitY, new Color(0, 255, 0)),
                new VertexPositionNormalColor(new Vector3(0.5f, -0.5f,  0.5f),  -Vector3.UnitY, new Color(0, 255, 0)),
                new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f,  0.5f), -Vector3.UnitY,  new Color(0, 255, 0)),

		        // Bottom
		        new VertexPositionNormalColor(new Vector3(-0.5f, 0.5f, -0.5f), Vector3.UnitY,  new Color(0, 128, 0)),
                new VertexPositionNormalColor(new Vector3(0.5f, 0.5f, -0.5f),  Vector3.UnitY, new Color(0, 128, 0)),
                new VertexPositionNormalColor(new Vector3(0.5f, 0.5f,  0.5f),  Vector3.UnitY, new Color(0, 128, 0)),
                new VertexPositionNormalColor(new Vector3(-0.5f, 0.5f,  0.5f), Vector3.UnitY,  new Color(0, 128, 0)),

		        // Left
		        new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f, -0.5f), -Vector3.UnitX,  new Color(0, 0, 255)),
                new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f,  0.5f), -Vector3.UnitX,  new Color(0, 0, 255)),
                new VertexPositionNormalColor(new Vector3(-0.5f, 0.5f,  0.5f),  -Vector3.UnitX, new Color(0, 0, 255)),
                new VertexPositionNormalColor(new Vector3(-0.5f, 0.5f, -0.5f),  -Vector3.UnitX, new Color(0, 0, 255)),

		        // Right
		        new VertexPositionNormalColor(new Vector3(0.5f, -0.5f, -0.5f), Vector3.UnitX,  new Color(0, 0, 128)),
                new VertexPositionNormalColor(new Vector3(0.5f, -0.5f,  0.5f), Vector3.UnitX,  new Color(0, 0, 128)),
                new VertexPositionNormalColor(new Vector3(0.5f, 0.5f,  0.5f),  Vector3.UnitX, new Color(0, 0, 128)),
                new VertexPositionNormalColor(new Vector3(0.5f, 0.5f, -0.5f),  Vector3.UnitX, new Color(0, 0, 128)),
            });

            return vb;
        }

        public static VertexBuffer GetNormalTextureUnitCube(GraphicsDevice device)
        {
            var vb = new VertexBuffer(device, VertexPositionNormalTexture.VertexDeclaration, 6 * 4, BufferUsage.WriteOnly);
            vb.SetData(new VertexPositionNormalTexture[] {

		        // Front
		        new VertexPositionNormalTexture(new Vector3(-0.5f, -0.5f,  -0.5f), -Vector3.UnitZ,  new Vector2(1, 1)),
                new VertexPositionNormalTexture(new Vector3(0.5f, -0.5f,  -0.5f),  -Vector3.UnitZ,  new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3(0.5f,  0.5f,  -0.5f),  -Vector3.UnitZ,  new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(-0.5f,  0.5f,  -0.5f), -Vector3.UnitZ,  new Vector2(1, 0)),

		        // Back
		        new VertexPositionNormalTexture(new Vector3(-0.5f, -0.5f,  0.5f),  Vector3.UnitZ,  new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3(0.5f, -0.5f,  0.5f),   Vector3.UnitZ,  new Vector2(1, 1)),
                new VertexPositionNormalTexture(new Vector3(0.5f,  0.5f,  0.5f),   Vector3.UnitZ,  new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(-0.5f,  0.5f,  0.5f),  Vector3.UnitZ,  new Vector2(0, 0)),

		        // Top
		        new VertexPositionNormalTexture(new Vector3(-0.5f, -0.5f, -0.5f),  -Vector3.UnitY,  new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3(0.5f, -0.5f, -0.5f),   -Vector3.UnitY,  new Vector2(1, 1)),
                new VertexPositionNormalTexture(new Vector3(0.5f, -0.5f,  0.5f),   -Vector3.UnitY,  new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(-0.5f, -0.5f,  0.5f),  -Vector3.UnitY,  new Vector2(0, 0)),

		        // Bottom
		        new VertexPositionNormalTexture(new Vector3(-0.5f, 0.5f, -0.5f), Vector3.UnitY,  new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(0.5f, 0.5f, -0.5f),  Vector3.UnitY,  new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(0.5f, 0.5f,  0.5f),  Vector3.UnitY,  new Vector2(1, 1)),
                new VertexPositionNormalTexture(new Vector3(-0.5f, 0.5f,  0.5f), Vector3.UnitY,  new Vector2(0, 1)),

		        // Left
		        new VertexPositionNormalTexture(new Vector3(-0.5f, -0.5f, -0.5f),  -Vector3.UnitX,  new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3(-0.5f, -0.5f,  0.5f),  -Vector3.UnitX,  new Vector2(1, 1)),
                new VertexPositionNormalTexture(new Vector3(-0.5f, 0.5f,  0.5f),   -Vector3.UnitX,  new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(-0.5f, 0.5f, -0.5f),   -Vector3.UnitX,  new Vector2(0, 0)),

		        // Right
		        new VertexPositionNormalTexture(new Vector3(0.5f, -0.5f, -0.5f), Vector3.UnitX,  new Vector2(1, 1)),
                new VertexPositionNormalTexture(new Vector3(0.5f, -0.5f,  0.5f), Vector3.UnitX,  new Vector2(0, 1)),
                new VertexPositionNormalTexture(new Vector3(0.5f, 0.5f,  0.5f),  Vector3.UnitX,  new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(0.5f, 0.5f, -0.5f),  Vector3.UnitX,  new Vector2(1, 0)),
            });

            return vb;
        }

        public static VertexBuffer GetNormalColorTextureUnitCube(GraphicsDevice device)
        {
            var vb = new VertexBuffer(device, VertexPositionNormalColorTexture.VertexDeclaration, 6 * 4, BufferUsage.WriteOnly);
            vb.SetData(new VertexPositionNormalColorTexture[] {

		        // Front
		        new VertexPositionNormalColorTexture(new Vector3(-0.5f, -0.5f,  -0.5f), -Vector3.UnitZ,  new Color(255, 0, 0),  new Vector2(1, 1)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, -0.5f,  -0.5f),  -Vector3.UnitZ,  new Color(255, 0, 0),  new Vector2(0, 1)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f,  0.5f,  -0.5f),  -Vector3.UnitZ,  new Color(255, 0, 0),  new Vector2(0, 0)),
                new VertexPositionNormalColorTexture(new Vector3(-0.5f,  0.5f,  -0.5f), -Vector3.UnitZ,  new Color(255, 0, 0),  new Vector2(1, 0)),

		        // Back
		        new VertexPositionNormalColorTexture(new Vector3(-0.5f, -0.5f,  0.5f), Vector3.UnitZ,  new Color(128, 0, 0),  new Vector2(0, 1)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, -0.5f,  0.5f),  Vector3.UnitZ,  new Color(128, 0, 0),  new Vector2(1, 1)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f,  0.5f,  0.5f),  Vector3.UnitZ,  new Color(128, 0, 0),  new Vector2(1, 0)),
                new VertexPositionNormalColorTexture(new Vector3(-0.5f,  0.5f,  0.5f), Vector3.UnitZ,  new Color(128, 0, 0),  new Vector2(0, 0)),

		        // Top
		        new VertexPositionNormalColorTexture(new Vector3(-0.5f, -0.5f, -0.5f), -Vector3.UnitY, new Color(0, 255, 0),  new Vector2(0, 1)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, -0.5f, -0.5f),  -Vector3.UnitY, new Color(0, 255, 0),  new Vector2(1, 1)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, -0.5f,  0.5f),  -Vector3.UnitY, new Color(0, 255, 0),  new Vector2(1, 0)),
                new VertexPositionNormalColorTexture(new Vector3(-0.5f, -0.5f,  0.5f), -Vector3.UnitY, new Color(0, 255, 0),  new Vector2(0, 0)),

		        // Bottom
		        new VertexPositionNormalColorTexture(new Vector3(-0.5f, 0.5f, -0.5f),  Vector3.UnitY, new Color(0, 128, 0),  new Vector2(0, 0)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, 0.5f, -0.5f),   Vector3.UnitY, new Color(0, 128, 0),  new Vector2(1, 0)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, 0.5f,  0.5f),   Vector3.UnitY, new Color(0, 128, 0),  new Vector2(1, 1)),
                new VertexPositionNormalColorTexture(new Vector3(-0.5f, 0.5f,  0.5f),  Vector3.UnitY, new Color(0, 128, 0),  new Vector2(0, 1)),

		        // Left
		        new VertexPositionNormalColorTexture(new Vector3(-0.5f, -0.5f, -0.5f),  -Vector3.UnitX, new Color(0, 0, 255),  new Vector2(0, 1)),
                new VertexPositionNormalColorTexture(new Vector3(-0.5f, -0.5f,  0.5f),  -Vector3.UnitX, new Color(0, 0, 255),  new Vector2(1, 1)),
                new VertexPositionNormalColorTexture(new Vector3(-0.5f, 0.5f,  0.5f),   -Vector3.UnitX, new Color(0, 0, 255),  new Vector2(1, 0)),
                new VertexPositionNormalColorTexture(new Vector3(-0.5f, 0.5f, -0.5f),   -Vector3.UnitX, new Color(0, 0, 255),  new Vector2(0, 0)),

		        // Right
		        new VertexPositionNormalColorTexture(new Vector3(0.5f, -0.5f, -0.5f),  Vector3.UnitX, new Color(0, 0, 128),  new Vector2(1, 1)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, -0.5f,  0.5f),  Vector3.UnitX, new Color(0, 0, 128),  new Vector2(0, 1)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, 0.5f,  0.5f),   Vector3.UnitX, new Color(0, 0, 128),  new Vector2(0, 0)),
                new VertexPositionNormalColorTexture(new Vector3(0.5f, 0.5f, -0.5f),   Vector3.UnitX, new Color(0, 0, 128),  new Vector2(1, 0)),
            });

            return vb;
        }

        public static IndexBuffer GetCubeIndexBuffer(GraphicsDevice device)
        {
            var ib = new IndexBuffer(device, IndexElementSize.SixteenBits, 6 * 2 * 3, BufferUsage.None);
            var data = new short[] {
		        // Front
		        0, 1, 2,
		        2, 3, 0,

		        // Back
		        6, 5, 4,
		        4, 7, 6,

		        // Top
		        10, 9, 8,
		        8, 11, 10,

		        // Bottom
		        12, 13, 14,
		        14, 15, 12,

		        // Left
		        18, 17, 16,
		        16, 19, 18,

		        // Right
		        20, 21, 22,
		        22, 23, 20,
            };
            ib.SetData(data);

            return ib;
        }
    }
}