using System;
using System.IO;
using OpenTK.Audio.OpenAL;

namespace Microsoft.Xna.Framework.Audio
{	
	public class Sound
	{	
		private int bufferID;
		private int sourceID;
		
		public Sound(string url, float volume, bool looping)
		{
			ALFormat format;
			int size;
			int freq;
			byte[] data;
			Stream s;
			
			try
			{				
		 		s = File.OpenRead(new Uri(url).LocalPath);
			}
			catch(IOException e)
			{
				throw new Content.ContentLoadException("Could not load audio data", e);
			}	
			
			data = AudioLoader.Load(s, out format, out size, out freq);
			
			s.Close();
			
			Initialize(data, format, size, freq, volume, looping);
		}
		
		public Sound(byte[] audiodata, float volume, bool looping) 
		{
			ALFormat format;
			int size;
			int freq;
			byte[] data;
			Stream s;
			
			try
			{				
		 		s = new MemoryStream(audiodata);
			}
			catch(IOException e)
			{
				throw new Content.ContentLoadException("Could not load audio data", e);
			}	
			
			data = AudioLoader.Load(s, out format, out size, out freq);
			
			s.Close();
			
			Initialize(data, format, size, freq, volume, looping);			
		}
		
		private void Initialize(byte[] data, ALFormat format, int size, int frequency, 
		                        float volume, bool looping)
		{
			bufferID = AL.GenBuffer();
			sourceID = AL.GenSource();
			
			try
			{
				AL.BufferData(bufferID, format, data, size, frequency);				
			}
			catch(Exception ex)
			{
				throw ex;	
			}
			
			Volume = volume;
			Looping = looping;
		}
		
		public void Dispose()
		{
			if (bufferID == -1)
				return;			
			
			AL.DeleteSource(sourceID);
			AL.DeleteBuffer(bufferID);
			bufferID = -1;
		}
		
		public double Duration
		{
			get
			{
//				return _audioPlayer.Duration();
				throw new NotImplementedException();
			}
		}
		
		public double CurrentPosition
		{
			get
			{
//				return _audioPlayer.CurrentTime;
				throw new NotImplementedException();
			}
			set
			{
//				_audioPlayer.CurrentTime = value;
				throw new NotImplementedException();
			}
		}
			
		public bool Looping
		{
			get
			{
//				return _audioPlayer.Loops;
				throw new NotImplementedException();
			}
			set
			{
//				_audioPlayer.Loops = value;
				throw new NotImplementedException();
			}
		}
		
		public float Pan
		{
			get;
			set;
		}
		
		public bool Playing
		{
			get
			{
//				return _audioPlayer.IsPlaying();
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
				if ( value )
				{
//					if (!_audioPlayer.IsPlaying())
//					{
//						Play();
//					}
				}
				else
				{
//					if (_audioPlayer.IsPlaying())
//					{
//						Stop();
//					}
				}
			}
		}
		
		public void Pause()
		{		
//			//HACK: Stopping or pausing NSSound is really slow (~200ms), don't if the sample is short :/
//			if (Duration > 2) {
//				_audioPlayer.Pause();
//			}
		}
		
		public void Play()
		{		
			AL.SourcePlay(sourceID);
		}
		
		public void Stop()
		{			
//			if (Duration > 2) { 
//				_audioPlayer.Stop();
//			}
		}
		
		public float Volume
		{
			get
			{
//				return _audioPlayer.Volume;
				throw new NotImplementedException();
			}
			set
			{
//				_audioPlayer.Volume = value;
				throw new NotImplementedException();
			}
		}
		
		public static Sound CreateAndPlay(string url, float volume, bool looping)
		{
//			Sound sound = new Sound(url, volume, looping);
//			
//			sound.Play();
//			
//			return sound;
			throw new NotImplementedException();
		}
	}
}
