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
		private Shader oldVertexShader;
		private Shader oldPixelShader;
		private Texture[] oldTextures= new Texture[3];
		private SamplerState[] oldSamplers = new SamplerState[3];
		private RenderTargetBinding[] oldTargets;
		private int oldBufferCount;
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
				EffectResource.YUVToRGBAEffect.Bytecode
			);

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
			oldVertexShader = currentDevice.VertexShader;
			oldPixelShader = currentDevice.PixelShader;

			// Prep our samplers
			for (int i = 0; i < 3; i += 1)
			{
				oldTextures[i] = currentDevice.Textures[i];
				oldSamplers[i] = currentDevice.SamplerStates[i];
				currentDevice.Textures[i] = yuvTextures[i];
				currentDevice.SamplerStates[i] = SamplerState.LinearClamp;
			}

            shaderProgram.CurrentTechnique.Passes[0].Apply();

			// Prep buffers
			oldBufferCount = currentDevice.VertexBuffers.Count;
			if (oldBuffers == null || oldBuffers.Length < oldBufferCount)
				oldBuffers = new VertexBufferBinding[oldBufferCount];

			for (var i = 0; i < oldBufferCount; i++)
				oldBuffers[i] = currentDevice.VertexBuffers.Get(i);

			currentDevice.SetVertexBuffers(vertBuffer);

			// Prep target bindings
			oldTargets = currentDevice.GetRenderTargets();
			currentDevice.SetRenderTargets(videoTexture);

			// Prep render state
			prevBlend = currentDevice.BlendState;
			prevDepthStencil = currentDevice.DepthStencilState;
			prevRasterizer = currentDevice.RasterizerState;
			currentDevice.BlendState = BlendState.Opaque;
			currentDevice.DepthStencilState = DepthStencilState.None;
			currentDevice.RasterizerState = RasterizerState.CullNone;

			// Prep viewport
			prevViewport = currentDevice.Viewport;
			currentDevice.Viewport = viewport;
		}

		private void GL_popState()
		{
			currentDevice.VertexShader = oldVertexShader;
			currentDevice.PixelShader = oldPixelShader;

			// Restore GL state
			currentDevice.BlendState = prevBlend;
			currentDevice.DepthStencilState = prevDepthStencil;
			currentDevice.RasterizerState = prevRasterizer;
			prevBlend = null;
			prevDepthStencil = null;
			prevRasterizer = null;

			if (oldTargets == null || oldTargets.Length == 0)
			{
				currentDevice.SetRenderTarget(null);
			}
			else
			{
				// TODO make sure this does not clear previously bound targets
				IRenderTarget oldTarget = oldTargets[0].RenderTarget as IRenderTarget;
				currentDevice.SetRenderTargets(oldTargets);
			}
			oldTargets = null;

			// Set viewport AFTER setting targets!
			currentDevice.Viewport = prevViewport;

			// Restore buffers
			currentDevice.VertexBuffers.Set(oldBuffers, oldBufferCount);
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

		#region Platform specific methods

		private void PlatformInitialize()
		{
			timer = new Stopwatch();
			videoTexture = new RenderTargetBinding[1];
		}

		private void PlatformSetIsLooped()
		{
		}

		private void PlatformSetIsMuted()
		{
			UpdateVolume();
		}

		private TimeSpan PlatformGetPlayPosition()
		{
			return timer.Elapsed;
		}

		private void PlatformSetVolume()
		{
			UpdateVolume();
		}

		private void PlatformGetState(ref MediaState state)
		{
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

		private void PlatformDispose(bool disposing)
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

		private Texture2D PlatformGetTexture()
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

		private void PlatformPlay()
		{
			checkDisposed();

			// We need to assign this regardless of what happens next.

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

		private void PlatformStop()
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

		private void PlatformPause()
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

		private void PlatformResume()
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
				audioStream.SubmitFloatBuffer(audioData, 0, samples);
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
			yuvTextures[0].SetDataFromPointer(yuvData);

			var yuvData1 = new IntPtr(yuvData.ToInt64() + (Video.Width * Video.Height));
			yuvTextures[1].SetDataFromPointer(yuvData1);

			var yuvData2 = new IntPtr(yuvData.ToInt64() + (Video.Width * Video.Height) + (Video.Width / 2 * Video.Height / 2));
			yuvTextures[2].SetDataFromPointer(yuvData2);

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
