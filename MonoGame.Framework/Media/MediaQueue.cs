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
				_activeSongIndex = (int)MathHelper.Clamp(_activeSongIndex + direction, 0, Count - 1);
			
			return this[_activeSongIndex];
		}
		
		internal new void Clear ()
		{
			Song song;
			for(; this.Count > 0; )
			{
				song = this[0];
				song.Stop();
				this.Remove(song);
			}	
		}
	}
}

