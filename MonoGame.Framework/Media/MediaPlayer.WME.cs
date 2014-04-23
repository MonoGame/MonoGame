// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Audio;
using System.Linq;
using SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;
using SharpDX.Win32;
using Windows.UI.Core;


namespace Microsoft.Xna.Framework.Media
{
    public static partial class MediaPlayer
    {
        // RAYB: This needs to be turned back into a readonly.
        private static MediaEngine _mediaEngineEx;
        private static CoreDispatcher _dispatcher;

        private static void PlatformInitialize()
        {
            MediaManager.Startup(true);
            using (var factory = new MediaEngineClassFactory())
            using (var attributes = new MediaEngineAttributes { AudioCategory = AudioStreamCategory.GameMedia })
            {
                var creationFlags = MediaEngineCreateFlags.AudioOnly;

                var mediaEngine = new MediaEngine(factory, attributes, creationFlags, MediaEngineExOnPlaybackEvent);
                _mediaEngineEx = mediaEngine.QueryInterface<MediaEngineEx>();
            }

            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
        }

        private static void MediaEngineExOnPlaybackEvent(MediaEngineEvent mediaEvent, long param1, int param2)
        {
            if (mediaEvent != MediaEngineEvent.Ended)
                return;

            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => OnSongFinishedPlaying(null, null)).AsTask();
        }

        #region Properties

        private static void PlatformSetIsMuted()
        {
            _mediaEngineEx.Muted = _isMuted;
        }

        private static void PlatformSetIsRepeating()
        {
            _mediaEngineEx.Loop = _isRepeating;
        }

        private static TimeSpan PlatformGetPlayPosition()
        {
            return TimeSpan.FromSeconds(_mediaEngineEx.CurrentTime);
        }

        private static bool PlatformGetGameHasControl()
        {
            // TODO: Fix me!
            return true;
        }

        private static void PlatformSetVolume()
        {
            _mediaEngineEx.Volume = _volume;       
        }
		
		#endregion

        private static void PlatformPause()
        {
            _mediaEngineEx.Pause();
        }

        private static void PlatformPlaySong(Song song)
        {
            _mediaEngineEx.Source = song.FilePath;            
            _mediaEngineEx.Load();
            _mediaEngineEx.Play();
        }

        private static void PlatformResume()
        {
            _mediaEngineEx.Play(); 
        }

        private static void PlatformStop()
        {
            _mediaEngineEx.Source = null;
        }
    }
}

