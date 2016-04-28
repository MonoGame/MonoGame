// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;
using SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Win32;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class VideoPlayer : IDisposable
    {
        private static MediaSession _session;
        private static AudioStreamVolume _volumeController;
        private static PresentationClock _clock;

        // HACK: Need SharpDX to fix this.
        private static Guid AudioStreamVolumeGuid;
        private static Texture2D _texture;
        private static Callback _callback;

        private class Callback : IAsyncCallback
        {
            private VideoPlayer _player;

            public Callback(VideoPlayer player)
            {
                _player = player;
            }

            public void Dispose()
            {
            }

            public IDisposable Shadow { get; set; }

            public void Invoke(AsyncResult asyncResultRef)
            {
                var ev = _session.EndGetEvent(asyncResultRef);

                // Trigger an "on Video Ended" event here if needed

                if (ev.TypeInfo == MediaEventTypes.SessionTopologyStatus && ev.Get(EventAttributeKeys.TopologyStatus) == TopologyStatus.Ready)
                    _player.OnTopologyReady();
                else if (ev.TypeInfo == MediaEventTypes.EndOfPresentation)
                    _player.OnPresentationEnded();

                _session.BeginGetEvent(this, null);
            }

            public AsyncCallbackFlags Flags { get; private set; }
            public WorkQueueId WorkQueueId { get; private set; }
        }

        private void PlatformInitialize()
        {
            // The GUID is specified in a GuidAttribute attached to the class
            AudioStreamVolumeGuid = Guid.Parse(((GuidAttribute)typeof(AudioStreamVolume).GetCustomAttributes(typeof(GuidAttribute), false)[0]).Value);

            MediaManagerState.CheckStartup();
            MediaFactory.CreateMediaSession(null, out _session);
        }

        private void CreateTexture()
        {
            if (_currentVideo == null)
                return;

            if (_texture != null)
            {
                // If the new video is different in size to the previous video, dispose of the current texture
                if (_texture.Width != _currentVideo.Width || _texture.Height != _currentVideo.Height)
                {
                    _texture.Dispose();
                    _texture = null;
                }
            }

            if (_texture == null)
                _texture = new Texture2D(Game.Instance.GraphicsDevice, _currentVideo.Width, _currentVideo.Height, false, SurfaceFormat.Bgr32);
        }

        private Texture2D PlatformGetTexture()
        {
            CreateTexture();

            if (_currentVideo != null && State != MediaState.Stopped)
            {
                var sampleGrabber = _currentVideo.SampleGrabber;
                var texData = sampleGrabber.TextureData;
                if (texData != null)
                    _texture.SetData(texData);
            }

            return _texture;
        }

        private void PlatformGetState(ref MediaState result)
        {
            if (_clock != null)
            {
                ClockState state;
                _clock.GetState(0, out state);

                switch (state)
                {
                    case ClockState.Running:
                        result = MediaState.Playing;
                        return;

                    case ClockState.Paused:
                        result = MediaState.Paused;
                        return;
                }
            }

            result = MediaState.Stopped;
        }

        private void PlatformPause()
        {
            _session.Pause();
        }

        private void PlatformPlay()
        {
            // Cleanup the last song first.
            if (State != MediaState.Stopped)
            {
                _session.Stop();
                _session.ClearTopologies();
                _session.Close();
                if (_volumeController != null)
                {
                    _volumeController.Dispose();
                    _volumeController = null;
                }
                _clock.Dispose();
            }

            CreateTexture();

            //create the callback if it hasn't been created yet
            if (_callback == null)
            {
                _callback = new Callback(this);
                _session.BeginGetEvent(_callback, null);
            }

            // Set the new song.
            _session.SetTopology(SessionSetTopologyFlags.Immediate, _currentVideo.Topology);

            // Get the clock.
            _clock = _session.Clock.QueryInterface<PresentationClock>();

            // Start playing.
            var varStart = new Variant();
            _session.Start(null, varStart);
        }

        private void PlatformResume()
        {
            _session.Start(null, null);
        }

        private void PlatformStop()
        {
            _session.ClearTopologies();
            _session.Stop();
            _session.Close();
            if (_volumeController != null)
            {
                _volumeController.Dispose();
                _volumeController = null;
            }
            _clock.Dispose();
            _clock = null;
        }

        private void SetChannelVolumes()
        {
            if (_volumeController != null && !_volumeController.IsDisposed)
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

        private void PlatformSetVolume()
        {
            if (_volumeController == null)
                return;

            SetChannelVolumes();
        }

        private void PlatformSetIsLooped()
        {
        }

        private void PlatformSetIsMuted()
        {
            if (_volumeController == null)
                return;

            SetChannelVolumes();
        }

        private TimeSpan PlatformGetPlayPosition()
        {
            return TimeSpan.FromTicks(_clock.Time);
        }

        private void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
                if (_texture != null)
                {
                    _texture.Dispose();
                    _texture = null;
                }
            }
        }

        private void OnTopologyReady()
        {
            if (_session.IsDisposed)
                return;

            // Get the volume interface.
            IntPtr volumeObjectPtr;
            MediaFactory.GetService(_session, MediaServiceKeys.StreamVolume, AudioStreamVolumeGuid, out volumeObjectPtr);
            _volumeController = CppObject.FromPointer<AudioStreamVolume>(volumeObjectPtr);

            SetChannelVolumes();
        }

        private void OnPresentationEnded()
        {
            if (_isLooped)
            {
                PlatformPlay();
            }
            else
            {
                Stop();
            }
        }
    }
}
