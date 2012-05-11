using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Media
{
	public class MediaQueue : List<Song>
	{
		private int _activeSongIndex = 0;
		private Random random = new Random();
		
		public MediaQueue () : base()
		{
			
		}
		
		
		public Song ActiveSong
		{
			get
			{
				if (this.Count == 0)
					return null;
				
				return this[_activeSongIndex];
			}
		}
		
		public int ActiveSongIndex
		{
			get { return _activeSongIndex; }
			set { _activeSongIndex = value; }
		}
		
		internal Song getNextSong(int direction, bool shuffle)
		{
			if (shuffle)
				_activeSongIndex = random.Next(this.Count);
			else
			{
				_activeSongIndex += direction;
				
				if (_activeSongIndex >= Count)
					_activeSongIndex = 0;
				else if (_activeSongIndex < 0)
					_activeSongIndex = Count - 1;
			}
			
			return this[_activeSongIndex];
		}
		
		internal new void Clear ()
		{
			Song song;
			for( int x = 0; x < this.Count; x++ )
			{
				song = this[x];
				song.Stop();
				this.Remove(song);
			}	
		}
		
		internal void queueSong(Song song)
		{
			this.Add(song);
		}
	}
}

