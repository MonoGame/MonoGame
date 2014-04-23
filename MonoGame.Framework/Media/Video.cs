// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// Represents a video.
    /// </summary>
    public sealed partial class Video : IDisposable
	{
		private TimeSpan _duration;
		private float _framesPerSecond = 0.0f;
		private int _height = 0;
		private int _width = 0;
		private VideoSoundtrackType _soundtrackType = VideoSoundtrackType.Dialog;
		private bool _disposed;

		#region Public API

		/// <summary>
		/// I actually think this is a file PATH...
		/// </summary>
		public string FileName { get; private set; }

		/// <summary>
		/// Gets the duration of the Video.
        /// </summary>
        public TimeSpan Duration
        {
            get { return _duration; }
        }

        /// <summary>
        /// Gets the frame rate of this video.
        /// </summary>
        public float FramesPerSecond
        {
            get { return _framesPerSecond; }
        }

        /// <summary>
        /// Gets the height of this video, in pixels.
        /// </summary>
        public int Height
        {
            get { return _height; }
        }

        /// <summary>
        /// Gets the VideoSoundtrackType for this video.
        /// </summary>
        public VideoSoundtrackType VideoSoundtrackType
        {
            get { return _soundtrackType; }
        }

        /// <summary>
        /// Gets the width of this video, in pixels.
        /// </summary>
        public int Width
        {
            get { return _width; }
        }

        #endregion

        #region Internal API

        internal Video(string fileName, float durationMS) :
            this(fileName)
        {
            _duration = TimeSpan.FromMilliseconds(durationMS);
        }

        internal Video(string fileName)
        {
            FileName = fileName;

            PlatformInitialize();
        }

        ~Video()
        {
            Dispose(false);
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                PlatformDispose(disposing);
                _disposed = true;
            }
        }

        #endregion
    }
}
