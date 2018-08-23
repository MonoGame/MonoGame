// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;
using SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Win32;
using System;
using System.Diagnostics;
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

        private InternalState _internalState;
        private MediaSession _session;
        private AudioStreamVolume _volumeController;
        private PresentationClock _clock;
        private const int defaultTimeoutMs = 1000;
        private byte[] _black = null;
#if WINRT
        readonly object _locker = new object();
#endif

        private Guid AudioStreamVolumeGuid;
        private Texture2D _texture;
        private Callback _callback;
        internal MediaSession Session { get { return _session; } }
        private VideoSampleGrabber _sampleGrabber;

        private static readonly Variant PositionCurrent = new Variant();
        private static readonly Variant PositionBeginning = new Variant { ElementType = VariantElementType.Long, Value = 0L };

        private class Callback : IAsyncCallback
        {
            public Callback()
            {
            }

            public void Dispose()
            {
            }

            public IDisposable Shadow { get; set; }

            public void Invoke(AsyncResult asyncResultRef)
            {
                var player = (VideoPlayer)asyncResultRef.State;
                var ev = player.Session.EndGetEvent(asyncResultRef);
                // Trigger an "on Video Ended" event here if needed
                if (ev.TypeInfo == MediaEventTypes.SessionTopologyStatus && ev.Get(EventAttributeKeys.TopologyStatus) == TopologyStatus.Ready)
                    player.OnTopologyReady();
                else if (ev.TypeInfo == MediaEventTypes.SessionStarted)
                    player.OnSessionStarted();
                else if (ev.TypeInfo == MediaEventTypes.SessionStopped)
                    player.OnSessionStopped();
                else if (ev.TypeInfo == MediaEventTypes.SessionClosed)
                    player.OnSessionClosed();
                else if (ev.TypeInfo == MediaEventTypes.SessionPaused)
                    player.OnSessionPaused();
                else if (ev.TypeInfo == MediaEventTypes.EndOfPresentation)
                    player.OnPresentationEnded();

                if (player.Session != null)
                    player.Session.BeginGetEvent(this, player);
            }

            public AsyncCallbackFlags Flags { get; private set; }
            public WorkQueueId WorkQueueId { get; private set; }
        }

        private void PlatformInitialize()
        {
            // The GUID is specified in a GuidAttribute attached to the class
            AudioStreamVolumeGuid = Guid.Parse(((GuidAttribute)typeof(AudioStreamVolume).GetCustomAttributes(typeof(GuidAttribute), false)[0]).Value);

            MediaManagerState.CheckStartup();
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
                _texture = new Texture2D(_graphicsDevice, _currentVideo.Width, _currentVideo.Height, false, SurfaceFormat.Bgr32);
        }

        private Texture2D PlatformGetTexture()
        {
            CreateTexture();

            if (_currentVideo != null && State != MediaState.Stopped && State != MediaState.Paused)
            {
                var texData = _sampleGrabber.TextureData;
                if (texData != null)
                    _texture.SetData(texData);
                else
                {
                    // No texture data was returned, so make sure the texture is set to something.
                    if (_black == null)
                    {
                        _black = new byte[_texture.Width * _texture.Height * SurfaceFormat.Bgr32.GetSize()];
                        Array.Clear(_black, 0, _black.Length);
                    }
                    _texture.SetData(_black);
                }
            }

            return _texture;
        }

        private void PlatformGetState(ref MediaState result)
        {
            switch (_internalState)
            {
                case InternalState.Stopped:
                case InternalState.PresentationEnded:
                case InternalState.WaitingForSessionStop:
                    result = MediaState.Stopped;
                    return;

                case InternalState.Paused:
                case InternalState.WaitingForSessionPaused:
                    result = MediaState.Paused;
                    return;
            }

            result = MediaState.Playing;
        }

        private void PlatformPause()
        {
            _internalState = InternalState.WaitingForSessionPaused;
            _session.Pause();
            WaitForInternalStateChange(InternalState.Paused);
        }

        private void PlatformPlay()
        {
            // Cleanup the last video first.
            if (State != MediaState.Stopped)
            {
                PlatformStop();
            }

            MediaFactory.CreateMediaSession(null, out _session);
            CreateTexture();

            // Create the callback if it hasn't been created yet
            if (_callback == null)
            {
                _callback = new Callback();
            }
            _session.BeginGetEvent(_callback, this);

            // Set the new video.
            _internalState = InternalState.WaitingForSessionStart;
            Topology topology;
            _currentVideo.Open(this, out topology, out _sampleGrabber);
            _session.SetTopology(SessionSetTopologyFlags.Immediate, topology);

            WaitForInternalStateChange(InternalState.Playing);
        }

        private void PlatformResume()
        {
            _internalState = InternalState.WaitingForSessionStart;
            _session.Start(null, PositionCurrent);
            WaitForInternalStateChange(InternalState.Playing);
        }

        private void PlatformStop()
        {
            if (State != MediaState.Stopped)
            {
                _internalState = InternalState.WaitingForSessionStop;
                _session.Stop();
            }
            WaitForInternalStateChange(InternalState.Stopped);
        }

        bool WaitForInternalStateChange(InternalState expectedState, int milliseconds = defaultTimeoutMs)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            while (_internalState != expectedState)
            {
#if WINRT
                lock (_locker)
                    System.Threading.Monitor.Wait(_locker, 1);
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
                PlatformStop();
            }
        }

        private void OnTopologyReady()
        {
            if (_session.IsDisposed)
                return;

            // Get the volume interface. Returns null if the video has no audio tracks.
            IntPtr volumeObj;
            try
            {
                MediaFactory.GetService(_session, MediaServiceKeys.StreamVolume, AudioStreamVolumeGuid, out volumeObj);
                _volumeController = CppObject.FromPointer<AudioStreamVolume>(volumeObj);
            }
            catch (SharpDXException ex)
            {
                unchecked
                {
                    if (ex.HResult != (int)0x80004002) // E_NOINTERFACE
                        throw;
                }
            }

            SetChannelVolumes();

            // Get the clock.
            _clock = _session.Clock.QueryInterface<PresentationClock>();

            // Start playing.
            _session.Start(null, PositionBeginning);
        }

        private void OnSessionStarted()
        {
            _internalState = InternalState.Playing;
        }

        private void OnSessionStopped()
        {
            _session.Close();
        }

        private void OnSessionClosed()
        {
            _sampleGrabber = null;
            if (_currentVideo != null)
            {
                _currentVideo.Close(this);
                _currentVideo = null;
            }
            SharpDX.Utilities.Dispose(ref _volumeController);
            SharpDX.Utilities.Dispose(ref _clock);
            _session.ClearTopologies();
            _session.Shutdown();
            SharpDX.Utilities.Dispose(ref _session);
            SharpDX.Utilities.Dispose(ref _texture);
            SharpDX.Utilities.Dispose(ref _callback);
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
