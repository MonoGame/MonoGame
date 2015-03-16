// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if WINDOWS_PHONE
extern alias MicrosoftXnaFramework;
using MsArtist = MicrosoftXnaFramework::Microsoft.Xna.Framework.Media.Artist;
#endif
using System;

namespace Microsoft.Xna.Framework.Media
{
    public sealed class Artist : IDisposable
    {
#if WINDOWS_PHONE
        private MsArtist artist;
#else
        private string artist;
#endif

        /// <summary>
        /// Gets the AlbumCollection for the Artist.
        /// </summary>
        public AlbumCollection Albums
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the object is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get
            {
#if WINDOWS_PHONE
                return this.artist.IsDisposed;
#else
                return true;
#endif
            }
        }

        /// <summary>
        /// Gets the name of the Artist.
        /// </summary>
        public string Name
        {
            get
            {
#if WINDOWS_PHONE
                return this.artist.Name;
#else
                return this.artist;
#endif
            }
        }

        /// <summary>
        /// Gets the SongCollection for the Artist.
        /// </summary>
        public SongCollection Songs
        {
            get
            {
                throw new NotImplementedException();
            }
        }

#if WINDOWS_PHONE
        public static implicit operator Artist(MsArtist artist)
        {
            return new Artist(artist);
        }

        private Artist(MsArtist artist)
        {
            this.artist = artist;
        }
#else
        public Artist(string artist)
        {
            this.artist = artist;
        }
#endif

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        public void Dispose()
        {
#if WINDOWS_PHONE
            this.artist.Dispose();
#endif
        }

        /// <summary>
        /// Returns a String representation of the Artist.
        /// </summary>
        public override string ToString()
        {
            return this.artist.ToString();
        }

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return this.artist.GetHashCode();
        }
    }
}
