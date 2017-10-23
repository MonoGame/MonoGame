using SharpDX;
using SharpDX.MediaFoundation;
using System;
using System.Threading.Tasks;
using SharpDX.Win32;
using System.Threading;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Video : IDisposable
    {
        private static readonly Variant PositionCurrent = new Variant();
        private static readonly Variant PositionBeginning = new Variant { ElementType = VariantElementType.Long, Value = 0L };

        private MediaSession _session;
        private Callback _callback;
        private AudioStreamVolume _volumeController;
        private PresentationClock _clock;

        private bool _waitingToStart;

        private bool _isMuted;
        private float _volume;

        private SharpDX.MediaFoundation.MediaSource _mediaSource;
        private Topology _topology;

        internal VideoSampleGrabber SampleGrabber { get; private set; }

        private class Callback : IAsyncCallback
        {
            private readonly Video _video;

            public Callback(Video video)
            {
                _video = video;
            }

            public void Dispose()
            {
            }

            public IDisposable Shadow { get; set; }
            public void Invoke(AsyncResult asyncResultRef)
            {
                var ev = _video._session.EndGetEvent(asyncResultRef);
                Task.Factory.StartNew(() =>
                {
                    Threading.BlockOnUIThread(() =>
                    {
                        if (_video.ProcessMediaEvent(ev))
                        {
                            _video._session.BeginGetEvent(this, null);
                        }

                        // If we don't dispose the event we leak memory.
                        ev.Dispose();
                    });
                });
            }

            public AsyncCallbackFlags Flags { get; private set; }
            public WorkQueueId WorkQueueId { get; private set; }
        }

        private bool ProcessMediaEvent(MediaEvent ev)
        {
            // Trigger an "on Video Ended" event here if needed
            switch (ev.TypeInfo)
            {
                case MediaEventTypes.SessionTopologyStatus:
                    if (ev.Get(EventAttributeKeys.TopologyStatus) == TopologyStatus.Ready)
                        OnTopologyReady();
                    break;
                case MediaEventTypes.SessionClosed:
                    OnSessionClosed();
                    return false;
            }

            return true;
        }

        private void OnTopologyReady()
        {
            IntPtr volumeObjectPtr;
            MediaFactory.GetService(_session, MediaServiceKeys.StreamVolume, VideoPlayer.AudioStreamVolumeGuid, out volumeObjectPtr);
            _volumeController = CppObject.FromPointer<AudioStreamVolume>(volumeObjectPtr);

            SetChannelVolumes();

            if (_waitingToStart)
            {
                _waitingToStart = false;
                PlatformPlay();
            }
        }

        private void PlatformInitialize()
        {
            if (_topology != null)
                return;

            // Referencing https://msdn.microsoft.com/en-us/library/windows/desktop/ms703190(v=vs.85).aspx
            // 1. Call the MFStartup function to initialise the Media Foundation platform (if needed)
            MediaManagerState.CheckStartup();

            // 2. Call MFCreateMediaSession to create a new instance of the Media Session.
            MediaFactory.CreateMediaSession(null, out _session);

            // 3. Use the source resolver to create a media source.
            SourceResolver resolver = new SourceResolver();

            ComObject source = resolver.CreateObjectFromURL(FileName, SourceResolverFlags.MediaSource);
            _mediaSource = source.QueryInterface<SharpDX.MediaFoundation.MediaSource>();
            resolver.Dispose();
            source.Dispose();

            PresentationDescriptor presDesc;
            _mediaSource.CreatePresentationDescriptor(out presDesc);

            // 4. Create a topology that connects the media source to the EVR and SAR.
            MediaFactory.CreateTopology(out _topology);
            for (var i = 0; i < presDesc.StreamDescriptorCount; i++)
            {
                SharpDX.Mathematics.Interop.RawBool selected;
                StreamDescriptor desc;
                presDesc.GetStreamDescriptorByIndex(i, out selected, out desc);

                if (selected)
                {
                    Activate activate = null;
                    var typeHandler = desc.MediaTypeHandler;
                    var majorType = typeHandler.MajorType;
                    if (majorType == MediaTypeGuids.Video)
                    {
                        SampleGrabber = new VideoSampleGrabber();

                        var mediaType = new MediaType();

                        mediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);

                        // Specify that we want the data to come in as RGB32.
                        mediaType.Set(MediaTypeAttributeKeys.Subtype, new Guid("00000016-0000-0010-8000-00AA00389B71"));

                        MediaFactory.CreateSampleGrabberSinkActivate(mediaType, SampleGrabber, out activate);

                        mediaType.Dispose();
                    }
                    else if (majorType == MediaTypeGuids.Audio)
                    {
                        MediaFactory.CreateAudioRendererActivate(out activate);
                    }

                    typeHandler.Dispose();
                    if (activate == null)
                    {
                        presDesc.DeselectStream(i);
                        continue;
                    }

                    TopologyNode sourceNode;
                    MediaFactory.CreateTopologyNode(TopologyType.SourceStreamNode, out sourceNode);

                    sourceNode.Set(TopologyNodeAttributeKeys.Source, _mediaSource);
                    sourceNode.Set(TopologyNodeAttributeKeys.PresentationDescriptor, presDesc);
                    sourceNode.Set(TopologyNodeAttributeKeys.StreamDescriptor, desc);

                    TopologyNode outputNode;
                    MediaFactory.CreateTopologyNode(TopologyType.OutputNode, out outputNode);
                    outputNode.Object = activate;

                    _topology.AddNode(sourceNode);
                    _topology.AddNode(outputNode);
                    sourceNode.ConnectOutput(0, outputNode, 0);

                    sourceNode.Dispose();
                    outputNode.Dispose();
                    activate.Dispose();
                }

                desc.Dispose();
            }

            presDesc.Dispose();

            // 5. Call IMFMediaSession::SetTopology to set the topology on the Media Session.
            _session.SetTopology(SessionSetTopologyFlags.Immediate, _topology);

            _clock = _session.Clock.QueryInterface<PresentationClock>();

            // 6. Use the IMFMMediaEventGenerator interface to get events from the Media Session.
            _callback = new Callback(this);
            _session.BeginGetEvent(_callback, null);

            // 7. We can now call play, pause, stop methods on the session to play the video.
        }

        private void SetChannelVolumes()
        {
            if (_volumeController == null)
                return;

            float volume = _isMuted ? 0f : _volume;
            for (int i = 0; i < _volumeController.ChannelCount; i++)
                _volumeController.SetChannelVolume(i, volume);
        }

        private void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
                // Referencing https://msdn.microsoft.com/en-us/library/windows/desktop/ms703190(v=vs.85).aspx
                // 8a. We must close the session and dispose its resources after the close completes asynchronously.
                if (_session != null)
                {
                    _session.Close();
                }
            }
        }

        internal void SetVolume(bool isMuted, float volume)
        {
            _isMuted = isMuted;
            _volume = volume;

            if (_volumeController == null)
                return;

            SetChannelVolumes();
        }


        internal void PlatformPlay()
        {
            if (_session == null || _volumeController == null)
            {
                _waitingToStart = true;
                return;
            }

            _session.Start(null, PositionBeginning);
        }

        internal void PlatformPause()
        {
            if (_waitingToStart)
            {
                _waitingToStart = false;
                return;
            }

            _session.Pause();
        }

        internal void PlatformResume()
        {
            if (_session == null || _volumeController == null)
            {
                _waitingToStart = true;
                return;
            }

            _session.Start(null, PositionCurrent);
        }

        internal void PlatformStop()
        {
            if (_waitingToStart)
            {
                _waitingToStart = false;
                return;
            }

            _session.Stop();
        }

        internal void PlatformGetState(out MediaState result)
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

        internal TimeSpan PlatformGetPlayPosition()
        {
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

        private void OnSessionClosed()
        {
            if (_callback != null)
            {
                _callback.Dispose();
                _callback = null;
            }

            if (_volumeController != null)
            {
                _volumeController.Dispose();
                _volumeController = null;
            }

            if (_clock != null)
            {
                _clock.Dispose();
                _clock = null;
            }

            // Referencing https://msdn.microsoft.com/en-us/library/windows/desktop/ms703190(v=vs.85).aspx
            // 8b. Call IMFMediaSource::Shutdown to shut down the media source.
            if (_mediaSource != null)
            {
                _mediaSource.Shutdown();
            }

            // 8c. Call IMFMediaSession::Shutdown to shut down the media session.
            if (_session != null)
            {
                _session.Shutdown();
                _session.Dispose();
                _session = null;
            }

            // We don't do 8d, shut down the media platform, here because we might have other media ongoing.

            DisposeTopology();
            DisposeSampleGrabber();
        }

        internal void DisposeTopology()
        {
            if (_topology != null)
            {
                _topology.Clear();
                _topology.Dispose();
                _topology = null;
            }

            if (_mediaSource != null)
            {
                _mediaSource.Dispose();
                _mediaSource = null;
            }
        }

        internal void DisposeSampleGrabber()
        {
            if (SampleGrabber != null)
            {
                SampleGrabber.Dispose();
                SampleGrabber = null;
            }
        }
    }
}
