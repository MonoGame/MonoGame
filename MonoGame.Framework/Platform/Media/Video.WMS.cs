using SharpDX;
using SharpDX.MediaFoundation;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Video : IDisposable
    {
        // Managed CreateSampleGrabberSinkActivate wrapper was made internal, and relying on reflection would be even worse.
        [DllImport("mf.dll", ExactSpelling = true, PreserveSig = false)]
        private static extern void MFCreateSampleGrabberSinkActivate(
            IntPtr pIMFMediaType,
            IntPtr pIMFSampleGrabberSinkCallback,
            out IntPtr ppIActivate);

        private Topology _topology;
        internal Topology Topology { get { return _topology; } }

        internal VideoSampleGrabber SampleGrabber { get; private set; }

        private MediaType _mediaType;

        private void PlatformInitialize()
        {
            if (Topology != null)
                return;

            MediaManagerState.CheckStartup();

            MediaFactory.CreateTopology(out _topology);

            SharpDX.MediaFoundation.MediaSource mediaSource;
            {
                using SourceResolver resolver = new SourceResolver();

                SharpDX.IUnknown source = resolver.CreateObjectFromURL(FileName,
                    SourceResolverFlags.MediaSource,
                    null,
                    out ObjectType objectType);
                if (objectType != ObjectType.MediaSource)
                {
                    throw new NotSupportedException($"{FileName} is not a media source.");
                }

                try
                {
                    mediaSource = SharpDX.ComObject.As<SharpDX.MediaFoundation.MediaSource>(source);
                }
                finally
                {
                    if (source is IDisposable disposableSource)
                    {
                        disposableSource.Dispose();
                    }
                }
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

                        IntPtr pCallback = CppObject.ToCallbackPtr<SampleGrabberSinkCallback>(SampleGrabber);

                        MFCreateSampleGrabberSinkActivate(_mediaType.NativePointer, pCallback, out IntPtr pActivate);
                        
                        activate = new Activate(pActivate);

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
