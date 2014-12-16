// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if WINDOWS_PHONE
extern alias MicrosoftXnaFramework;
using MsGenre = MicrosoftXnaFramework::Microsoft.Xna.Framework.Media.Genre;
#endif
using System;

namespace Microsoft.Xna.Framework.Media
{
    public sealed class Genre : IDisposable
    {
#if WINDOWS_PHONE
        private MsGenre genre;
#else
        private string genre;
#endif

        /// <summary>
        /// Gets the AlbumCollection for the Genre.
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
                return this.genre.IsDisposed;
#else
                return true;
#endif
            }
        }

        /// <summary>
        /// Gets the name of the Genre.
        /// </summary>
        public string Name
        {
            get
            {
#if WINDOWS_PHONE
                return this.genre.Name;
#else
                return this.genre;
#endif
            }
        }

        /// <summary>
        /// Gets the SongCollection for the Genre.
        /// </summary>
        public SongCollection Songs
        {
            get
            {
                throw new NotImplementedException();
            }
        }

#if WINDOWS_PHONE
        public static implicit operator Genre(MsGenre genre)
        {
            return new Genre(genre);
        }

        private Genre(MsGenre genre)
        {
            this.genre = genre;
        }
#else
        public Genre(string genre)
        {
            this.genre = genre;
        }
#endif

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        public void Dispose()
        {
#if WINDOWS_PHONE
            this.genre.Dispose();
#endif
        }

        /// <summary>
        /// Returns a String representation of the Genre.
        /// </summary>
        public override string ToString()
        {
#if WINDOWS_PHONE
            return this.genre.ToString();
#else
            return this.genre;
#endif
        }

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
#if WINDOWS_PHONE
            return this.genre.GetHashCode();
#else
            return this.genre.GetHashCode();
#endif
        }
    }
}
