// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Model - MonoGame")]
    public class ModelProcessor : ContentProcessor<NodeContent, ModelContent>
    {
        private bool _premultiplyTextureAlpha = true;
        private bool _premultiplyVertexColors = true;

        public ModelProcessor() { }

        public virtual Color ColorKeyColor { get; set; }

        public virtual bool ColorKeyEnabled { get; set; }

        public virtual MaterialProcessorDefaultEffect DefaultEffect { get; set; }

        public virtual bool GenerateMipmaps { get; set; }

        public virtual bool GenerateTangentFrames { get; set; }

        [DefaultValue(true)]
        public virtual bool PremultiplyTextureAlpha
        {
            get { return _premultiplyTextureAlpha; }
            set { _premultiplyTextureAlpha = value; }
        }

        [DefaultValue(true)]
        public virtual bool PremultiplyVertexColors
        {
            get { return _premultiplyVertexColors; }
            set { _premultiplyVertexColors = value; }
        }

        public virtual bool ResizeTexturesToPowerOfTwo { get; set; }

        public virtual float RotationX { get; set; }

        public virtual float RotationY { get; set; }

        public virtual float RotationZ { get; set; }

        public virtual float Scale { get; set; }

        public virtual bool SwapWindingOrder { get; set; }

        public virtual TextureProcessorOutputFormat TextureFormat { get; set; }

        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            throw new NotImplementedException();
        }

        protected virtual MaterialContent ConvertMaterial(MaterialContent material, ContentProcessorContext context)
        {
            throw new NotImplementedException();
        }

        protected virtual void ProcessGeometryUsingMaterial(MaterialContent material,
                                                            IEnumerable<GeometryContent> geometryCollection,
                                                            ContentProcessorContext context)
        {
            throw new NotImplementedException();
        }

        protected virtual void ProcessVertexChannel(GeometryContent content,
                                                    int vertexChannelIndex,
                                                    ContentProcessorContext context)
        {
            throw new NotImplementedException();
        }
    }
}
