#region File Description
//-----------------------------------------------------------------------------
// ParticleVertex.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

#endregion

namespace Particle3DSample
{
	/// <summary>
	/// Custom vertex structure for drawing particles.
	/// </summary>
	struct ParticleVertex
	{
		// Stores which corner of the particle quad this vertex represents.
		public Short2 Corner;

		// Stores the starting position of the particle.
		public Vector3 Position;

		// Stores the starting velocity of the particle.
		public Vector3 Velocity;

		// Four random values, used to make each particle look slightly different.
		public Color Random;

		// The time (in seconds) at which this particle was created.
		public float Time;


		// Describe the layout of this vertex structure.
		public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
		(
			new VertexElement (0, VertexElementFormat.Short2,
					VertexElementUsage.Position, 0),
	
			new VertexElement (4, VertexElementFormat.Vector3,
					VertexElementUsage.Position, 1),
	
			new VertexElement (16, VertexElementFormat.Vector3,
					VertexElementUsage.Normal, 0),
	
			new VertexElement (28, VertexElementFormat.Color,
					VertexElementUsage.Color, 0),
	
			new VertexElement (32, VertexElementFormat.Single,
					VertexElementUsage.TextureCoordinate, 0)
		);


		// Describe the size of this vertex structure.
		public const int SizeInBytes = 36;
	}
}
