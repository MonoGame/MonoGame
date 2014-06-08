// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

#if IOS
using MonoTouch.MediaPlayer;
#endif

namespace Microsoft.Xna.Framework.Media
{
    public static partial class MediaPlayer
    {

        #region Properties
        
        private static void PlatformInitialize()
        {

        }

        private static bool PlatformGetIsMuted()
        {
            return _isMuted;
        }

        private static void PlatformSetIsMuted(bool muted)
        {
            _isMuted = muted;

            if (_queue.Count == 0)
                return;

            var newVolume = _isMuted ? 0.0f : _volume;
            _queue.SetVolume(newVolume);
        }

        private static bool PlatformGetIsRepeating()
        {
            return _isRepeating;
        }

        private static void PlatformSetIsRepeating(bool repeating)
        {
            _isRepeating = repeating;
        }

        private static bool PlatformGetIsShuffled()
        {
            return _isShuffled;
        }

        private static void PlatformSetIsShuffled(bool shuffled)
        {
            _isShuffled = shuffled;
        }

        private static TimeSpan PlatformGetPlayPosition()
        {
            if (_queue.ActiveSong == null)
                return TimeSpan.Zero;

            return _queue.ActiveSong.Position;
        }

#if IOS
        private static void PlatformSetPlayPosition(TimeSpan playPosition)
        {
            if (_queue.ActiveSong != null)
                _queue.ActiveSong.Position = playPosition;
        }
#endif

        private static MediaState PlatformGetState()
        {
            return _state;
        }

        private static float PlatformGetVolume()
        {
            return _volume;
        }

        private static void PlatformSetVolume(float volume)
        {
            _volume = volume;

            if (_queue.ActiveSong == null)
                return;

            _queue.SetVolume(_isMuted ? 0.0f : _volume);
        }

        private static bool PlatformGetGameHasControl()
        {
#if IOS
            var musicPlayer = MPMusicPlayerController.iPodMusicPlayer;
				
			if (musicPlayer == null)
				return true;
				
			// TODO: Research the Interrupted state and see if it's valid to
			// have control at that time.
				
			// Note: This will throw a bunch of warnings/output to the console
			// if running in the simulator. This is a known issue:
			// http://forums.macrumors.com/showthread.php?t=689102
			if (musicPlayer.PlaybackState == MPMusicPlaybackState.Playing || 
				musicPlayer.PlaybackState == MPMusicPlaybackState.SeekingForward ||
				musicPlayer.PlaybackState == MPMusicPlaybackState.SeekingBackward)
				return false;
				
			return true;
#endif

            // TODO: Fix me!
            return true;
        }
		#endregion

        private static void PlatformPause()
        {
            if (_queue.ActiveSong == null)
                return;

            _queue.ActiveSong.Pause();
        }

        private static void PlatformPlaySong(Song song)
        {
            if (_queue.ActiveSong == null)
                return;

            song.SetEventHandler(OnSongFinishedPlaying);

            song.Volume = _isMuted ? 0.0f : _volume;
            song.Play();
        }

        private static void PlatformResume()
        {
            if (_queue.ActiveSong == null)
                return;

            _queue.ActiveSong.Resume();
        }

        private static void PlatformStop()
        {
            // Loop through so that we reset the PlayCount as well
            foreach (var song in Queue.Songs)
                _queue.ActiveSong.Stop();
        }
    }
}

