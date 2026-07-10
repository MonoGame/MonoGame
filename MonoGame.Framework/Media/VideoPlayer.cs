// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// Provides methods and properties to play back, pause, resume, and stop <see cref="Video"/>.
    /// <see cref="VideoPlayer"/> also exposes repeat, volume, and play position information.
    /// </summary>
    public sealed partial class VideoPlayer : IDisposable
    {
        #region Fields

        private MediaState _state;
        private Video _currentVideo;
        private float _volume = 1.0f;
        private bool _isLooped = false;
        private bool _isMuted = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value that indicates whether the object is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets a value that indicates whether the player is playing video in a loop.
        /// </summary>
        public bool IsLooped
        {
            get { return _isLooped; }
            set
            {
                if (_isLooped == value)
                    return;

                _isLooped = value;
                PlatformSetIsLooped();
            }
        }

        /// <summary>
        /// Gets or sets the muted setting for the video player.
        /// </summary>
        public bool IsMuted
        {
            get { return _isMuted; }
            set
            {
                if (_isMuted == value)
                    return;

                _isMuted = value;
                PlatformSetIsMuted();
            }
        }

        /// <summary>
        /// Gets the play position within the currently playing video.
        /// </summary>
        public TimeSpan PlayPosition
        {
            get
            {
                if (_currentVideo == null || State == MediaState.Stopped)
                    return TimeSpan.Zero;

                return PlatformGetPlayPosition();
            }
        }

        /// <summary>
        /// Gets the media playback state, <see cref="MediaState"/>.
        /// </summary>
        public MediaState State
        { 
            get
            {
                // Give the platform code a chance to update 
                // the playback state before we return the result.
                PlatformGetState(ref _state);
                return _state;
            }
        }

        /// <summary>
        /// Gets the <see cref="Media.Video"/> that is currently playing.
        /// </summary>
        public Video Video { get { return _currentVideo; } }

        /// <summary>
        /// Video player volume, from 0.0f (silence) to 1.0f (full volume relative to the current device volume).
        /// </summary>
        public float Volume
        {
            get { return _volume; }
            
            set
            {
                if (value < 0.0f || value > 1.0f)
                    throw new ArgumentOutOfRangeException();

                _volume = value;

                if (_currentVideo != null)
                    PlatformSetVolume();
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Creates a new instance of <see cref="VideoPlayer"/> class.
        /// </summary>
        public VideoPlayer()
        {
            _state = MediaState.Stopped;

            PlatformInitialize();
        }

        /// <summary>
        /// Retrieves a Texture2D containing the current frame of video being played.
        /// </summary>
        /// <returns>The current frame of video.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no video is set on the player</exception>
        /// <exception cref="InvalidOperationException">Thrown if the platform was unable to get a texture in a reasonable amount of time. Often the platform specific media code is running
        /// in a different thread or process. Note: This may be a change from XNA behaviour</exception>
        public Texture2D GetTexture()
        {
            if (_currentVideo == null)
                throw new InvalidOperationException("Operation is not valid due to the current state of the object");

            //XNA never returns a null texture
            const int retries = 5;
            const int sleepTimeFactor = 50;
            Texture2D texture=null;

            for (int i = 0; i < retries; i++)
            {
                texture = PlatformGetTexture();
                if (texture != null)
                {
                    break;
                }
                var sleepTime = i*sleepTimeFactor;
                Debug.WriteLine("PlatformGetTexture returned null ({0}) sleeping for {1} ms", i + 1, sleepTime);
                Thread.Sleep(sleepTime); //Sleep for longer and longer times
            }
            if (texture == null)
            {
                throw new InvalidOperationException("Platform returned a null texture");
            }

            return texture;
        }

        /// <summary>
        /// Pauses the currently playing video.
        /// </summary>
        public void Pause()
        {
            if (_currentVideo == null)
                return;

            PlatformPause();

            _state = MediaState.Paused;
        }

        /// <summary>
        /// Plays a <see cref="Video"/>.
        /// </summary>
        /// <param name="video">Video to play.</param>
        public void Play(Video video)
        {
            if (video == null)
                throw new ArgumentNullException("video is null.");

            if (_currentVideo == video)
            {
                var state = State;
							
                // No work to do if we're already
                // playing this video.
                if (state == MediaState.Playing)
                    return;

                // If we try to Play the same video
                // from a paused state, just resume it instead.
                if (state == MediaState.Paused)
                {
                    PlatformResume();
                    return;
                }
            }
            
            _currentVideo = video;

            PlatformPlay();

            _state = MediaState.Playing;

            // XNA doesn't return until the video is playing
            const int retries = 5;
            const int sleepTimeFactor = 50;

            for (int i = 0; i < retries; i++)
            {
                if (State == MediaState.Playing )
                {
                    break;
                }
                var sleepTime = i*sleepTimeFactor;
                Debug.WriteLine("State != MediaState.Playing ({0}) sleeping for {1} ms", i + 1, sleepTime);
                Thread.Sleep(sleepTime); //Sleep for longer and longer times
            }
            if (State != MediaState.Playing )
            {
                //We timed out - attempt to stop to fix any bad state
                Stop();
                throw new InvalidOperationException("cannot start video"); 
            }
        }

        /// <summary>
        /// Resumes a paused video.
        /// </summary>
        public void Resume()
        {
            if (_currentVideo == null)
                return;

            var state = State;

            // No work to do if we're already playing
            if (state == MediaState.Playing)
                return;

            if (state == MediaState.Stopped)
            {
                PlatformPlay();
                return;
            }

            PlatformResume();

            _state = MediaState.Playing;
        }

        /// <summary>
        /// Stops playing a video.
        /// </summary>
        public void Stop()
        {
            if (_currentVideo == null)
                return;

            PlatformStop();

            _state = MediaState.Stopped;
        }

        #endregion

        #region IDisposable Implementation

        /// <inheritdoc cref="IDisposable.Dispose()"/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                PlatformDispose(disposing);
                IsDisposed = true;
            }
        }

        #endregion

    }
}
