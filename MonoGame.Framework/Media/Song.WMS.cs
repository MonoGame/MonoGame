// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using SharpDX;
using SharpDX.MediaFoundation;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : IEquatable<Song>, IDisposable
    {
        private Topology _topology;

        internal Topology Topology { get { return _topology; } }

        private void PlatformInitialize(string fileName)
        {
            if (_topology != null)
                return;

            MediaManagerState.CheckStartup();

            MediaFactory.CreateTopology(out _topology);

            SharpDX.MediaFoundation.MediaSource mediaSource;
            {
                SourceResolver resolver;
                MediaFactory.CreateSourceResolver(out resolver);

                ObjectType otype;
                ComObject source;
                resolver.CreateObjectFromURL(FilePath, (int)SourceResolverFlags.MediaSource, null, out otype,
                                                out source);
                mediaSource = source.QueryInterface<SharpDX.MediaFoundation.MediaSource>();
                resolver.Dispose();
                source.Dispose();
            }

            PresentationDescriptor presDesc;
            mediaSource.CreatePresentationDescriptor(out presDesc);

            for (var i = 0; i < presDesc.StreamDescriptorCount; i++)
            {
                Bool selected;
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
                    if (majorType != MediaTypeGuids.Audio)
                        throw new NotSupportedException("The song contains video data!");

                    Activate activate;
                    MediaFactory.CreateAudioRendererActivate(out activate);
                    outputNode.Object = activate;

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
        }
    }
}

