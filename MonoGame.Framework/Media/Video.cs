// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// Represents a video.
    /// </summary>
    public sealed partial class Video : IDisposable
	{
		private bool _disposed;
		private GraphicsDevice _graphicsDevice;

		internal GraphicsDevice GraphicsDevice
		{
			get { return _graphicsDevice; }
			private set { _graphicsDevice = value; }
		}

		#region Public API

		/// <summary>
		/// I actually think this is a file PATH...
		/// </summary>
		public string FileName { get; private set; }

		/// <summary>
		/// Gets the duration of the Video.
        /// </summary>
        public TimeSpan Duration { get; internal set; }

        /// <summary>
        /// Gets the frame rate of this video.
        /// </summary>
        public float FramesPerSecond { get; internal set; }

        /// <summary>
        /// Gets the width of this video, in pixels.
        /// </summary>
        public int Width { get; internal set; }

        /// <summary>
        /// Gets the height of this video, in pixels.
        /// </summary>
        public int Height { get; internal set; }

        /// <summary>
        /// Gets the VideoSoundtrackType for this video.
        /// </summary>
        public VideoSoundtrackType VideoSoundtrackType { get; internal set; }

        #endregion

        #region Internal API

        internal Video(string fileName, GraphicsDevice graphicsDevice)
        {
            FileName = fileName;
            _graphicsDevice = graphicsDevice;
            PlatformInitialize();
        }

        partial void PlatformInitialize();

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

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                PlatformDispose(disposing);
                _disposed = true;
            }
        }

        partial void PlatformDispose(bool disposing);

        #endregion
    }
}
