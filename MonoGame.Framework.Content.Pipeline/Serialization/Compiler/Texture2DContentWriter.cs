// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    [ContentTypeWriter]
    public class Texture2DContentWriter : ContentTypeWriter<Texture2DContent>
    {
        protected internal override void Write(ContentWriter output, Texture2DContent value)
        {
            SurfaceFormat format;
            var bmpContent = value.Faces[0][0];
            if (!bmpContent.TryGetFormat(out format))
                throw new Exception("Couldn't get Format for TextureContent.");

            output.Write((int)format);
            output.Write(bmpContent.Width);
            output.Write(bmpContent.Height);

            // TODO: is this correct?
            var mipCount = value.Faces.Count * value.Faces[0].Count;
            output.Write(mipCount); 

            foreach(var chain in value.Faces)
            {
                foreach (var face in chain)
                {
                    var faceData = face.GetPixelData();
                    output.Write(faceData.Length);
                    output.Write(faceData);
                }
            }
        }

        /// <summary>
        /// Gets the assembly qualified name of the runtime loader for this type.
        /// </summary>
        /// <param name="targetPlatform">Name of the platform.</param>
        /// <returns>Name of the runtime loader.</returns>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            // Base the reader type string from a known public class in the same namespace in the same assembly
            Type type = typeof(ContentReader);
            string readerType = type.Namespace + ".Texture2DReader, " + type.Assembly.FullName;
            return readerType;
        }

        /// <summary>
        /// Gets the assembly qualified name of the runtime target type. The runtime target type often matches the design time type, but may differ.
        /// </summary>
        /// <param name="targetPlatform">The target platform.</param>
        /// <returns>The qualified name.</returns>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            // Base the reader type string from a known public class in the same namespace in the same assembly
            Type type = typeof(ContentReader);
            string readerType = type.Namespace + ".Texture2DReader, " + type.AssemblyQualifiedName;
            return readerType;
        }

        /// <summary>
        /// Indicates whether a given type of content should be compressed.
        /// </summary>
        /// <param name="targetPlatform">The target platform of the content build.</param>
        /// <param name="value">The object about to be serialized, or null if a collection of objects is to be serialized.</param>
        /// <returns>true if the content of the requested type should be compressed; false otherwise.</returns>
        /// <remarks>This base class implementation of this method always returns true. It should be overridden
        /// to return false if there would be little or no useful reduction in size of the content type's data
        /// from a general-purpose lossless compression algorithm.
        /// The implementations for Song Class and SoundEffect Class data return false because data for these
        /// content types is already in compressed form.</remarks>
        protected internal override bool ShouldCompressContent(TargetPlatform targetPlatform, object value)
        {
            return false;
        }
    }
}
