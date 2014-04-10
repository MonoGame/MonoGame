// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using MonoTouch.MediaPlayer;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

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

        private void PlatformPause()
        {
            throw new NotImplementedException();
        }

        private void PlatformResume()
        {
            _currentVideo.Player.Start();
        }

        private void PlatformPlay()
        {
            _platform.IsPlayingVideo = true;

            _playbackDidFinishObserver = NSNotificationCenter.DefaultCenter.AddObserver(
                MPMoviePlayerController.PlaybackDidFinishNotification, OnStop);

            _currentVideo.MovieView.MoviePlayer.RepeatMode = IsLooped ? MPMovieRepeatMode.One : MPMovieRepeatMode.None;

            _platform.ViewController.PresentModalViewController(_currentVideo.MovieView, animated: false);
            _currentVideo.MovieView.MoviePlayer.Play();
        }

        private void PlatformStop()
        {
            if (_playbackDidFinishObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_playbackDidFinishObserver);
                _playbackDidFinishObserver = null;
            }

            _currentVideo.MovieView.MoviePlayer.Stop();
            _platform.IsPlayingVideo = false;
            _platform.ViewController.DismissModalViewControllerAnimated(false);
        }

        private void OnStop(NSNotification e)
        {
            Stop();
        }

                private TimeSpan PlatformGetPlayPosition()
        {
            throw new NotImplementedException();
        }

        private TimeSpan PlatformSetVolume()
        {
            throw new NotImplementedException();
        }
    }
}