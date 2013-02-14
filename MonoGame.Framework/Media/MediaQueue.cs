using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Media
{
	public sealed class MediaQueue
	{
        List<Song> songs = new List<Song>();
		private int _activeSongIndex = 0;
		private Random random = new Random();
		
		public MediaQueue()
		{
			
		}
		
		public Song ActiveSong
		{
			get
			{
				if (songs.Count == 0)
					return null;
				
				return songs[_activeSongIndex];
			}
		}
		
		public int ActiveSongIndex
		{
			get { return _activeSongIndex; }
			set { _activeSongIndex = value; }
		}

        internal int Count
        {
            get
            {
                return songs.Count;
            }
        }

        internal IEnumerable<Song> Songs
        {
            get
            {
                return songs;
            }
        }

		internal Song GetNextSong(int direction, bool shuffle)
		{
			if (shuffle)
				_activeSongIndex = random.Next(songs.Count);
			else			
				_activeSongIndex = (int)MathHelper.Clamp(_activeSongIndex + direction, 0, songs.Count - 1);
			
			return songs[_activeSongIndex];
		}
		
		internal void Clear()
		{
			Song song;
			for(; songs.Count > 0; )
			{
				song = songs[0];
#if !WINRT
				song.Stop();
#endif
				songs.Remove(song);
			}	
		}

#if !WINRT
        internal void SetVolume(float volume)
        {
            int count = songs.Count;
            for (int i = 0; i < count; ++i)
                songs[i].Volume = volume;
        }
#endif

        internal void Add(Song song)
        {
            songs.Add(song);
        }

#if !WINRT
        internal void Stop()
        {
            int count = songs.Count;
            for (int i = 0; i < count; ++i)
                songs[i].Stop();
        }
#endif
	}
}

