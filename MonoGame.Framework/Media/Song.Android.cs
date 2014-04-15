// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : IEquatable<Song>, IDisposable
    {
        static Android.Media.MediaPlayer _androidPlayer;
        static Song _playingSong;

        private void PlatformInitialize(string fileName)
        {
            if (_androidPlayer == null)
            {
                _androidPlayer = new Android.Media.MediaPlayer();
                _androidPlayer.Completion += new EventHandler(AndroidPlayer_Completion);
            }
        }

        static void AndroidPlayer_Completion(object sender, EventArgs e)
        {
            var playingSong = _playingSong;
            _playingSong = null;

            if (playingSong != null && playingSong.DonePlaying != null)
                playingSong.DonePlaying(sender, e);
        }

        /// <summary>
        /// Set the event handler for "Finished Playing". Done this way to prevent multiple bindings.
        /// </summary>
        internal void SetEventHandler(FinishedPlayingHandler handler)
        {
            if (DonePlaying != null)
                return;
            DonePlaying += handler;
        }

        private void PlatformDispose(bool disposing)
        {
            // Appears to be a noOp on Android
        }

        internal void Play()
        {
            // Prepare the player
            var afd = Game.Activity.Assets.OpenFd(_name);
            if (afd != null)
            {
                _androidPlayer.Reset();
                _androidPlayer.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
                _androidPlayer.Prepare();
                _androidPlayer.Looping = MediaPlayer.IsRepeating;
                _playingSong = this;
            }

            _androidPlayer.Start();
            _playCount++;
        }

        internal void Resume()
        {
            _androidPlayer.Start();
        }

        internal void Pause()
        {
            _androidPlayer.Pause();
        }

        internal void Stop()
        {
            _androidPlayer.Stop();
            _playingSong = null;
            _playCount = 0;
        }

        internal float Volume
        {
            get
            {
                return 0.0f;
            }

            set
            {
                _androidPlayer.SetVolume(value, value);
            }
        }

        public TimeSpan Position
        {
            get
            {
                if (_playingSong == this)
                    return TimeSpan.FromMilliseconds(_androidPlayer.CurrentPosition);

                return TimeSpan.Zero;
            }
        }
    }
}

