// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// Provides access to a playlist in the media library.
    /// </summary>
    /// <remarks>
    /// Obtain <see cref="Playlist"/> objects through the <see cref="PlaylistCollection.this"/> indexer.
    /// </remarks>
    public sealed class Playlist : IDisposable
    {
        /// <summary>
        /// Gets the duration of the <see cref="Playlist"/>.
        /// </summary>
        public TimeSpan Duration
        {
            get;
			internal set;
        }

        /// <summary>
        /// Gets the name of the <see cref="Playlist"/>.
        /// </summary>
        public string Name
        {
            get;
			internal set;
        }

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
		public void Dispose()
        {
        }

		
        /*public SongCollection Songs
        {
            get
            {
            }
        }*/
    }
}

