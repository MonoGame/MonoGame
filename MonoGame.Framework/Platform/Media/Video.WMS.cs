using SharpDX;
using SharpDX.MediaFoundation;
using System;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Video : IDisposable
    {
        private Topology _topology;
        internal Topology Topology { get { return _topology; } }

        internal VideoSampleGrabber SampleGrabber { get; private set; }

        MediaType _mediaType;

        private void PlatformInitialize()
        {
            if (Topology != null)
                return;

            MediaManagerState.CheckStartup();

            MediaFactory.CreateTopology(out _topology);

            SharpDX.MediaFoundation.MediaSource mediaSource;
            {
                SourceResolver resolver = new SourceResolver();

                ObjectType otype;
                ComObject source = resolver.CreateObjectFromURL(FileName, SourceResolverFlags.MediaSource, null, out otype);
                mediaSource = source.QueryInterface<SharpDX.MediaFoundation.MediaSource>();
                resolver.Dispose();
                source.Dispose();
            }

            PresentationDescriptor presDesc;
            mediaSource.CreatePresentationDescriptor(out presDesc);

            for (var i = 0; i < presDesc.StreamDescriptorCount; i++)
            {
                SharpDX.Mathematics.Interop.RawBool selected;
                StreamDescriptor desc;
                presDesc.GetStreamDescriptorByIndex(i, out selected, out desc);

                if (selected)
                {
                    TopologyNode sourceNode;
                    MediaFactory.CreateTopologyNode(TopologyType.SourceStreamNode, out sourceNode);

                    sourceNode.Set(TopologyNodeAttributeKeys.Source, mediaSource);
                    sourceNode.Set(TopologyNodeAttributeKeys.PresentationDescriptor, presDesc);
                    sourceNode.Set(TopologyNodeAttributeKeys.StreamDescriptor, desc);

                    TopologyNode outputNode;
                    MediaFactory.CreateTopologyNode(TopologyType.OutputNode, out outputNode);

                    var majorType = desc.MediaTypeHandler.MajorType;
                    if (majorType == MediaTypeGuids.Video)
                    {
                        Activate activate;

                        SampleGrabber = new VideoSampleGrabber();

                        _mediaType = new MediaType();

                        _mediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);

                        // Specify that we want the data to come in as RGB32.
                        _mediaType.Set(MediaTypeAttributeKeys.Subtype, new Guid("00000016-0000-0010-8000-00AA00389B71"));

                        MediaFactory.CreateSampleGrabberSinkActivate(_mediaType, SampleGrabber, out activate);
                        outputNode.Object = activate;
                    }

                    if (majorType == MediaTypeGuids.Audio)
                    {
                        Activate activate;
                        MediaFactory.CreateAudioRendererActivate(out activate);

                        outputNode.Object = activate;
                    }

                    _topology.AddNode(sourceNode);
                    _topology.AddNode(outputNode);
                    sourceNode.ConnectOutput(0, outputNode, 0);

                    sourceNode.Dispose();
                    outputNode.Dispose();
                }

                desc.Dispose();
            }

            presDesc.Dispose();
            mediaSource.Dispose();
        }

        private void PlatformDispose(bool disposing)
        {
            if (_topology != null)
            {
                _topology.Dispose();
                _topology = null;
            }

            if (SampleGrabber != null)
            {
                SampleGrabber.Dispose();
                SampleGrabber = null;
            }
        }
    }
}
