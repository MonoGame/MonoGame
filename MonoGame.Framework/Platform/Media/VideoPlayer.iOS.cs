// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using Foundation;
using AVKit;
using AVFoundation;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class VideoPlayer : IDisposable
    {
        private Game _game;
        private iOSGamePlatform _platform;
        private NSObject _playbackDidFinishObserver;

        private void PlatformInitialize()
        {
            _game = Game.Instance;
            _platform = (iOSGamePlatform)_game.Services.GetService(typeof(iOSGamePlatform));

            if (_platform == null)
                throw new InvalidOperationException("No iOSGamePlatform instance was available");
        }

        private Texture2D PlatformGetTexture()
        {
            throw new NotImplementedException();
        }

        private void PlatformGetState(ref MediaState result)
        {
        }

        private void PlatformPause()
        {
            _currentVideo.MovieView.Player.Pause ();
        }

        private void PlatformResume()
        {
            _currentVideo.MovieView.Player.Play ();
        }

        private void PlatformPlay()
        {
            _platform.IsPlayingVideo = true;

            _playbackDidFinishObserver = AVPlayerItem.Notifications.ObserveDidPlayToEndTime(OnStop);

            _platform.ViewController.PresentViewController(_currentVideo.MovieView, false, null);
            _currentVideo.MovieView.Player.Play();
        }

        private void PlatformStop()
        {
            if (_playbackDidFinishObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_playbackDidFinishObserver);
                _playbackDidFinishObserver = null;
            }

            _currentVideo.MovieView.Player.Pause ();
            _currentVideo.MovieView.Player.Rate = 0f;

            _platform.IsPlayingVideo = false;
            _platform.ViewController.DismissViewController(false, null);
        }

        private void OnStop(object sender, NSNotificationEventArgs args)
        {
            Stop();
        }

        private TimeSpan PlatformGetPlayPosition()
        {
            return TimeSpan.FromSeconds (_currentVideo.MovieView.Player.CurrentTime.Seconds);
        }

        private void PlatformSetIsLooped()
        {
            throw new NotImplementedException();
        }

        private void PlatformSetIsMuted()
        {
            throw new NotImplementedException();
        }

        private TimeSpan PlatformSetVolume()
        {
            throw new NotImplementedException();
        }

        private void PlatformDispose(bool disposing)
        {
        }
    }
}