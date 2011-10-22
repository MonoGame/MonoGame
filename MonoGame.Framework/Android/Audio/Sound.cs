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
using Android.Content.Res;

namespace Microsoft.Xna.Framework.Audio
{
    internal class Sound : IDisposable
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
                catch(Exception ex)
                {
                    Log.Debug("Sound.Worker" , "Sound thread: Work Exception" + ex.ToString());
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
		
		~Sound()
		{
			Dispose();	
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

        public double Duration
        {
            get
            {
                return _player != null ? _player.Duration : 0;
            }
        }

        public double CurrentPosition
        {
            get
            {
                return _player != null ? _player.CurrentPosition : 0;
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

            //_player.Start();
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


       
              
        public Sound(string filename, float volume, bool looping)
        {
            this._player = new MediaPlayer();
            // get the Asset Descriptor and Release it when the SetDataSource returns
            // otherwise you cant play the file
            using (AssetFileDescriptor fd = Game.contextInstance.Assets.OpenFd(filename))
            {
                _player.SetDataSource(fd.FileDescriptor);
            }
            _player.Prepared += this.OnPrepared;
            this.Looping = looping;
            this.Volume = volume;            
            // prepare on the background  thread
            try
            {
                _player.PrepareAsync();
            }
            catch (Exception ex)
            {
                Log.Debug("MonoGameInfo", ex.ToString());
            }
        }

        public Sound(byte[] audiodata, float volume, bool looping)
        {
            throw new NotImplementedException();
        }

    }
}