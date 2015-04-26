// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Win32;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Media
{
    public static partial class MediaPlayer
    {
        //RAYB: This probably needs to get flipped back into a readonly.
        private static MediaSession _session;
        private static AudioStreamVolume _volumeController;
        private static PresentationClock _clock;

        private static Guid AudioStreamVolumeGuid;

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
            // The GUID is specified in a GuidAttribute attached to the class
            AudioStreamVolumeGuid = Guid.Parse(((GuidAttribute)typeof(AudioStreamVolume).GetCustomAttributes(typeof(GuidAttribute), false)[0]).Value);

            MediaManagerState.CheckStartup();
            MediaFactory.CreateMediaSession(null, out _session);
        }

        #region Properties

        private static bool PlatformGetIsMuted()
        {
            return _isMuted;
        }

        private static void PlatformSetIsMuted(bool muted)
        {
            _isMuted = muted;

            SetChannelVolumes();
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
            return _clock != null ? TimeSpan.FromTicks(_clock.Time) : TimeSpan.Zero;
        }

        private static bool PlatformGetGameHasControl()
        {
            // TODO: Fix me!
            return true;
        }

        private static MediaState PlatformGetState()
        {
            return _state;
        }

        private static float PlatformGetVolume()
        {
            return _volume;
        }

        private static void SetChannelVolumes()
        {
            if (_volumeController != null)
            {
                float volume = _volume;
                if (IsMuted)
                    volume = 0.0f;

                for (int i = 0; i < _volumeController.ChannelCount; i++)
                {
                    _volumeController.SetChannelVolume(i, volume);
                }
            }
        }

        private static void PlatformSetVolume(float volume)
        {
            _volume = volume;
            SetChannelVolumes();
        }

        #endregion

        internal static IntPtr GetVolumeObj(MediaSession session)
        {
            // Get the volume interface - shared between MediaPlayer and VideoPlayer
            const int retries = 10;
            const int sleepTimeFactor = 50;

            var volumeObj = (IntPtr)0;

            //See https://github.com/mono/MonoGame/issues/2620
            //MediaFactory.GetService throws a SharpDX exception for unknown reasons. it appears retrying will solve the problem but there
            //is no specific number of times, nor pause that works. So we will retry N times with an increasing Sleep between each one
            //before finally throwing the error we saw in the first place.
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    MediaFactory.GetService(session, MediaServiceKeys.StreamVolume, AudioStreamVolumeGuid, out volumeObj);
                    break;
                }
                catch (SharpDXException)
                {
                    if (i == retries - 1)
                    {
                        throw;
                    }
                    Debug.WriteLine("MediaFactory.GetService failed({0}) sleeping for {1} ms", i + 1, i * sleepTimeFactor);
                    Thread.Sleep(i * sleepTimeFactor); //Sleep for longer and longer times
                }
            }
            return volumeObj;
        }

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
                _session.ClearTopologies();
                _session.Close();
                _volumeController.Dispose();
                _clock.Dispose();
            }

            // Set the new song.
            _session.SetTopology(SessionSetTopologyFlags.Immediate, song.Topology);

            // Get the volume interface.
            _volumeController = CppObject.FromPointer<AudioStreamVolume>(MediaPlayer.GetVolumeObj(_session));
            SetChannelVolumes();

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
            var varStart = new Variant();
            _session.Start(null, varStart);
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
