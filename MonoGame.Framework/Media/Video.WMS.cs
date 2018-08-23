// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using SharpDX;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Video : IDisposable
    {
        class State
        {
            internal VideoPlayer _videoPlayer;
            internal Topology _topology;
            internal VideoSampleGrabber _sampleGrabber;
            internal MediaType _mediaType;
            internal SharpDX.MediaFoundation.MediaSource _mediaSource;
            internal PresentationDescriptor _presDesc;
            internal Activate _activate;
        }

        List<State> _states = new List<State>();

        private void PlatformInitialize()
        {
        }

        State GetState(VideoPlayer videoPlayer)
        {
            foreach (var state in _states)
            {
                if (ReferenceEquals(state._videoPlayer, videoPlayer))
                {
                    return state;
                }
            }
            return null;
        }

        internal void Open(VideoPlayer videoPlayer, out Topology topology, out VideoSampleGrabber sampleGrabber)
        {
            var state = new State();
            state._videoPlayer = videoPlayer;
            _states.Add(state);

            MediaFactory.CreateTopology(out state._topology);

            using (var resolver = new SourceResolver())
            {
                using (ComObject source = resolver.CreateObjectFromURL(FileName, SourceResolverFlags.MediaSource))
                {
                    state._mediaSource = source.QueryInterface<SharpDX.MediaFoundation.MediaSource>();
                }
            }

            state._mediaSource.CreatePresentationDescriptor(out state._presDesc);

            for (var i = 0; i < state._presDesc.StreamDescriptorCount; i++)
            {
                SharpDX.Mathematics.Interop.RawBool selected;
                StreamDescriptor desc;
                state._presDesc.GetStreamDescriptorByIndex(i, out selected, out desc);

                if (selected)
                {
                    TopologyNode sourceNode;
                    MediaFactory.CreateTopologyNode(TopologyType.SourceStreamNode, out sourceNode);

                    sourceNode.Set(TopologyNodeAttributeKeys.Source, state._mediaSource);
                    sourceNode.Set(TopologyNodeAttributeKeys.PresentationDescriptor, state._presDesc);
                    sourceNode.Set(TopologyNodeAttributeKeys.StreamDescriptor, desc);

                    TopologyNode outputNode;
                    MediaFactory.CreateTopologyNode(TopologyType.OutputNode, out outputNode);

                    var majorType = desc.MediaTypeHandler.MajorType;
                    if (majorType == MediaTypeGuids.Video)
                    {
                        state._sampleGrabber = new VideoSampleGrabber();

                        state._mediaType = new MediaType();
                        state._mediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                        // Specify that we want the data to come in as RGB32.
                        state._mediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.Rgb32);

                        MediaFactory.CreateSampleGrabberSinkActivate(state._mediaType, state._sampleGrabber, out state._activate);
                        outputNode.Object = state._activate;
                    }

                    if (majorType == MediaTypeGuids.Audio)
                    {
                        MediaFactory.CreateAudioRendererActivate(out state._activate);

                        outputNode.Object = state._activate;
                    }

                    state._topology.AddNode(sourceNode);
                    state._topology.AddNode(outputNode);
                    sourceNode.ConnectOutput(0, outputNode, 0);
                }

                desc.Dispose();
            }

            topology = state._topology;
            sampleGrabber = state._sampleGrabber;
        }

        internal void Close(VideoPlayer videoPlayer)
        {
            var state = GetState(videoPlayer);
            if (state != null)
            {
                if (state._topology != null)
                {
                    // Disconnect the node outputs
                    for (int i = state._topology.NodeCount - 1; i >= 0; --i)
                    {
                        TopologyNode node;
                        state._topology.GetNode((short)i, out node);
                        if (node.OutputCount > 0)
                            node.DisconnectOutput(0);
                    }
                    // Dispose the nodes
                    for (int i = state._topology.NodeCount - 1; i >= 0; --i)
                    {
                        TopologyNode node;
                        state._topology.GetNode((short)i, out node);
                        state._topology.RemoveNode(node);
                        node.Dispose();
                    }
                }

                SharpDX.Utilities.Dispose(ref state._topology);
                SharpDX.Utilities.Dispose(ref state._sampleGrabber);
                SharpDX.Utilities.Dispose(ref state._presDesc);
                state._mediaSource.Shutdown();
                SharpDX.Utilities.Dispose(ref state._mediaSource);
                MediaFactory.ShutdownObject(state._activate);
                SharpDX.Utilities.Dispose(ref state._activate);
                state._mediaType = null;
                _states.Remove(state);
            }
        }

        private void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
                for (int i = _states.Count - 1; i >= 0; --i)
                {
                    var state = _states[i];
                    state._videoPlayer.Stop();
                }
            }
        }
    }
}
