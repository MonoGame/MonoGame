using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Win32;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class VideoPlayer : IDisposable
    {
        private static MediaSession _session;
        private static AudioStreamVolume _volumeController;
        private static PresentationClock _clock;
        private static Video _newSessionVideo;
        private static Video _currentSessionVideo;

        // HACK: Need SharpDX to fix this.
        private static Guid AudioStreamVolumeGuid;

        private static readonly Variant PositionCurrent = new Variant();
        private static readonly Variant PositionBeginning = new Variant { ElementType = VariantElementType.Long, Value = 0L };

        private static TaskScheduler _uiTaskScheduler;
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

                var task = Task.Factory.StartNew(() => _player.OnMediaSessionEvent(ev),
                    CancellationToken.None, TaskCreationOptions.None, _uiTaskScheduler);
                task.Wait();

                _session.BeginGetEvent(this, null);
            }

            public AsyncCallbackFlags Flags { get; private set; }
            public WorkQueueId WorkQueueId { get; private set; }
        }

        private void OnMediaSessionEvent(MediaEvent ev)
        {
            // Trigger an "on Video Ended" event here if needed
            switch (ev.TypeInfo)
            {
                case MediaEventTypes.SessionTopologyStatus:
                    if (ev.Get(EventAttributeKeys.TopologyStatus) == TopologyStatus.Ready)
                        OnTopologyReady();
                    break;
                case MediaEventTypes.SessionStopped:
                    OnSessionStopped();
                    break;
            }
        }

        private void PlatformInitialize()
        {
            // The GUID is specified in a GuidAttribute attached to the class
            AudioStreamVolumeGuid = Guid.Parse(((GuidAttribute)typeof(AudioStreamVolume).GetCustomAttributes(typeof(GuidAttribute), false)[0]).Value);

            MediaManagerState.CheckStartup();
            MediaFactory.CreateMediaSession(null, out _session);

            //create the callback if it hasn't been created yet
            if (_callback == null)
            {
                _uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                _callback = new Callback(this);
                _session.BeginGetEvent(_callback, null);
            }
        }

        private Texture2D PlatformGetTexture()
        {
            var sampleGrabber = _currentVideo.SampleGrabber;

            var texData = sampleGrabber.TextureData;

            if (texData == null)
                return null;

            // TODO: This could likely be optimized if we held on to the SharpDX Surface/Texture data,
            // and set it on an XNA one rather than constructing a new one every time this is called.
            var retTex = new Texture2D(Game.Instance.GraphicsDevice, _currentVideo.Width, _currentVideo.Height, false, SurfaceFormat.Bgr32);
            
            retTex.SetData(texData);
            
            return retTex;
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
            if (_currentSessionVideo == _currentVideo)
            {
                _session.Start(null, PositionBeginning);
                return;
            }

            if (State != MediaState.Stopped)
            {
                // The session needs to be stopped to reset the play position
                // The new video will be started after the SessionStopped event is received
                _newSessionVideo = _currentVideo;
                _session.Stop();
                return;
            }

            StartVideo(_currentVideo);
        }

        private void StartVideo(Video video)
        {
            if (_volumeController != null)
            {
                _volumeController.Dispose();
                _volumeController = null;
            }

            // Set the new video.
            _currentSessionVideo = video;

            _session.SetTopology(SessionSetTopologyFlags.Immediate, _currentVideo.Topology);

            _session.Start(null, PositionBeginning);

            // The volume service won't be available until the session topology
            // is ready, so we now need to wait for the event indicating this
        }

        private void PlatformResume()
        {
            _session.Start(null, PositionCurrent);
        }

        private void PlatformStop()
        {
            _session.Stop();
        }

        private void SetChannelVolumes()
        {
            if (_volumeController == null)
                return;

            float volume = _isMuted ? 0f : _volume;
            for (int i = 0; i < _volumeController.ChannelCount; i++)
                _volumeController.SetChannelVolume(i, volume);
        }

        private void PlatformSetVolume()
        {
            if (_volumeController == null)
                return;

            SetChannelVolumes();
        }

        private void PlatformSetIsLooped()
        {
            throw new NotImplementedException();
        }

        private void PlatformSetIsMuted()
        {
            if (_volumeController == null)
                return;

            SetChannelVolumes();
        }

        private TimeSpan PlatformGetPlayPosition()
        {
            if (State == MediaState.Stopped)
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

        private void PlatformDispose(bool disposing)
        {
        }

        private void OnTopologyReady()
        {
            IntPtr volumeObjectPtr;
            MediaFactory.GetService(_session, MediaServiceKeys.StreamVolume, AudioStreamVolumeGuid, out volumeObjectPtr);
            _volumeController = CppObject.FromPointer<AudioStreamVolume>(volumeObjectPtr);

            SetChannelVolumes();
        }

        private void OnSessionStopped()
        {
            if (_newSessionVideo != null)
            {
                StartVideo(_newSessionVideo);
                _newSessionVideo = null;
            }
        }
    }
}
