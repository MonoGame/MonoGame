using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Microsoft.Xna.Framework.Audio
{
    public class Sound
    {
        internal MediaPlayer _player;
        private float _Volume;
        private bool _Looping;

        private static bool Running = true;
        private static Queue<Action> WorkItems = new Queue<Action>();

        private static void Enqueue(Action workItem)
        {
            lock (Sound.WorkItems)
            {
                Sound.WorkItems.Enqueue(workItem);
                Monitor.Pulse(Sound.WorkItems);
            }
        }

        private static void Worker(object state)
        {
            while (Sound.Running)
            {
                Action workItem;

                lock (Sound.WorkItems)
                {
                    if (Sound.WorkItems.Count == 0)
                        Monitor.Wait(Sound.WorkItems);

                    workItem = Sound.WorkItems.Dequeue();
                }

                try
                {
                    workItem();
                }
                catch
                {
                    Log.Debug("Sound.Worker" , "Sound thread: Work Exception");
                }
            }
        }

        static Sound()
        {
            ThreadPool.QueueUserWorkItem(Worker);
        }
        
        private Sound(MediaPlayer player)
        {
            _player = player;
        }
		
		public void Dispose()
		{
			_player.Dispose();
		}

        public float Volume
        {
            get { return this._Volume; }
            set
            {
                this._Volume = value;
                if (this._player == null)
                    return;

                _player.SetVolume(value, value);

            }
        }

        public bool Looping
        {
            get { return this._Looping; }
            set
            {
                if (this._Looping != value) {
                    this._Looping = value;

                    if (_player == null)
                        return;

                    _player.Looping = value;
                }
            }

        }

        public void Play()
        {
            if (this._player == null)
                return;

            Sound.Enqueue(_player.Start);
        }
		
		public void Pause()
        {
            if (this._player == null)
                return;

            lock (Sound.WorkItems) {
                _player.Pause();
            }
        }
		
        public void Stop()
        {
            if (this._player == null)
                return;

            lock (Sound.WorkItems) {
                _player.Stop();
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
				return _player.IsPlaying;
			}
		}

        internal bool IsPrepared { get; private set; }

        protected void OnPrepared(object sender, EventArgs e)
        {
            IsPrepared = true;
        }

        public static Sound Create(string assetPath, float volume, bool looping)
        {
            MediaPlayer player = new MediaPlayer();
            Sound sound = new Sound(player);
			//This breaks the platformer sample. Not sure if it works anywhere else
            //player.SetDataSource(Game.contextInstance.Assets.OpenFd(assetPath).FileDescriptor);
			player.SetDataSource(assetPath);
            player.Prepared += sound.OnPrepared;
            sound.Looping = looping;
            sound.Volume = volume;
            
            Sound.Enqueue(player.Prepare);

            return sound;
        }

        public static Sound CreateAndPlay(string url, float volume, bool looping)
        {
            Sound sound = Sound.Create(url, volume, looping);
            sound.Play();

            return sound;
        }

    }
}