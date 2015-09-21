// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Win32;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Media
{
    public static partial class MediaPlayer
    {
        private static MediaSession _session;
        private static AudioStreamVolume _volumeController;
        private static PresentationClock _clock;
        private static Song _nextSong;
        private static Song _currentSong;

        private enum SessionState { Stopped, Stopping, Started, Paused, Ended }
        private static SessionState _sessionState = SessionState.Stopped;

        private static Guid AudioStreamVolumeGuid;

        private static readonly Variant PositionCurrent = new Variant();
        private static readonly Variant PositionBeginning = new Variant { ElementType = VariantElementType.Long, Value = 0L };

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

                switch (ev.TypeInfo)
                {
                    case MediaEventTypes.EndOfPresentation:
                        _sessionState = SessionState.Ended;
                        OnSongFinishedPlaying(null, null);
                        break;
                    case MediaEventTypes.SessionTopologyStatus:
                        if (ev.Get(EventAttributeKeys.TopologyStatus) == TopologyStatus.Ready)
                            OnTopologyReady();
                        break;
                    case MediaEventTypes.SessionStopped:
                        OnSessionStopped();
                        break;
                }

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

            _callback = new Callback();
            _session.BeginGetEvent(_callback, null);

            _clock = _session.Clock.QueryInterface<PresentationClock>();
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
            if ((_sessionState == SessionState.Stopped) || (_sessionState == SessionState.Stopping))
                return TimeSpan.Zero;
            try
            {
                return TimeSpan.FromTicks(_clock.Time);
            }
            catch (SharpDXException)
            {
                // The presentation clock is most likely not quite ready yet
                return TimeSpan.Zero;
            }
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
            if (_volumeController == null)
                return;

            float volume = _isMuted ? 0f : _volume;
            for (int i = 0; i < _volumeController.ChannelCount; i++)
                _volumeController.SetChannelVolume(i, volume);
        }

        private static void PlatformSetVolume(float volume)
        {
            _volume = volume;
            SetChannelVolumes();
        }

        #endregion

        private static void PlatformPause()
        {
            if (_sessionState != SessionState.Started)
                return;
            _sessionState = SessionState.Paused;
            _session.Pause();
        }

        private static void PlatformPlaySong(Song song)
        {
            if (_currentSong == song)
                ReplayCurrentSong(song);
            else
                PlayNewSong(song);
        }

        private static void ReplayCurrentSong(Song song)
        {
            if (_sessionState == SessionState.Stopping)
            {
                // The song will be started after the SessionStopped event is received
                _nextSong = song;
                return;
            }

            StartSession(PositionBeginning);
        }

        private static void PlayNewSong(Song song)
        {
            if (_sessionState != SessionState.Stopped)
            {
                // The session needs to be stopped to reset the play position
                // The new song will be started after the SessionStopped event is received
                _nextSong = song;
                PlatformStop();
                return;
            }

            StartNewSong(song);
        }

        private static void StartNewSong(Song song)
        {
            if (_volumeController != null)
            {
                _volumeController.Dispose();
                _volumeController = null;
            }

            _currentSong = song;

            _session.SetTopology(SessionSetTopologyFlags.Immediate, song.Topology);

            StartSession(PositionBeginning);

            // The volume service won't be available until the session topology
            // is ready, so we now need to wait for the event indicating this
        }

        private static void StartSession(Variant startPosition)
        {
            _sessionState = SessionState.Started;
            _session.Start(null, startPosition);
        }

        private static void OnTopologyReady()
        {
            IntPtr volumeObjectPtr;
            MediaFactory.GetService(_session, MediaServiceKeys.StreamVolume, AudioStreamVolumeGuid, out volumeObjectPtr);
            _volumeController = CppObject.FromPointer<AudioStreamVolume>(volumeObjectPtr);

            SetChannelVolumes();
        }

        private static void PlatformResume()
        {
            if (_sessionState != SessionState.Paused)
                return;
            StartSession(PositionCurrent);
        }

        private static void PlatformStop()
        {
            if ((_sessionState == SessionState.Stopped) || (_sessionState == SessionState.Stopping))
                return;
            bool hasFinishedPlaying = (_sessionState == SessionState.Ended);
            _sessionState = SessionState.Stopping;
            if (hasFinishedPlaying)
            {
                // The play position needs to be reset before stopping otherwise the next song may not start playing
                _session.Start(null, PositionBeginning);
            }
            _session.Stop();
        }

        private static void OnSessionStopped()
        {
            _sessionState = SessionState.Stopped;
            if (_nextSong != null)
            {
                if (_nextSong != _currentSong)
                    StartNewSong(_nextSong);
                else
                    StartSession(PositionBeginning);
                _nextSong = null;
            }
        }
    }
}
