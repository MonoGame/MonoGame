// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    class EnumSerializer : ContentTypeSerializer
    {
        public EnumSerializer(Type targetType) :
            base(targetType, targetType.Name)
        {
        }

        protected internal override object Deserialize(IntermediateReader input, ContentSerializerAttribute format, object existingInstance)
        {
            var str = input.Xml.ReadString();
            try
            {
                return Enum.Parse(TargetType, str, true);
            }
            catch (Exception ex)
            {
                throw input.NewInvalidContentException(ex, "Invalid enum value '{0}' for type '{1}'", str, TargetType.Name);
            }
        }

        protected internal override void Serialize(IntermediateWriter output, object value, ContentSerializerAttribute format)
        {
            Debug.Assert(value.GetType() == TargetType, "Got invalid value type!");
            output.Xml.WriteString(value.ToString());
        }
    }
}