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


namespace Microsoft.Xna.Framework.Media
{
    public static partial class MediaPlayer
    {
        //RAYB: This probably needs to get flipped back into a readonly.
        private static  MediaSession _session;
        private static SimpleAudioVolume _volumeController;
        private static PresentationClock _clock;

        // HACK: Need SharpDX to fix this.
        private static readonly Guid MRPolicyVolumeService = Guid.Parse("1abaa2ac-9d3b-47c6-ab48-c59506de784d");
        private static readonly Guid SimpleAudioVolumeGuid = Guid.Parse("089EDF13-CF71-4338-8D13-9E569DBDC319");

	    private static Callback _callback;

	    private class Callback : IAsyncCallback
	    {
		    public void Dispose()
		    {
		    }

		    public IDisposable Shadow { get; set; }
		    public void Invoke(AsyncResult asyncResultRef)
		    {
			    var ev = _session.EndGetEvent(asyncResultRef);
			
			    if (ev.TypeInfo == MediaEventTypes.EndOfPresentation)
				    OnSongFinishedPlaying(null, null);

			    _session.BeginGetEvent(this, null);
		    }

		    public AsyncCallbackFlags Flags { get; private set; }
		    public WorkQueueId WorkQueueId { get; private set; }
	    }

        private static void PlatformInitialize()
        {
            MediaManagerState.CheckStartup();
            MediaFactory.CreateMediaSession(null, out _session);
        }

        #region Properties

        private static void PlatformSetIsMuted()
        {
            if (_volumeController != null)
                _volumeController.Mute = _isMuted;
        }

        private static TimeSpan PlatformGetPlayPosition()
        {
            return _clock != null ? TimeSpan.FromTicks(_clock.Time) : TimeSpan.Zero;
        }

        private static bool PlatformGetGameHasControl()
        {
            // TODO: Fix me!
            return true;
        }

        private static void PlatformSetVolume()
        {
			if (_volumeController != null)
                _volumeController.MasterVolume = _volume;
        }
		
		#endregion

        private static void PlatformPause()
        {
            _session.Pause();
        }

        private static void PlatformPlaySong(Song song)
        {
            // Cleanup the last song first.
            if (State != MediaState.Stopped)
            {
				_session.Stop();
				_session.Close();
                _volumeController.Dispose();
                _clock.Dispose();
			}

            // Set the new song.
            _session.SetTopology(0, song.Topology);

            // Get the volume interface.
            IntPtr volumeObj;

            
            try
            {
                MediaFactory.GetService(_session, MRPolicyVolumeService, SimpleAudioVolumeGuid, out volumeObj);
            }
            catch
            {
                MediaFactory.GetService(_session, MRPolicyVolumeService, SimpleAudioVolumeGuid, out volumeObj);
            }  
          

            _volumeController = CppObject.FromPointer<SimpleAudioVolume>(volumeObj);
            _volumeController.Mute = _isMuted;
            _volumeController.MasterVolume = _volume;

            // Get the clock.
            _clock = _session.Clock.QueryInterface<PresentationClock>();

			//create the callback if it hasn't been created yet
			if (_callback == null)
			{
				_callback = new Callback();
				_session.BeginGetEvent(_callback, null);
			}

            // Start playing.
            var varStart = new Variant();
            _session.Start(null, varStart);
        }

        private static void PlatformResume()
        {
            _session.Start(null, null);
        }

        private static void PlatformStop()
		{
			_session.ClearTopologies();
			_session.Stop();
			_session.Close();
            _volumeController.Dispose();
            _volumeController = null;
            _clock.Dispose();
            _clock = null;
        }
    }
}

