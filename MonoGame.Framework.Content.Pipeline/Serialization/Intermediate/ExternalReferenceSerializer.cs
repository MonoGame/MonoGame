// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class ExternalReferenceSerializer<T> : ContentTypeSerializer<ExternalReference<T>>
    {
        public ExternalReferenceSerializer() :
            base("ExternalReference")
        {
        }

        protected internal override ExternalReference<T> Deserialize(IntermediateReader input, ContentSerializerAttribute format, ExternalReference<T> existingInstance)
        {
            var result = existingInstance ?? new ExternalReference<T>();
            input.ReadExternalReference(result);
            return result;
        }

        protected internal override void Serialize(IntermediateWriter output, ExternalReference<T> value, ContentSerializerAttribute format)
        {
            output.WriteExternalReference(value);
        }
    }
}