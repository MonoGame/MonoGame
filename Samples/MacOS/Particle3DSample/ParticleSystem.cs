#region File Description
//-----------------------------------------------------------------------------
// ParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

#endregion

namespace Particle3DSample
{
	/// <summary>
	/// The main component in charge of displaying particles.
	/// </summary>
	public class ParticleSystem : DrawableGameComponent
	{
	#region Fields


		// Name of the XML settings file describing this particle system.
		string settingsName;


		// Settings class controls the appearance and animation of this particle system.
		ParticleSettings settings;


		// For loading the effect and particle texture.
		ContentManager content;


		// Custom effect for drawing particles. This computes the particle
		// animation entirely in the vertex shader: no per-particle CPU work required!
		Effect particleEffect;


		// Shortcuts for accessing frequently changed effect parameters.
		EffectParameter effectViewParameter;
		EffectParameter effectProjectionParameter;
		EffectParameter effectViewportScaleParameter;
		EffectParameter effectTimeParameter;


		// An array of particles, treated as a circular queue.
		ParticleVertex[] particles;


		// A vertex buffer holding our particles. This contains the same data as
		// the particles array, but copied across to where the GPU can access it.
		DynamicVertexBuffer vertexBuffer;


		// Index buffer turns sets of four vertices into particle quads (pairs of triangles).
		IndexBuffer indexBuffer;


		// The particles array and vertex buffer are treated as a circular queue.
		// Initially, the entire contents of the array are free, because no particles
		// are in use. When a new particle is created, this is allocated from the
		// beginning of the array. If more than one particle is created, these will
		// always be stored in a consecutive block of array elements. Because all
		// particles last for the same amount of time, old particles will always be
		// removed in order from the start of this active particle region, so the
		// active and free regions will never be intermingled. Because the queue is
		// circular, there can be times when the active particle region wraps from the
		// end of the array back to the start. The queue uses modulo arithmetic to
		// handle these cases. For instance with a four entry queue we could have:
		//
		//      0
		//      1 - first active particle
		//      2 
		//      3 - first free particle
		//
		// In this case, particles 1 and 2 are active, while 3 and 4 are free.
		// Using modulo arithmetic we could also have:
		//
		//      0
		//      1 - first free particle
		//      2 
		//      3 - first active particle
		//
		// Here, 3 and 0 are active, while 1 and 2 are free.
		//
		// But wait! The full story is even more complex.
		//
		// When we create a new particle, we add them to our managed particles array.
		// We also need to copy this new data into the GPU vertex buffer, but we don't
		// want to do that straight away, because setting new data into a vertex buffer
		// can be an expensive operation. If we are going to be adding several particles
		// in a single frame, it is faster to initially just store them in our managed
		// array, and then later upload them all to the GPU in one single call. So our
		// queue also needs a region for storing new particles that have been added to
		// the managed array but not yet uploaded to the vertex buffer.
		//
		// Another issue occurs when old particles are retired. The CPU and GPU run
		// asynchronously, so the GPU will often still be busy drawing the previous
		// frame while the CPU is working on the next frame. This can cause a
		// synchronization problem if an old particle is retired, and then immediately
		// overwritten by a new one, because the CPU might try to change the contents
		// of the vertex buffer while the GPU is still busy drawing the old data from
		// it. Normally the graphics driver will take care of this by waiting until
		// the GPU has finished drawing inside the VertexBuffer.SetData call, but we
		// don't want to waste time waiting around every time we try to add a new
		// particle! To avoid this delay, we can specify the SetDataOptions.NoOverwrite
		// flag when we write to the vertex buffer. This basically means "I promise I
		// will never try to overwrite any data that the GPU might still be using, so
		// you can just go ahead and update the buffer straight away". To keep this
		// promise, we must avoid reusing vertices immediately after they are drawn.
		//
		// So in total, our queue contains four different regions:
		//
		// Vertices between firstActiveParticle and firstNewParticle are actively
		// being drawn, and exist in both the managed particles array and the GPU
		// vertex buffer.
		//
		// Vertices between firstNewParticle and firstFreeParticle are newly created,
		// and exist only in the managed particles array. These need to be uploaded
		// to the GPU at the start of the next draw call.
		//
		// Vertices between firstFreeParticle and firstRetiredParticle are free and
		// waiting to be allocated.
		//
		// Vertices between firstRetiredParticle and firstActiveParticle are no longer
		// being drawn, but were drawn recently enough that the GPU could still be
		// using them. These need to be kept around for a few more frames before they
		// can be reallocated.

		int firstActiveParticle;
		int firstNewParticle;
		int firstFreeParticle;
		int firstRetiredParticle;


		// Store the current time, in seconds.
		float currentTime;


		// Count how many times Draw has been called. This is used to know
		// when it is safe to retire old particles back into the free list.
		int drawCounter;


		// Shared random number generator.
		static Random random = new Random ();


	#endregion

	#region Initialization


		/// <summary>
		/// Constructor.
		/// </summary>
		public ParticleSystem (Game game,ContentManager content,string settingsName)
		: base(game)
		{
			this.content = content;
			this.settingsName = settingsName;
		}
		
		/// <summary>
		/// Loads graphics for the particle system.
		/// </summary>
		protected override void LoadContent ()
		{
			//return;
			settings = content.Load<ParticleSettings> (settingsName);

			// Allocate the particle array, and fill in the corner fields (which never change).
			particles = new ParticleVertex[settings.MaxParticles * 4];

			for (int i = 0; i < settings.MaxParticles; i++) {
				particles [i * 4 + 0].Corner = new Short2 (-1, -1);
				particles [i * 4 + 1].Corner = new Short2 (1, -1);
				particles [i * 4 + 2].Corner = new Short2 (1, 1);
				particles [i * 4 + 3].Corner = new Short2 (-1, 1);
			}

			LoadParticleEffect ();

			// Create a dynamic vertex buffer.
			vertexBuffer = new DynamicVertexBuffer (GraphicsDevice, ParticleVertex.VertexDeclaration,
						settings.MaxParticles * 4, BufferUsage.WriteOnly);

			// Create and populate the index buffer.
			ushort[] indices = new ushort[settings.MaxParticles * 6];

			for (int i = 0; i < settings.MaxParticles; i++) {
				indices [i * 6 + 0] = (ushort)(i * 4 + 0);
				indices [i * 6 + 1] = (ushort)(i * 4 + 1);
				indices [i * 6 + 2] = (ushort)(i * 4 + 2);

				indices [i * 6 + 3] = (ushort)(i * 4 + 0);
				indices [i * 6 + 4] = (ushort)(i * 4 + 2);
				indices [i * 6 + 5] = (ushort)(i * 4 + 3);
			}

			indexBuffer = new IndexBuffer (GraphicsDevice, typeof(ushort), indices.Length, BufferUsage.WriteOnly);

			indexBuffer.SetData (indices);
		}


		/// <summary>
		/// Helper for loading and initializing the particle effect.
		/// </summary>
		void LoadParticleEffect ()
		{
			//Effect effect = content.Load<Effect> ("ParticleEffect");
			Effect effect = new ParticleEffect(GraphicsDevice);
			
			// If we have several particle systems, the content manager will return
			// a single shared effect instance to them all. But we want to preconfigure
			// the effect with parameters that are specific to this particular
			// particle system. By cloning the effect, we prevent one particle system
			// from stomping over the parameter settings of another.

			//particleEffect = effect.Clone ();
			// No cloning for now so we will just create a new effect for now
			particleEffect = effect;
			
			EffectParameterCollection parameters = particleEffect.Parameters;

			// Look up shortcuts for parameters that change every frame.
			effectViewParameter = parameters ["View"];
			effectProjectionParameter = parameters ["Projection"];
			effectViewportScaleParameter = parameters ["ViewportScale"];
			effectTimeParameter = parameters ["CurrentTime"];

			// Set the values of parameters that do not change.
			parameters ["Duration"].SetValue ((float)settings.Duration.TotalSeconds);
			parameters ["DurationRandomness"].SetValue (settings.DurationRandomness);
			parameters ["Gravity"].SetValue (settings.Gravity);
			parameters ["EndVelocity"].SetValue (settings.EndVelocity);
			parameters ["MinColor"].SetValue (settings.MinColor.ToVector4 ());
			parameters ["MaxColor"].SetValue (settings.MaxColor.ToVector4 ());

			parameters ["RotateSpeed"].SetValue (
				new Vector2 (settings.MinRotateSpeed, settings.MaxRotateSpeed));

			parameters ["StartSize"].SetValue (
				new Vector2 (settings.MinStartSize, settings.MaxStartSize));

			parameters ["EndSize"].SetValue (
				new Vector2 (settings.MinEndSize, settings.MaxEndSize));

			// Load the particle texture, and set it onto the effect.
			Texture2D texture = content.Load<Texture2D> (settings.TextureName);

			parameters ["Texture"].SetValue (texture);
		}


	#endregion

	#region Update and Draw


		/// <summary>
		/// Updates the particle system.
		/// </summary>
		public override void Update (GameTime gameTime)
		{
			if (gameTime == null)
				throw new ArgumentNullException ("gameTime");

			currentTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

			RetireActiveParticles ();
			FreeRetiredParticles ();

			// If we let our timer go on increasing for ever, it would eventually
			// run out of floating point precision, at which point the particles
			// would render incorrectly. An easy way to prevent this is to notice
			// that the time value doesn't matter when no particles are being drawn,
			// so we can reset it back to zero any time the active queue is empty.

			if (firstActiveParticle == firstFreeParticle)
				currentTime = 0;

			if (firstRetiredParticle == firstActiveParticle)
				drawCounter = 0;
		}


		/// <summary>
		/// Helper for checking when active particles have reached the end of
		/// their life. It moves old particles from the active area of the queue
		/// to the retired section.
		/// </summary>
		void RetireActiveParticles ()
		{
			float particleDuration = (float)settings.Duration.TotalSeconds;

			while (firstActiveParticle != firstNewParticle) {
				// Is this particle old enough to retire?
				// We multiply the active particle index by four, because each
				// particle consists of a quad that is made up of four vertices.
				float particleAge = currentTime - particles [firstActiveParticle * 4].Time;

				if (particleAge < particleDuration)
					break;

				// Remember the time at which we retired this particle.
				particles [firstActiveParticle * 4].Time = drawCounter;

				// Move the particle from the active to the retired queue.
				firstActiveParticle++;

				if (firstActiveParticle >= settings.MaxParticles)
					firstActiveParticle = 0;
			}
		}


		/// <summary>
		/// Helper for checking when retired particles have been kept around long
		/// enough that we can be sure the GPU is no longer using them. It moves
		/// old particles from the retired area of the queue to the free section.
		/// </summary>
		void FreeRetiredParticles ()
		{
			while (firstRetiredParticle != firstActiveParticle) {
				// Has this particle been unused long enough that
				// the GPU is sure to be finished with it?
				// We multiply the retired particle index by four, because each
				// particle consists of a quad that is made up of four vertices.
				int age = drawCounter - (int)particles [firstRetiredParticle * 4].Time;

				// The GPU is never supposed to get more than 2 frames behind the CPU.
				// We add 1 to that, just to be safe in case of buggy drivers that
				// might bend the rules and let the GPU get further behind.
				if (age < 3)
					break;

				// Move the particle from the retired to the free queue.
				firstRetiredParticle++;

				if (firstRetiredParticle >= settings.MaxParticles)
					firstRetiredParticle = 0;
			}
		}


		/// <summary>
		/// Draws the particle system.
		/// </summary>
		public override void Draw (GameTime gameTime)
		{
			GraphicsDevice device = GraphicsDevice;

			// Restore the vertex buffer contents if the graphics device was lost.
			if (vertexBuffer.IsContentLost) {
				vertexBuffer.SetData (particles);
			}

			// If there are any particles waiting in the newly added queue,
			// we'd better upload them to the GPU ready for drawing.
			if (firstNewParticle != firstFreeParticle) {
				AddNewParticlesToVertexBuffer ();
			}

			// If there are any active particles, draw them now!
			if (firstActiveParticle != firstFreeParticle) {
				device.BlendState = settings.BlendState;
				device.DepthStencilState = DepthStencilState.DepthRead;

				// Set an effect parameter describing the viewport size. This is
				// needed to convert particle sizes into screen space point sizes.
				effectViewportScaleParameter.SetValue (new Vector2 (0.5f / device.Viewport.AspectRatio, -0.5f));

				// Set an effect parameter describing the current time. All the vertex
				// shader particle animation is keyed off this value.
				effectTimeParameter.SetValue (currentTime);

				// Set the particle vertex and index buffer.
				device.SetVertexBuffer (vertexBuffer);
				device.Indices = indexBuffer;

				// Activate the particle effect.
				foreach (EffectPass pass in particleEffect.CurrentTechnique.Passes) {
					pass.Apply ();

					if (firstActiveParticle < firstFreeParticle) {
						// If the active particles are all in one consecutive range,
						// we can draw them all in a single call.
						device.DrawIndexedPrimitives (PrimitiveType.TriangleList, 0, 
						firstActiveParticle * 4, (firstFreeParticle - firstActiveParticle) * 4, 
						firstActiveParticle * 6, (firstFreeParticle - firstActiveParticle) * 2);
					} else {
						// If the active particle range wraps past the end of the queue
						// back to the start, we must split them over two draw calls.
						device.DrawIndexedPrimitives (PrimitiveType.TriangleList, 0, 
						firstActiveParticle * 4, (settings.MaxParticles - firstActiveParticle) * 4, 
						firstActiveParticle * 6, (settings.MaxParticles - firstActiveParticle) * 2);

						if (firstFreeParticle > 0) {
							device.DrawIndexedPrimitives (PrimitiveType.TriangleList, 0, 
							0, firstFreeParticle * 4, 
							0, firstFreeParticle * 2);
						}
					}
				}

				// Reset some of the renderstates that we changed,
				// so as not to mess up any other subsequent drawing.
				device.DepthStencilState = DepthStencilState.Default;
			}

			drawCounter++;
		}


		/// <summary>
		/// Helper for uploading new particles from our managed
		/// array to the GPU vertex buffer.
		/// </summary>
		void AddNewParticlesToVertexBuffer ()
		{
			int stride = ParticleVertex.SizeInBytes;

			if (firstNewParticle < firstFreeParticle) {
				// If the new particles are all in one consecutive range,
				// we can upload them all in a single call.
				//				vertexBuffer.SetData (firstNewParticle * stride * 4, particles, 
				//					firstNewParticle * 4, 
				//					(firstFreeParticle - firstNewParticle) * 4, 
				//					stride, SetDataOptions.NoOverwrite);			} else {
				// If the new particle range wraps past the end of the queue
				// back to the start, we must split them over two upload calls.
				//				vertexBuffer.SetData (firstNewParticle * stride * 4, particles, 
				//					firstNewParticle * 4, 
				//					(settings.MaxParticles - firstNewParticle) * 4, 
				//					stride, SetDataOptions.NoOverwrite);

				if (firstFreeParticle > 0) {
					//					vertexBuffer.SetData (0, particles, 
					//					0, firstFreeParticle * 4, 
					//					stride, SetDataOptions.NoOverwrite);
				}
			}

			// Move the particles we just uploaded from the new to the active queue.
			firstNewParticle = firstFreeParticle;
		}


	#endregion

	#region Public Methods


		/// <summary>
		/// Sets the camera view and projection matrices
		/// that will be used to draw this particle system.
		/// </summary>
		public void SetCamera (Matrix view, Matrix projection)
		{
			effectViewParameter.SetValue (view);
			effectProjectionParameter.SetValue (projection);
		}


		/// <summary>
		/// Adds a new particle to the system.
		/// </summary>
		public void AddParticle (Vector3 position, Vector3 velocity)
		{
			// Figure out where in the circular queue to allocate the new particle.
			int nextFreeParticle = firstFreeParticle + 1;

			if (nextFreeParticle >= settings.MaxParticles)
				nextFreeParticle = 0;

			// If there are no free particles, we just have to give up.
			if (nextFreeParticle == firstRetiredParticle)
				return;

			// Adjust the input velocity based on how much
			// this particle system wants to be affected by it.
			velocity *= settings.EmitterVelocitySensitivity;

			// Add in some random amount of horizontal velocity.
			float horizontalVelocity = MathHelper.Lerp (settings.MinHorizontalVelocity, 
							settings.MaxHorizontalVelocity, 
							(float)random.NextDouble ());

			double horizontalAngle = random.NextDouble () * MathHelper.TwoPi;

			velocity.X += horizontalVelocity * (float)Math.Cos (horizontalAngle);
			velocity.Z += horizontalVelocity * (float)Math.Sin (horizontalAngle);

			// Add in some random amount of vertical velocity.
			velocity.Y += MathHelper.Lerp (settings.MinVerticalVelocity, 
					settings.MaxVerticalVelocity, 
					(float)random.NextDouble ());

			// Choose four random control values. These will be used by the vertex
			// shader to give each particle a different size, rotation, and color.
			Color randomValues = new Color ((byte)random.Next (255),
					(byte)random.Next (255),
					(byte)random.Next (255),
					(byte)random.Next (255));

			// Fill in the particle vertex structure.
			for (int i = 0; i < 4; i++) {
				particles [firstFreeParticle * 4 + i].Position = position;
				particles [firstFreeParticle * 4 + i].Velocity = velocity;
				particles [firstFreeParticle * 4 + i].Random = randomValues;
				particles [firstFreeParticle * 4 + i].Time = currentTime;
			}

			firstFreeParticle = nextFreeParticle;
		}


	#endregion
	}
}
