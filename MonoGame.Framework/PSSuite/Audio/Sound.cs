using System;

using PssSound = Sce.Pss.Core.Audio.Sound;
using Sce.Pss.Core.Audio;

namespace Microsoft.Xna.Framework.Audio
{
	/// <summary>
	/// Pause,Resume,Duration,CurrentPosition are unsupported on PSS
	/// </summary>
    internal class Sound : IDisposable
    {
		private PssSound _sound;
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
			Play();
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
                return _soundPlayer.Status == SoundStatus.Playing;
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
			_sound = new PssSound(filename);
            _soundPlayer = _sound.CreatePlayer();           

            this.Looping = looping;
            this.Volume = volume;
            this.Rate = 1.0f;
        }

        public Sound(byte[] audiodata, float volume, bool looping)
        {
			_sound = new PssSound(audiodata);
            _soundPlayer = _sound.CreatePlayer();           

            this.Looping = looping;
            this.Volume = volume;
            this.Rate = 1.0f;
        }
    }
}