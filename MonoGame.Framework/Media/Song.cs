// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : IEquatable<Song>, IDisposable
    {
        private string _name;
		private int _playCount = 0;
        private TimeSpan _duration = TimeSpan.Zero;
        bool disposed;

#if ANDROID || OPENAL || PSM || WEB
        internal delegate void FinishedPlayingHandler(object sender, EventArgs args);
#if !LINUX
        event FinishedPlayingHandler DonePlaying;
#endif
#endif

        internal Song(string fileName, int durationMS)
            : this(fileName)
        {
            _duration = TimeSpan.FromMilliseconds(durationMS);
        }

		internal Song(string fileName)
		{			
			_name = fileName;

            PlatformInitialize(fileName);
        }

        ~Song()
        {
            Dispose(false);
        }

        internal string FilePath
		{
			get { return _name; }
		}
		
		public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    PlatformDispose(disposing);
                }

                disposed = true;
            }
        }

#if WINDOWS_MEDIA_ENGINE || WINDOWS_PHONE

        private void PlatformDispose(bool disposing)
        {
            // NO OP on Win8.
        }

        private void PlatformInitialize(string fileName)
        {
            // NO OP on Win8.
        }
#endif

        public bool Equals(Song song)
        {
#if DIRECTX
            return song != null && song.FilePath == FilePath;
#else
			return ((object)song != null) && (Name == song.Name);
#endif
		}
		
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
		
		public override bool Equals(Object obj)
		{
			if(obj == null)
			{
				return false;
			}
			
			return Equals(obj as Song);  
		}
		
		public static bool operator ==(Song song1, Song song2)
		{
			if((object)song1 == null)
			{
				return (object)song2 == null;
			}

			return song1.Equals(song2);
		}
		
		public static bool operator !=(Song song1, Song song2)
		{
		  return ! (song1 == song2);
		}

        public TimeSpan Duration
        {
            get
            {
                return _duration;
            }
        }	

        public bool IsProtected
        {
            get
            {
				return false;
            }
        }

        public bool IsRated
        {
            get
            {
				return false;
            }
        }

        public string Name
        {
            get
            {
				return Path.GetFileNameWithoutExtension(_name);
            }
        }

        public int PlayCount
        {
            get
            {
				return _playCount;
            }
        }

        public int Rating
        {
            get
            {
				return 0;
            }
        }

        public int TrackNumber
        {
            get
            {
				return 0;
            }
        }
    }
}

