using System;
using Android.Content;
using Android.Content.Res;
using Android.Media;
using Android.Util;

namespace Microsoft.Xna.Framework.Audio
{
    internal class Sound : IDisposable
    {
        private const int MAX_SIMULTANEOUS_SOUNDS = 10;
        private static SoundPool s_soundPool = new SoundPool(MAX_SIMULTANEOUS_SOUNDS, Stream.Music, 0);
        private int _soundId;
        private int _streamId;
		
		internal static SoundPool SoundPool
		{
			get {
				return s_soundPool;
			}
			
		}
		
		internal static void PauseAll()
		{
			s_soundPool.AutoPause();
		}
		
		internal static void ResumeAll()
		{
			s_soundPool.AutoResume();
		}

        ~Sound()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_soundId != 0)
            {
                s_soundPool.Unload(_soundId);
            }
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
                return false; // cant get this from soundpool.
            }
        }

        public void Play()
        {
            if (_soundId == 0)
                return;

            AudioManager audioManager = (AudioManager)Game.Activity.GetSystemService(Context.AudioService);

            float streamVolumeCurrent = audioManager.GetStreamVolume(Stream.Music);
            float streamVolumeMax = audioManager.GetStreamMaxVolume(Stream.Music);
            float streamVolume = streamVolumeCurrent / streamVolumeMax;
            float panRatio = (this.Pan + 1.0f) / 2.0f;
            float volumeLeft = Volume * streamVolume * (1.0f - panRatio);
            float volumeRight = Volume * streamVolume * panRatio;

            float rate = (float)Math.Pow(2, Rate);
            rate = Math.Max(Math.Min(rate, 2.0f), 0.5f);

            _streamId = s_soundPool.Play(_soundId, volumeLeft, volumeRight, 1, Looping ? -1 : 0, rate);
        }

        public void Pause()
        {
            s_soundPool.Pause(_streamId);
        }
		
        public void Stop()
        {
            s_soundPool.Stop(_streamId);
        }

        public Sound(string filename, float volume, bool looping)
        {
            using (AssetFileDescriptor fd = Game.Activity.Assets.OpenFd(filename))
                _soundId = s_soundPool.Load(fd.FileDescriptor, fd.StartOffset, fd.Length, 1);

            this.Looping = looping;
            this.Volume = volume;
        }

        public Sound(byte[] audiodata, float volume, bool looping)
        {
            _soundId = 0;
            //throw new NotImplementedException();
        }

        internal static void IncreaseMediaVolume()
        {
            AudioManager audioManager = (AudioManager)Game.Activity.GetSystemService(Context.AudioService);

            audioManager.AdjustStreamVolume(Stream.Music, Adjust.Raise, VolumeNotificationFlags.ShowUi);
        }

        internal static void DecreaseMediaVolume()
        {
            AudioManager audioManager = (AudioManager)Game.Activity.GetSystemService(Context.AudioService);

            audioManager.AdjustStreamVolume(Stream.Music, Adjust.Lower, VolumeNotificationFlags.ShowUi);
        }
    }
}