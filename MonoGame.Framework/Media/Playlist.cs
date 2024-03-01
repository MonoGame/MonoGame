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
    /// Obtain Playlist objects through the <see cref="PlaylistCollection.this">PlaylistCollection.this</see> indexer.
    /// </remarks>
    public sealed class Playlist : IDisposable
    {
        /// <summary>
        /// Gets the duration of the Playlist.
        /// </summary>
        public TimeSpan Duration
        {
            get;
			internal set;
        }

        /// <summary>
        /// Gets the name of the Playlist.
        /// </summary>
        public string Name
        {
            get;
			internal set;
        }

        /// <inheritdoc cref="IDisposable.Dispose()"/>
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

