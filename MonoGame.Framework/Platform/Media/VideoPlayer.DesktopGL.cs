#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2019 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public sealed partial class VideoPlayer : IDisposable
	{
		#region Hardware-accelerated YUV -> RGBA

		private Effect shaderProgram;
		private IntPtr stateChangesPtr;
		private Texture2D[] yuvTextures = new Texture2D[3];
		private Viewport viewport;

		private static VertexPositionTexture[] vertices = new VertexPositionTexture[]
		{
			new VertexPositionTexture(
				new Vector3(-1.0f, -1.0f, 0.0f),
				new Vector2(0.0f, 1.0f)
			),
			new VertexPositionTexture(
				new Vector3(1.0f, -1.0f, 0.0f),
				new Vector2(1.0f, 1.0f)
			),
			new VertexPositionTexture(
				new Vector3(-1.0f, 1.0f, 0.0f),
				new Vector2(0.0f, 0.0f)
			),
			new VertexPositionTexture(
				new Vector3(1.0f, 1.0f, 0.0f),
				new Vector2(1.0f, 0.0f)
			)
		};
		private VertexBufferBinding vertBuffer;

		// Used to restore our previous GL state.
		private Texture[] oldTextures= new Texture[3];
		private SamplerState[] oldSamplers = new SamplerState[3];
		private RenderTargetBinding[] oldTargets;
		private VertexBufferBinding[] oldBuffers;
		private BlendState prevBlend;
		private DepthStencilState prevDepthStencil;
		private RasterizerState prevRasterizer;
		private Viewport prevViewport;

		private void GL_initialize()
		{
			// Load the YUV->RGBA Effect
			shaderProgram = new Effect(
				currentDevice,
				Resources.YUVToRGBAEffect
			);
			unsafe
			{
				stateChangesPtr = Marshal.AllocHGlobal(
					sizeof(MojoShader.MOJOSHADER_effectStateChanges)
				);
			}

			// Allocate the vertex buffer
			vertBuffer = new VertexBufferBinding(
				new VertexBuffer(
					currentDevice,
					VertexPositionTexture.VertexDeclaration,
					4,
					BufferUsage.WriteOnly
				)
			);
			vertBuffer.VertexBuffer.SetData(vertices);
		}

		private void GL_dispose()
		{
			if (currentDevice == null)
			{
				// We never initialized to begin with...
				return;
			}
			currentDevice = null;

			// Delete the Effect
			if (shaderProgram != null)
			{
				shaderProgram.Dispose();
			}
			if (stateChangesPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(stateChangesPtr);
			}

			// Delete the vertex buffer
			if (vertBuffer.VertexBuffer != null)
			{
				vertBuffer.VertexBuffer.Dispose();
			}

			// Delete the textures if they exist
			for (int i = 0; i < 3; i += 1)
			{
				if (yuvTextures[i] != null)
				{
					yuvTextures[i].Dispose();
				}
			}
		}

		private void GL_setupTextures(int width, int height)
		{
			// Allocate YUV GL textures
			for (int i = 0; i < 3; i += 1)
			{
				if (yuvTextures[i] != null)
				{
					yuvTextures[i].Dispose();
				}
			}
			yuvTextures[0] = new Texture2D(
				currentDevice,
				width,
				height,
				false,
				SurfaceFormat.Alpha8
			);
			yuvTextures[1] = new Texture2D(
				currentDevice,
				width / 2,
				height / 2,
				false,
				SurfaceFormat.Alpha8
			);
			yuvTextures[2] = new Texture2D(
				currentDevice,
				width / 2,
				height / 2,
				false,
				SurfaceFormat.Alpha8
			);

			// Precalculate the viewport
			viewport = new Viewport(0, 0, width, height);
		}

		private void GL_pushState()
		{
			// Begin the effect, flagging to restore previous state on end
			currentDevice.GLDevice.BeginPassRestore(
				shaderProgram.glEffect,
				stateChangesPtr
			);

			// Prep our samplers
			for (int i = 0; i < 3; i += 1)
			{
				oldTextures[i] = currentDevice.Textures[i];
				oldSamplers[i] = currentDevice.SamplerStates[i];
				currentDevice.Textures[i] = yuvTextures[i];
				currentDevice.SamplerStates[i] = SamplerState.LinearClamp;
			}

			// Prep buffers
			oldBuffers = currentDevice.GetVertexBuffers();
			currentDevice.SetVertexBuffers(vertBuffer);

			// Prep target bindings
			oldTargets = currentDevice.GetRenderTargets();
			currentDevice.GLDevice.SetRenderTargets(
				videoTexture,
				null,
				DepthFormat.None
			);

			// Prep render state
			prevBlend = currentDevice.BlendState;
			prevDepthStencil = currentDevice.DepthStencilState;
			prevRasterizer = currentDevice.RasterizerState;
			currentDevice.BlendState = BlendState.Opaque;
			currentDevice.DepthStencilState = DepthStencilState.None;
			currentDevice.RasterizerState = RasterizerState.CullNone;

			// Prep viewport
			prevViewport = currentDevice.Viewport;
			currentDevice.GLDevice.SetViewport(viewport);
		}

		private void GL_popState()
		{
			// End the effect, restoring the previous shader state
			currentDevice.GLDevice.EndPassRestore(shaderProgram.glEffect);

			// Restore GL state
			currentDevice.BlendState = prevBlend;
			currentDevice.DepthStencilState = prevDepthStencil;
			currentDevice.RasterizerState = prevRasterizer;
			prevBlend = null;
			prevDepthStencil = null;
			prevRasterizer = null;

			/* Restore targets using GLDevice directly.
			 * This prevents accidental clearing of previously bound targets.
			 */
			if (oldTargets == null || oldTargets.Length == 0)
			{
				currentDevice.GLDevice.SetRenderTargets(
					null,
					null,
					DepthFormat.None
				);
			}
			else
			{
				IRenderTarget oldTarget = oldTargets[0].RenderTarget as IRenderTarget;
				currentDevice.GLDevice.SetRenderTargets(
					oldTargets,
					oldTarget.DepthStencilBuffer,
					oldTarget.DepthStencilFormat
				);
			}
			oldTargets = null;

			// Set viewport AFTER setting targets!
			currentDevice.GLDevice.SetViewport(prevViewport);

			// Restore buffers
			currentDevice.SetVertexBuffers(oldBuffers);
			oldBuffers = null;

			// Restore samplers
			for (int i = 0; i < 3; i += 1)
			{
				currentDevice.Textures[i] = oldTextures[i];
				currentDevice.SamplerStates[i] = oldSamplers[i];
				oldTextures[i] = null;
				oldSamplers[i] = null;
			}
		}

		#endregion

		#region Public Member Data: XNA VideoPlayer Implementation

		public bool IsDisposed
		{
			get;
			private set;
		}

		public bool IsLooped
		{
			get;
			set;
		}

		private bool backing_ismuted;
		public bool IsMuted
		{
			get
			{
				return backing_ismuted;
			}
			set
			{
				backing_ismuted = value;
				UpdateVolume();
			}
		}

		public TimeSpan PlayPosition
		{
			get
			{
				return timer.Elapsed;
			}
		}

		public MediaState State
		{
			get;
			private set;
		}

		public Video Video
		{
			get;
			private set;
		}

		private float backing_volume;
		public float Volume
		{
			get
			{
				return backing_volume;
			}
			set
			{
				if (value > 1.0f)
				{
					backing_volume = 1.0f;
				}
				else if (value < 0.0f)
				{
					backing_volume = 0.0f;
				}
				else
				{
					backing_volume = value;
				}
				UpdateVolume();
			}
		}

		#endregion

		#region Private Member Data: XNA VideoPlayer Implementation

		// We use this to update our PlayPosition.
		private Stopwatch timer;

		// Store this to optimize things on our end.
		private RenderTargetBinding[] videoTexture;

		// We need to access the GraphicsDevice frequently.
		private GraphicsDevice currentDevice;

		#endregion

		#region Private Member Data: Theorafile

		private IntPtr yuvData;
		private int currentFrame;

		private const int AUDIO_BUFFER_SIZE = 4096 * 2;
		private static readonly float[] audioData = new float[AUDIO_BUFFER_SIZE];
		private static GCHandle audioHandle = GCHandle.Alloc(audioData, GCHandleType.Pinned);
		private IntPtr audioDataPtr = audioHandle.AddrOfPinnedObject();

		#endregion

		#region Private Member Data: Audio Stream

		private DynamicSoundEffectInstance audioStream;

		#endregion

		#region Private Methods: XNA VideoPlayer Implementation

		private void checkDisposed()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("VideoPlayer");
			}
		}

		#endregion

		#region Private Methods: Audio Stream

		private void UpdateVolume()
		{
			if (audioStream == null)
			{
				return;
			}
			if (IsMuted)
			{
				audioStream.Volume = 0.0f;
			}
			else
			{
				/* FIXME: Works around MasterVolume only temporarily!
				 * We need to detach this source from the AL listener properties.
				 * -flibit
				 */
				audioStream.Volume = Volume * (1.0f / SoundEffect.MasterVolume);
			}
		}

		#endregion

		#region Public Methods: XNA VideoPlayer Implementation

		public VideoPlayer()
		{
			// Initialize public members.
			IsDisposed = false;
			IsLooped = false;
			IsMuted = false;
			State = MediaState.Stopped;
			Volume = 1.0f;

			// Initialize private members.
			timer = new Stopwatch();
			videoTexture = new RenderTargetBinding[1];
		}

		public void Dispose()
		{
			if (IsDisposed)
			{
				return;
			}

			// Stop the VideoPlayer. This gets almost everything...
			Stop();

			// Destroy the other GL bits.
			GL_dispose();

			// Dispose the DynamicSoundEffectInstance
			if (audioStream != null)
			{
				audioStream.Dispose();
				audioStream = null;
			}

			// Dispose the Texture.
			if (videoTexture[0].RenderTarget != null)
			{
				videoTexture[0].RenderTarget.Dispose();
			}

			// Free the YUV buffer
			if (yuvData != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(yuvData);
				yuvData = IntPtr.Zero;
			}

			// Okay, we out.
			IsDisposed = true;
		}

		public Texture2D GetTexture()
		{
			checkDisposed();

			if (Video == null)
			{
				throw new InvalidOperationException();
			}

			// Be sure we can even get something from Theorafile...
			if (	State == MediaState.Stopped ||
				Video.theora == IntPtr.Zero ||
				Theorafile.tf_hasvideo(Video.theora) == 0	)
			{
				 // Screw it, give them the old one.
				return videoTexture[0].RenderTarget as Texture2D;
			}

			int thisFrame = (int) (timer.Elapsed.TotalMilliseconds / (1000.0 / Video.FramesPerSecond));
			if (thisFrame > currentFrame)
			{
				// Only update the textures if we need to!
				if (Theorafile.tf_readvideo(
					Video.theora,
					yuvData,
					thisFrame - currentFrame
				) == 1 || currentFrame == -1) {
					UpdateTexture();
				}
				currentFrame = thisFrame;
			}

			// Check for the end...
			bool ended = Theorafile.tf_eos(Video.theora) == 1;
			if (audioStream != null)
			{
				ended &= audioStream.PendingBufferCount == 0;
			}
			if (ended)
			{
				// FIXME: This is part of the Duration hack!
				if (Video.needsDurationHack)
				{
					Video.Duration = timer.Elapsed; // FIXME: Frames * FPS? -flibit
				}

				// Stop and reset the timer. If we're looping, the loop will start it again.
				timer.Stop();
				timer.Reset();

				// Kill whatever audio/video we've got
				if (audioStream != null)
				{
					audioStream.Stop();
					audioStream.Dispose();
					audioStream = null;
				}

				// Reset the stream no matter what happens next
				Theorafile.tf_reset(Video.theora);

				// If looping, go back to the start. Otherwise, we'll be exiting.
				if (IsLooped)
				{
					// Starting over!
					InitializeTheoraStream();

					// Start! Again!
					timer.Start();
					if (audioStream != null)
					{
						audioStream.Play();
					}
				}
				else
				{
					// We out
					State = MediaState.Stopped;
				}
			}

			// Finally.
			return videoTexture[0].RenderTarget as Texture2D;
		}

		public void Play(Video video)
		{
			checkDisposed();

			// We need to assign this regardless of what happens next.
			Video = video;

			// FIXME: This is a part of the Duration hack!
			if (Video.needsDurationHack)
			{
				Video.Duration = TimeSpan.MaxValue;
			}

			// Check the player state before attempting anything.
			if (State != MediaState.Stopped)
			{
				return;
			}

			// Update the player state now, before initializing
			State = MediaState.Playing;

			// Carve out YUV buffer before doing any decoder work
			if (yuvData != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(yuvData);
			}
			yuvData = Marshal.AllocHGlobal(Video.Width * Video.Height * 2);

			// Hook up the decoder to this player
			InitializeTheoraStream();

			// Set up the texture data
			if (Theorafile.tf_hasvideo(Video.theora) == 1)
			{
				// The VideoPlayer will use the GraphicsDevice that is set now.
				if (currentDevice != Video.GraphicsDevice)
				{
					GL_dispose();
					currentDevice = Video.GraphicsDevice;
					GL_initialize();
				}

				RenderTargetBinding overlap = videoTexture[0];
				videoTexture[0] = new RenderTargetBinding(
					new RenderTarget2D(
						currentDevice,
						Video.Width,
						Video.Height,
						false,
						SurfaceFormat.Color,
						DepthFormat.None,
						0,
						RenderTargetUsage.PreserveContents
					)
				);
				if (overlap.RenderTarget != null)
				{
					overlap.RenderTarget.Dispose();
				}
				GL_setupTextures(
					Video.Width,
					Video.Height
				);
			}

			// The player can finally start now!
			timer.Start();
			if (audioStream != null)
			{
				audioStream.Play();
			}
		}

		public void Stop()
		{
			checkDisposed();

			// Check the player state before attempting anything.
			if (State == MediaState.Stopped)
			{
				return;
			}

			// Update the player state.
			State = MediaState.Stopped;

			// Wait for the player to end if it's still going.
			timer.Stop();
			timer.Reset();
			if (audioStream != null)
			{
				audioStream.Stop();
				audioStream.Dispose();
				audioStream = null;
			}
			Theorafile.tf_reset(Video.theora);
		}

		public void Pause()
		{
			checkDisposed();

			// Check the player state before attempting anything.
			if (State != MediaState.Playing)
			{
				return;
			}

			// Update the player state.
			State = MediaState.Paused;

			// Pause timer, audio.
			timer.Stop();
			if (audioStream != null)
			{
				audioStream.Pause();
			}
		}

		public void Resume()
		{
			checkDisposed();

			// Check the player state before attempting anything.
			if (State != MediaState.Paused)
			{
				return;
			}

			// Update the player state.
			State = MediaState.Playing;

			// Unpause timer, audio.
			timer.Start();
			if (audioStream != null)
			{
				audioStream.Resume();
			}
		}

		#endregion

		#region Private Theora Audio Stream Methods

		private void OnBufferRequest(object sender, EventArgs args)
		{
			int samples = Theorafile.tf_readaudio(
				Video.theora,
				audioDataPtr,
				AUDIO_BUFFER_SIZE
			);
			if (samples > 0)
			{
				audioStream.SubmitFloatBufferEXT(
					audioData,
					0,
					samples
				);
			}
			else if (Theorafile.tf_eos(Video.theora) == 1)
			{
				// Okay, we ran out. No need for this!
				audioStream.BufferNeeded -= OnBufferRequest;
			}
		}

		#endregion

		#region Private Theora Video Stream Methods

		private void UpdateTexture()
		{
			// Prepare YUV GL textures with our current frame data
			currentDevice.GLDevice.SetTextureData2DPointer(
				yuvTextures[0],
				yuvData
			);
			currentDevice.GLDevice.SetTextureData2DPointer(
				yuvTextures[1],
				new IntPtr(
					yuvData.ToInt64() +
					(Video.Width * Video.Height)
				)
			);
			currentDevice.GLDevice.SetTextureData2DPointer(
				yuvTextures[2],
				new IntPtr(
					yuvData.ToInt64() +
					(Video.Width * Video.Height) +
					(Video.Width / 2 * Video.Height / 2)
				)
			);

			// Draw the YUV textures to the framebuffer with our shader.
			GL_pushState();
			currentDevice.DrawPrimitives(
				PrimitiveType.TriangleStrip,
				0,
				2
			);
			GL_popState();
		}

		#endregion

		#region Theora Decoder Hookup Method

		private void InitializeTheoraStream()
		{
			// Grab the first video frame ASAP.
			while (Theorafile.tf_readvideo(Video.theora, yuvData, 1) == 0);

			// Grab the first bit of audio. We're trying to start the decoding ASAP.
			if (Theorafile.tf_hasaudio(Video.theora) == 1)
			{
				int channels, samplerate;
				Theorafile.tf_audioinfo(Video.theora, out channels, out samplerate);
				audioStream = new DynamicSoundEffectInstance(
					samplerate,
					(AudioChannels) channels
				);
				audioStream.BufferNeeded += OnBufferRequest;
				UpdateVolume();

				// Fill and queue the buffers.
				for (int i = 0; i < 4; i += 1)
				{
					OnBufferRequest(audioStream, EventArgs.Empty);
					if (audioStream.PendingBufferCount == i)
					{
						break;
					}
				}
			}

			currentFrame = -1;
		}

		#endregion
	}
}
