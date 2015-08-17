// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using SharpDX.MediaFoundation;
using SharpDX.Win32;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Media
{
    public static partial class MediaPlayer
    {
        //RAYB: This probably needs to get flipped back into a readonly.
        private static MediaSession _session;
        private static AudioStreamVolume _volumeController;
        private static PresentationClock _clock;

        private static Guid AudioStreamVolumeGuid;

        private static MediaSessionCallback _callback;

        private static void OnMediaSessionEvent(MediaEvent ev)
        {
            switch (ev.TypeInfo)
            {
                case MediaEventTypes.EndOfPresentation:
                    OnSongFinishedPlaying(null, null);
                    break;
                case MediaEventTypes.SessionTopologyStatus:
                    if (ev.Get(EventAttributeKeys.TopologyStatus) == TopologyStatus.Ready)
                        OnTopologyReady();
                    break;
            }
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
            if ((_volumeController == null) || _volumeController.IsDisposed)
                return;

            var volume = IsMuted ? 0f : _volume;
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

            //create the callback
            _callback = new MediaSessionCallback(_session, OnMediaSessionEvent);

            // Set the new song.
            _session.SetTopology(SessionSetTopologyFlags.Immediate, song.Topology);
        }

        private static void OnTopologyReady()
        {
            if (_session.IsDisposed)
                return;

            // Get the volume interface.
            IntPtr volumeObjectPtr;
            MediaFactory.GetService(_session, MediaServiceKeys.StreamVolume, AudioStreamVolumeGuid, out volumeObjectPtr);
            _volumeController = new AudioStreamVolume(volumeObjectPtr);

            SetChannelVolumes();

            // Get the clock.
            _clock = _session.Clock.QueryInterface<PresentationClock>();

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
