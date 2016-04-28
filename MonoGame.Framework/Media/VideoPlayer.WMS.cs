// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;
using SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Win32;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class VideoPlayer : IDisposable
    {
        enum InternalState
        {
            Stopped,
            WaitingForSessionStart,
            Playing,
            WaitingForSessionPaused,
            Paused,
            WaitingForSessionStop,
            PresentationEnded,
        }

        InternalState _internalState;
        private MediaSession _session;
        private AudioStreamVolume _volumeController;
        private PresentationClock _clock;
        const int defaultTimeoutMs = 1000;

        // HACK: Need SharpDX to fix this.
        private Guid AudioStreamVolumeGuid;
        private Texture2D _texture;
        private Callback _callback;
        private static Texture2D _texture;
        internal MediaSession Session { get { return _session; } }

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
                var ev = _player.Session.EndGetEvent(asyncResultRef);

                // Trigger an "on Video Ended" event here if needed
                if (ev.TypeInfo == MediaEventTypes.SessionTopologyStatus && ev.Get(EventAttributeKeys.TopologyStatus) == TopologyStatus.Ready)
                    _player.OnTopologyReady();
                else if (ev.TypeInfo == MediaEventTypes.SessionStarted)
                    _player.OnSessionStarted();
                else if (ev.TypeInfo == MediaEventTypes.SessionStopped)
                    _player.OnSessionStopped();
                else if (ev.TypeInfo == MediaEventTypes.SessionClosed)
                    _player.OnSessionClosed();
                else if (ev.TypeInfo == MediaEventTypes.SessionPaused)
                    _player.OnSessionPaused();
                else if (ev.TypeInfo == MediaEventTypes.EndOfPresentation)
                    _player.OnPresentationEnded();

                _player.Session.BeginGetEvent(this, null);
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
                else
                {
                    // No texture data was returned, so make sure the texture is set to something.
                    var black = new byte[_texture.Width * _texture.Height * SurfaceFormat.Bgr32.GetSize()];
                    Array.Clear(black, 0, black.Length);
                    _texture.SetData(black);
                }
            }

            return _texture;
        }

        private void PlatformGetState(ref MediaState result)
        {
            switch (_internalState)
            {
                case InternalState.Playing:
                    result = MediaState.Playing;
                    return;

                case InternalState.Paused:
                    result = MediaState.Paused;
                    return;
            }

            result = MediaState.Stopped;
        }

        private void PlatformPause()
        {
            _internalState = InternalState.WaitingForSessionPaused;
            _session.Pause();
            WaitForInternalStateChange(InternalState.Paused);
        }

        private void PlatformPlay()
        {
            System.Diagnostics.Debug.WriteLine("PlatformPlay");
            // Cleanup the last video first.
            if (State != MediaState.Stopped)
            {
                PlatformStop();
            }

            CreateTexture();

            // Create the callback if it hasn't been created yet
            if (_callback == null)
            {
                _callback = new Callback(this);
                _session.BeginGetEvent(_callback, null);
            }

            // Set the new video.
            _internalState = InternalState.WaitingForSessionStart;
            _session.SetTopology(SessionSetTopologyFlags.Immediate, _currentVideo.Topology);

            WaitForInternalStateChange(InternalState.Playing);
        }

        private void PlatformResume()
        {
            _internalState = InternalState.WaitingForSessionStart;
            _session.Start(null, null);
            WaitForInternalStateChange(InternalState.Playing);
        }

        private void PlatformStop()
        {
            System.Diagnostics.Debug.WriteLine("PlatformStop");
            if (State == MediaState.Playing)
            {
                _internalState = InternalState.WaitingForSessionStop;
                _session.Stop();
                WaitForInternalStateChange(InternalState.Stopped);
            }
            else
            {
                _internalState = InternalState.Stopped;
            }
            System.Diagnostics.Debug.WriteLine("PlatformStopped");
        }

        bool WaitForInternalStateChange(InternalState expectedState, int milliseconds = defaultTimeoutMs)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            while (_internalState != expectedState)
            {
#if WINRT
                Task.Delay(1).Wait();
#else
                Thread.Sleep(1);
#endif
                if (timer.ElapsedMilliseconds > milliseconds)
                    return false;
            }
            return true;
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
                SharpDX.Utilities.Dispose(ref _volumeController);
                SharpDX.Utilities.Dispose(ref _clock);
                SharpDX.Utilities.Dispose(ref _session);
                SharpDX.Utilities.Dispose(ref _texture);
                SharpDX.Utilities.Dispose(ref _callback);
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

            // Get the clock.
            _clock = _session.Clock.QueryInterface<PresentationClock>();

            // Start playing.
            var varStart = new Variant();
            _session.Start(null, varStart);
        }

        private void OnSessionStarted()
        {
            _internalState = InternalState.Playing;
        }

        private void OnSessionStopped()
        {
            System.Diagnostics.Debug.WriteLine("OnSessionStopped");
            _session.Close();
        }
 
        private void OnSessionClosed()
        {
            System.Diagnostics.Debug.WriteLine("OnSessionClosed");
            if (_volumeController != null)
            {
                _volumeController.Dispose();
                _volumeController = null;
            }
            _clock.Dispose();
            _clock = null;
            _internalState = InternalState.Stopped;
        }

        private void OnSessionPaused()
        {
            _internalState = InternalState.Paused;
        }

        private void OnPresentationEnded()
        {
            if (_isLooped)
            {
                var varStart = new Variant();
                _session.Start(null, varStart);
                WaitForInternalStateChange(InternalState.Playing);
            }
            else
            {
                _internalState = InternalState.PresentationEnded;
                Stop();
            }
        }
    }
}
