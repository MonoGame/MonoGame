using System;

using Sce.Pss.Core.Audio;

namespace Microsoft.Xna.Framework.Audio
{
    internal class Sound : IDisposable
    {
		private Sound _sound;
        private SoundPlayer _soundPlayer;
		
		

        ~Sound()
        {
            Dispose();
        }

        public void Dispose()
        {
            _soundPlayer.Stop();
			_soundPlayer.Dispose();
            _sound.Dispose();
        }

        public void Resume()
        {
            if (_soundId != 0)
            {
                s_soundPool.Resume(_soundId);
            }
        }

        public float Volume { get; set; }
        public bool Looping { get; set; }
        public float Rate { get; set; }
        public float Pan { get; set; }

        public double Duration
        {
            get { return 0; } // cant get this from soundpool.
        }

        public double CurrentPosition
        {
            get { return 0; } // cant get this from soundpool.
        }

        public bool Playing
        {
            get
            {
                return _soundPlayer.Status == SoundStatus.Playing; // cant get this from soundpool.
            }
        }

        public void Play()
        {
			if (_soundPlayer != null )
			{
				_soundPlayer.Volume = this.Volume;
				_soundPlayer.PlaybackRate = this.Rate;
				_soundPlayer.Pan = this.Pan;
				_soundPlayer.Loop = this.Looping;
				_soundPlayer.Play();
			}
            
        }

        public void Pause()
        {
            if (_soundPlayer != null )
			{
				_soundPlayer.Stop();
			}
        }
		
        public void Stop()
        {
            if (_soundPlayer != null )
			{
				_soundPlayer.Stop();
			}
        }

        public Sound(string filename, float volume, bool looping)
        {
			_sound = new Sound(filename);
            _soundPlayer = sound.CreatePlayer();           

            this.Looping = looping;
            this.Volume = volume;
        }

        public Sound(byte[] audiodata, float volume, bool looping)
        {
			_sound = new Sound(audiodata);
            _soundPlayer = sound.CreatePlayer();           

            this.Looping = looping;
            this.Volume = volume;
        }        
    }
}