#region License
/* TheoraPlay VideoPlayer for MonoGame
 *
 * Copyright (c) 2013 Ethan Lee.
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 * claim that you wrote the original software. If you use this software in a
 * product, an acknowledgment in the product documentation would be
 * appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not be
 * misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source distribution.
 *
 * Ethan "flibitijibibo" Lee <flibitijibibo@flibitijibibo.com>
 *
 */
#endregion

#region VideoPlayer Graphics Define
#if SDL2
#define VIDEOPLAYER_OPENGL
#endif
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using Microsoft.Xna.Framework.Graphics;

using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL;
#endregion

namespace Microsoft.Xna.Framework.Media
{
    public sealed class VideoPlayer : IDisposable
    {
        #region Hardware-accelerated YUV -> RGBA
#if VIDEOPLAYER_OPENGL
        private const string shader_vertex =
            "#version 110\n" +
            "attribute vec2 pos;\n" +
            "attribute vec2 tex;\n" +
            "void main() {\n" +
            "   gl_Position = vec4(pos.xy, 0.0, 1.0);\n" +
            "   gl_TexCoord[0].xy = tex;\n" +
            "}\n";
        private const string shader_fragment =
            "#version 110\n" +
            "uniform sampler2D samp0;\n" +
            "uniform sampler2D samp1;\n" +
            "uniform sampler2D samp2;\n" +
            "const vec3 offset = vec3(-0.0625, -0.5, -0.5);\n" +
            "const vec3 Rcoeff = vec3(1.164,  0.000,  1.596);\n" +
            "const vec3 Gcoeff = vec3(1.164, -0.391, -0.813);\n" +
            "const vec3 Bcoeff = vec3(1.164,  2.018,  0.000);\n" +
            "void main() {\n" +
            "   vec2 tcoord;\n" +
            "   vec3 yuv, rgb;\n" +
            "   tcoord = gl_TexCoord[0].xy;\n" +
            "   yuv.x = texture2D(samp0, tcoord).r;\n" +
            "   yuv.y = texture2D(samp1, tcoord).r;\n" +
            "   yuv.z = texture2D(samp2, tcoord).r;\n" +
            "   yuv += offset;\n" +
            "   rgb.r = dot(yuv, Rcoeff);\n" +
            "   rgb.g = dot(yuv, Gcoeff);\n" +
            "   rgb.b = dot(yuv, Bcoeff);\n" +
            "   gl_FragColor = vec4(rgb, 1.0);\n" +
            "}\n";
        
        private int shaderProgram;
        private int[] yuvTextures;
        private int rgbaFramebuffer;
        
        private float[] vert_pos;
        private float[] vert_tex;
        
        // Used to restore our previous GL state.
        private int[] oldTextures;
        private int oldShader;
        private int oldFramebuffer;
        private int oldActiveTexture;
        private int[] oldViewport;
        private bool oldCullState;
        private bool oldDepthMask;
        private bool oldDepthTest;
        private bool oldAlphaTest;
        private bool oldBlendState;
        
        private void GL_initialize()
        {
            // Initialize the old viewport array.
            oldViewport = new int[4];
            
            // Initialize the texture storage array.
            oldTextures = new int[3];
            
            // Create the YUV textures.
            yuvTextures = new int[3];
            GL.GenTextures(3, yuvTextures);
            
            // Create the RGBA framebuffer target.
            GL.GenFramebuffers(1, out rgbaFramebuffer);
            
            // Create our pile of vertices.
            vert_pos = new float[2 * 4]; // 2 dimensions * 4 vertices
            vert_tex = new float[2 * 4];
                    vert_pos[0] = -1.0f;
                    vert_pos[1] =  1.0f;
                    vert_tex[0] =  0.0f;
                    vert_tex[1] =  1.0f;
                    vert_pos[2] =  1.0f;
                    vert_pos[3] =  1.0f;
                    vert_tex[2] =  1.0f;
                    vert_tex[3] =  1.0f;
                    vert_pos[4] = -1.0f;
                    vert_pos[5] = -1.0f;
                    vert_tex[4] =  0.0f;
                    vert_tex[5] =  0.0f;
                    vert_pos[6] =  1.0f;
                    vert_pos[7] = -1.0f;
                    vert_tex[6] =  1.0f;
                    vert_tex[7] =  0.0f;
            
            // Create the vertex/fragment shaders.
            int vshader_id = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vshader_id, shader_vertex);
            GL.CompileShader(vshader_id);
            int fshader_id = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fshader_id, shader_fragment);
            GL.CompileShader(fshader_id);
            
            // Create the shader program.
            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vshader_id);
            GL.AttachShader(shaderProgram, fshader_id);
            GL.BindAttribLocation(shaderProgram, 0, "pos");
            GL.BindAttribLocation(shaderProgram, 1, "tex");
            GL.LinkProgram(shaderProgram);
            GL.DeleteShader(vshader_id);
            GL.DeleteShader(fshader_id);
        }
        
        private void GL_dispose()
        {
            // Delete the shader program.
            GL.DeleteProgram(shaderProgram);
            
            // Delete the RGBA framebuffer target.
            GL.DeleteFramebuffers(1, ref rgbaFramebuffer);
            
            // Delete the YUV textures.
            GL.DeleteTextures(3, yuvTextures);
        }
        
        private void GL_internal_genTexture(    int texID,
                                                int width,
                                                int height,
                                                PixelInternalFormat internalFormat,
                                                PixelFormat format,
                                                PixelType type  )
        {
            // Bind the desired texture.
            GL.BindTexture(TextureTarget.Texture2D, texID);
            
            // Set the texture parameters, for completion/consistency's sake.
            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureWrapS,
                (int) TextureWrapMode.ClampToEdge
            );
            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureWrapT,
                (int) TextureWrapMode.ClampToEdge
            );
            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter,
                (int) TextureMinFilter.Linear
            );
            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Linear
            );
            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureBaseLevel,
                0
            );
            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMaxLevel,
                0
            );
            
            // Allocate the texture data.
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                internalFormat,
                width,
                height,
                0,
                format,
                type,
                IntPtr.Zero
            );
        }
        
        private void GL_setupTargets(int width, int height)
        {
            // We're going to be messing with things to do this...
            GL_pushState();
            
            // We'll just use this for all the texture work.
            GL.ActiveTexture(TextureUnit.Texture0);
            
            // Attach the Texture2D to the framebuffer.
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, rgbaFramebuffer);
            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D,
                videoTexture.glTexture,
                0
            );
            
            // Allocate YUV GL textures
            GL_internal_genTexture(
                yuvTextures[0],
                width,
                height,
                PixelInternalFormat.Luminance,
                PixelFormat.Luminance,
                PixelType.UnsignedByte
            );
            GL_internal_genTexture(
                yuvTextures[1],
                width / 2,
                height / 2,
                PixelInternalFormat.Luminance,
                PixelFormat.Luminance,
                PixelType.UnsignedByte
            );
            GL_internal_genTexture(
                yuvTextures[2],
                width / 2,
                height / 2,
                PixelInternalFormat.Luminance,
                PixelFormat.Luminance,
                PixelType.UnsignedByte
            );
            
            // Aaand we should be set now.
            GL_popState();
        }
        
        private void GL_pushState()
        {
            GL.GetInteger(GetPName.Viewport, oldViewport);
            GL.GetInteger(GetPName.CurrentProgram, out oldShader);
            GL.GetInteger(GetPName.ActiveTexture, out oldActiveTexture);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.GetInteger(GetPName.TextureBinding2D, out oldTextures[0]);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.GetInteger(GetPName.TextureBinding2D, out oldTextures[1]);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.GetInteger(GetPName.TextureBinding2D, out oldTextures[2]);
            GL.GetInteger(GetPName.FramebufferBinding, out oldFramebuffer);
            oldCullState = GL.IsEnabled(EnableCap.CullFace);
            GL.Disable(EnableCap.CullFace);
            GL.GetBoolean(GetPName.DepthWritemask, out oldDepthMask);
            GL.DepthMask(false);
            oldDepthTest = GL.IsEnabled(EnableCap.DepthTest);
            GL.Disable(EnableCap.DepthTest);
            oldAlphaTest = GL.IsEnabled(EnableCap.AlphaTest);
            GL.Disable(EnableCap.AlphaTest);
            oldBlendState = GL.IsEnabled(EnableCap.Blend);
            GL.Disable(EnableCap.Blend);
        }
        
        private void GL_popState()
        {
            GL.Viewport(oldViewport[0], oldViewport[1], oldViewport[2], oldViewport[3]);
            GL.UseProgram(oldShader);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, oldTextures[0]);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, oldTextures[1]);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, oldTextures[2]);
            GL.ActiveTexture((TextureUnit) oldActiveTexture);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, oldFramebuffer);
            if (oldCullState)
            {
                GL.Enable(EnableCap.CullFace);
            }
            GL.DepthMask(oldDepthMask);
            if (oldDepthTest)
            {
                GL.Enable(EnableCap.DepthTest);
            }
            if (oldAlphaTest)
            {
                GL.Enable(EnableCap.AlphaTest);
            }
            if (oldBlendState)
            {
                GL.Enable(EnableCap.Blend);
            }
        }
#endif
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
        
        // Thread containing our video player.
        private Thread playerThread;
        
        // Store this to optimize things on our end.
        private Texture2D videoTexture;
        
        // Used to sync the A/V output on thread start.
        private bool audioStarted;
        #endregion
        
        #region Private Member Data: TheoraPlay
        // Grabbed from the Video streams.
        private TheoraPlay.THEORAPLAY_VideoFrame currentVideo;
        private TheoraPlay.THEORAPLAY_VideoFrame nextVideo;
        private TheoraPlay.THEORAPLAY_AudioPacket currentAudio;
        private IntPtr previousFrame;
        
        // Audio's done separately from the player thread.
        private Thread audioDecoderThread;
        
        // Used to prevent a frame from getting lost before we get the texture.
        private bool frameLocked;
        #endregion
        
        #region Private Member Data: OpenAL
        private int audioSourceIndex;
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
        
        #region Private Methods: OpenAL
        private void UpdateVolume()
        {
            if (IsMuted)
            {
                AL.Source(audioSourceIndex, ALSourcef.Gain, 0.0f);
            }
            else
            {
                AL.Source(audioSourceIndex, ALSourcef.Gain, Volume);
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
            frameLocked = false;
            audioStarted = false;
            
            // Initialize this here to prevent null GetTexture returns.
            videoTexture = new Texture2D(
                Game.Instance.GraphicsDevice,
                1280,
                720
            );
            
#if VIDEOPLAYER_OPENGL
            // Initialize the OpenGL bits.
            GL_initialize();
#endif
        }
        
        public void Dispose()
        {
            // Stop the VideoPlayer. This gets almost everything...
            Stop();
            
#if VIDEOPLAYER_OPENGL
            // Destroy the OpenGL bits.
            GL_dispose();
#endif
            
            // Dispose the Texture.
            videoTexture.Dispose();
            
            // Okay, we out.
            IsDisposed = true;
        }
        
        public Texture2D GetTexture()
        {
            checkDisposed();
            
            // Be sure we can even get something from TheoraPlay...
            if (    State == MediaState.Stopped ||
                    Video.theoraDecoder == IntPtr.Zero ||
                    TheoraPlay.THEORAPLAY_isInitialized(Video.theoraDecoder) == 0   )
            {
                return videoTexture; // Screw it, give them the old one.
            }
            
            // Assign this locally, or else the thread will ruin your face.
            frameLocked = true;
            
#if VIDEOPLAYER_OPENGL
            // Set up an environment to muck about in.
            GL_pushState();
            
            // Bind our shader program.
            GL.UseProgram(shaderProgram);
            
            // Set uniform values.
            GL.Uniform1(
                GL.GetUniformLocation(shaderProgram, "samp0"),
                0
            );
            GL.Uniform1(
                GL.GetUniformLocation(shaderProgram, "samp1"),
                1
            );
            GL.Uniform1(
                GL.GetUniformLocation(shaderProgram, "samp2"),
                2
            );
            
            // Set up the vertex pointers/arrays.
            GL.VertexAttribPointer(
                0,
                2,
                VertexAttribPointerType.Float,
                false,
                2 * sizeof(float),
                vert_pos
            );
            GL.VertexAttribPointer(
                1,
                2,
                VertexAttribPointerType.Float,
                false,
                2 * sizeof(float),
                vert_tex
            );
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            
            // Bind our target framebuffer.
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, rgbaFramebuffer);
            
            // Prepare YUV GL textures with our current frame data
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, yuvTextures[0]);
            GL.TexSubImage2D(
                TextureTarget.Texture2D,
                0,
                0,
                0,
                (int) currentVideo.width,
                (int) currentVideo.height,
                PixelFormat.Luminance,
                PixelType.UnsignedByte,
                currentVideo.pixels
            );
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, yuvTextures[1]);
            GL.TexSubImage2D(
                TextureTarget.Texture2D,
                0,
                0,
                0,
                (int) currentVideo.width / 2,
                (int) currentVideo.height / 2,
                PixelFormat.Luminance,
                PixelType.UnsignedByte,
                new IntPtr(
                    currentVideo.pixels.ToInt64() +
                    (currentVideo.width * currentVideo.height)
                )
            );
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, yuvTextures[2]);
            GL.TexSubImage2D(
                TextureTarget.Texture2D,
                0,
                0,
                0,
                (int) currentVideo.width / 2,
                (int) currentVideo.height / 2,
                PixelFormat.Luminance,
                PixelType.UnsignedByte,
                new IntPtr(
                    currentVideo.pixels.ToInt64() +
                    (currentVideo.width * currentVideo.height) +
                    (currentVideo.width / 2 * currentVideo.height / 2)
                )
            );
            
            // Flip the viewport, because loldirectx
            GL.Viewport(
                0,
                0,
                (int) currentVideo.width,
                (int) currentVideo.height
            );
            
            // Draw the YUV textures to the framebuffer with our shader.
            GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);
            
            // Clean up after ourselves.
            GL_popState();
#else
            // Just copy it to an array, since it's RGBA anyway.
            try
            {
                byte[] theoraPixels = TheoraPlay.getPixels(
                    currentVideo.pixels,
                    (int) currentVideo.width * (int) currentVideo.height * 4
                );
                
                // TexImage2D.
                videoTexture.SetData<byte>(theoraPixels);
            }
            catch(Exception e)
            {
                System.Console.WriteLine(
                    "WARNING: THEORA FRAME COPY FAILED: " +
                    e.Message
                );
                frameLocked = false;
                return videoTexture; // Hope this still has something in it...
            }
#endif
   
            // Release the lock on the frame, we're done.
            frameLocked = false;
            
            return videoTexture;
        }
        
        public void Play(Video video)
        {
            checkDisposed();
            
            // We need to assign this regardless of what happens next.
            Video = video;
            
            // FIXME: This is a part of the Duration hack!
            Video.Duration = TimeSpan.MaxValue;
            
            // Check the player state before attempting anything.
            if (State != MediaState.Stopped)
            {
                return;
            }
            
            // In rare cases, the thread might still be going. Wait until it's done.
            if (playerThread != null && playerThread.IsAlive)
            {
                Stop();
            }

            // Create new Thread instances in case we use this player multiple times.
            playerThread = new Thread(new ThreadStart(this.RunVideo));
            audioDecoderThread = new Thread(new ThreadStart(this.DecodeAudio));
            
            // Update the player state now, for the thread we're about to make.
            State = MediaState.Playing;
            
            // Start the video if it hasn't been yet.
            video.Initialize();
            
            // Grab the first bit of audio. We're trying to start the decoding ASAP.
            if (TheoraPlay.THEORAPLAY_hasAudioStream(Video.theoraDecoder) != 0)
            {
                currentAudio = TheoraPlay.getAudioPacket(Video.audioStream);
                audioDecoderThread.Start();
            }
            else
            {
                audioStarted = true; // Welp.
            }
            
            // Grab the first bit of video, set up the texture.
            if (TheoraPlay.THEORAPLAY_hasVideoStream(Video.theoraDecoder) != 0)
            {
                currentVideo = TheoraPlay.getVideoFrame(Video.videoStream);
                previousFrame = Video.videoStream;
                do
                {
                    // The decoder miiight not be ready yet.
                    Video.videoStream = TheoraPlay.THEORAPLAY_getVideo(Video.theoraDecoder);
                } while (Video.videoStream == IntPtr.Zero);
                nextVideo = TheoraPlay.getVideoFrame(Video.videoStream);
                
                Texture2D overlap = videoTexture;
                videoTexture = new Texture2D(
                    Game.Instance.GraphicsDevice,
                    (int) currentVideo.width,
                    (int) currentVideo.height,
                    false,
                    SurfaceFormat.Color
                );
                overlap.Dispose();
#if VIDEOPLAYER_OPENGL
                GL_setupTargets(
                    (int) currentVideo.width,
                    (int) currentVideo.height
                );
#endif
            }
            
            // Initialize the thread!
            System.Console.Write("Starting Theora player...");
            playerThread.Start();
            System.Console.Write(" Waiting for initialization...");
            while (!playerThread.IsAlive);
            System.Console.WriteLine(" Done!");
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
            if (!playerThread.IsAlive)
            {
                return;
            }
            System.Console.Write("Signaled Theora player to stop, waiting...");
            playerThread.Join();
            System.Console.WriteLine(" Done!");
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
        }
        #endregion
        
        #region The Theora audio decoder thread
        private bool StreamAudio(int buffer)
        {
            // The size of our abstracted buffer.
            const int BUFFER_SIZE = 4096 * 2;
            
            // Store our abstracted buffer into here.
            List<float> data = new List<float>();
            
            // Be sure we have an audio stream first!
            if (Video.audioStream == IntPtr.Zero)
            {
                return false; // NOPE
            }
            
            // Add to the buffer from the decoder until it's large enough.
            while (data.Count < BUFFER_SIZE && State != MediaState.Stopped)
            {
                data.AddRange(
                    TheoraPlay.getSamples(
                        currentAudio.samples,
                        currentAudio.frames * currentAudio.channels
                    )
                );
                
                // We've copied the audio, so free this.
                TheoraPlay.THEORAPLAY_freeAudio(Video.audioStream);
                
                do
                {
                    Video.audioStream = TheoraPlay.THEORAPLAY_getAudio(Video.theoraDecoder);
                    if (State == MediaState.Stopped)
                    {
                        // Screw it, just bail out ASAP.
                        return false;
                    }
                } while (Video.audioStream == IntPtr.Zero);
                currentAudio = TheoraPlay.getAudioPacket(Video.audioStream);
                
                if ((BUFFER_SIZE - data.Count) < 4096)
                {
                    break;
                }
            }
            
            // If we actually got data, buffer it into OpenAL.
            if (data.Count > 0)
            {
                AL.BufferData(
                    buffer,
                    (currentAudio.channels == 2) ? ALFormat.StereoFloat32Ext : ALFormat.MonoFloat32Ext,
                    data.ToArray(),
                    data.Count * 2 * currentAudio.channels, // Dear OpenAL: WTF?! Love, flibit
                    currentAudio.freq
                );
                return true;
            }
            return false;
        }
        
        private void DecodeAudio()
        {
            // The number of AL buffers to queue into the source.
            const int NUM_BUFFERS = 4;
            
            // Generate the source.
            audioSourceIndex = AL.GenSource();
            
            // Generate the alternating buffers.
            int[] buffers = AL.GenBuffers(NUM_BUFFERS);
            
            // Fill and queue the buffers.
            for (int i = 0; i < NUM_BUFFERS; i++)
            {
                if (!StreamAudio(buffers[i]))
                {
                    break;
                }
            }
            AL.SourceQueueBuffers(audioSourceIndex, NUM_BUFFERS, buffers);
            
            // We now have some audio to start with. Go!
            audioStarted = true;
            
            while (State != MediaState.Stopped)
            {
                // When a buffer has been processed, refill it.
                int processed;
                AL.GetSource(audioSourceIndex, ALGetSourcei.BuffersProcessed, out processed);
                while (processed-- > 0)
                {
                    int buffer = AL.SourceUnqueueBuffer(audioSourceIndex);
                    if (!StreamAudio(buffer))
                    {
                        break;
                    }
                    AL.SourceQueueBuffer(audioSourceIndex, buffer);
                }
            }
            
            // Force stop the OpenAL source and destroy it with the buffers.
            if (AL.GetSourceState(audioSourceIndex) != ALSourceState.Stopped)
            {
                AL.SourceStop(audioSourceIndex);
            }
            AL.DeleteSource(audioSourceIndex);
            AL.DeleteBuffers(buffers);
            
            // Audio is done.
            audioStarted = false;
        }
        #endregion
        
        #region The Theora video player thread
        private void RunVideo()
        {
            // FIXME: Maybe use an actual thread synchronization technique.
            while (!audioStarted && State != MediaState.Stopped);
            
            while (State != MediaState.Stopped)
            {
                // Someone needs to look at their memory management...
                if (Game.Instance == null)
                {
                    System.Console.WriteLine("Game exited before video player! Halting...");
                    State = MediaState.Stopped;
                }
                
                // Sleep when paused, update the video state when playing.
                if (State == MediaState.Paused)
                {
                    // Pause the OpenAL source.
                    if (AL.GetSourceState(audioSourceIndex) == ALSourceState.Playing)
                    {
                        AL.SourcePause(audioSourceIndex);
                    }
                    
                    // Stop the timer in here so we know when we really stopped.
                    if (timer.IsRunning)
                    {
                        timer.Stop();
                    }
                    
                    // Arbitrarily 1 frame in a 30fps movie.
                    Thread.Sleep(33);
                }
                else
                {
                    // Start the timer, whether we're starting or unpausing.
                    if (!timer.IsRunning)
                    {
                        timer.Start();
                    }
                    
                    // If we're getting here, we should be playing the audio...
                    if (TheoraPlay.THEORAPLAY_hasAudioStream(Video.theoraDecoder) != 0)
                    {
                        if (AL.GetSourceState(audioSourceIndex) != ALSourceState.Playing)
                        {
                            AL.SourcePlay(audioSourceIndex);
                        }
                    }
                    
                    // Get the next video from from the decoder, if a stream exists.
                    if (TheoraPlay.THEORAPLAY_hasVideoStream(Video.theoraDecoder) != 0)
                    {
                        // Only step when it's time to do so.
                        if (nextVideo.playms <= timer.ElapsedMilliseconds)
                        {
                            // Wait until GetTexture() is done.
                            
                            // FIXME: Maybe use an actual thread synchronization technique.
                            while (frameLocked);
                            
                            // Assign the new currentVideo, free the old one.
                            currentVideo = nextVideo;
                            
                            // Get the next frame ready, free the old one.
                            IntPtr oldestFrame = previousFrame;
                            previousFrame = Video.videoStream;
                            Video.videoStream = TheoraPlay.THEORAPLAY_getVideo(Video.theoraDecoder);
                            if (Video.videoStream != IntPtr.Zero)
                            {
                                // Assign next frame, if it exists.
                                nextVideo = TheoraPlay.getVideoFrame(Video.videoStream);
                                
                                // Then free the _really_ old frame.
                                TheoraPlay.THEORAPLAY_freeVideo(oldestFrame);
                            }
                        }
                    }
                    
                    // If we're done decoding, we hit the end.
                    if (TheoraPlay.THEORAPLAY_isDecoding(Video.theoraDecoder) == 0)
                    {
                        // FIXME: This is a part of the Duration hack!
                        Video.Duration = new TimeSpan(0, 0, 0, 0, (int) currentVideo.playms);
                        
                        // Stop and reset the timer.
                        // If we're looping, the loop will start it again.
                        timer.Stop();
                        timer.Reset();
                        
                        // If looping, go back to the start. Otherwise, we'll be exiting.
                        if (IsLooped)
                        {
                            // FIXME: Er, wait, shit.
                            throw new NotImplementedException("Theora looping not implemented!");
                            // Revert to first frame.
                            // AL Stop
                            // AL Rewind
                        }
                        else
                        {
                            State = MediaState.Stopped;
                        }
                    }
                }
            }
            
            // Reset the video timer.
            timer.Stop();
            timer.Reset();
            
            // Stop the decoding, we don't need it anymore.
            audioDecoderThread.Join();
            
            // We're desperately trying to keep this until the very end.
            TheoraPlay.THEORAPLAY_freeVideo(previousFrame);
            
            // We're not playing any video anymore.
            Video.Dispose();
        }
        #endregion
    }
}
