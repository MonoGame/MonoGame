using System;
using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class CurveKeyCollectionSerializer : ContentTypeSerializer<CurveKeyCollection>
    {
        public CurveKeyCollectionSerializer() :
            base("Keys")
        { }

        public override bool CanDeserializeIntoExistingObject
        { get { return true; } }

        protected internal override CurveKeyCollection Deserialize(
            IntermediateReader input,
            ContentSerializerAttribute format,
            CurveKeyCollection existingInstance)
        {
            var result = existingInstance ?? new CurveKeyCollection();

            if (input.Xml.HasValue)
            {
                var elements = PackedElementsHelper.ReadElements(input);
                if (elements.Length > 0)
                {
                    // Each CurveKey consists of 5 elements
                    if (elements.Length % 5 != 0)
                        throw new InvalidContentException(
                            "Elements count in CurveKeyCollection is inncorect!");
                    try
                    {
                        // Parse all CurveKeys
                        for (int i = 0; i < elements.Length; i += 5)
                        {
                            // Order: Position, Value, TangentIn, TangentOut and Continuity
                            var curveKey = new CurveKey
                                (XmlConvert.ToSingle(elements[i]),
                                 XmlConvert.ToSingle(elements[i + 1]),
                                 XmlConvert.ToSingle(elements[i + 2]),
                                 XmlConvert.ToSingle(elements[i + 3]),
                                 (CurveContinuity)Enum.Parse(
                                     typeof(CurveContinuity),
                                     elements[i + 4],
                                     true));
                            result.Add(curveKey);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new InvalidContentException
                            ("Error parsing CurveKey", e);
                    }
                }
            }
            return result;
        }


        protected internal override void Serialize(
            IntermediateWriter output,
            CurveKeyCollection value,
            ContentSerializerAttribute format)
        {
            var elements = new List<string>();
            foreach (var curveKey in value)
            {
                // Order: Position, Value, TangentIn, TangentOut and Continuity
                elements.Add(XmlConvert.ToString(curveKey.Position));
                elements.Add(XmlConvert.ToString(curveKey.Value));
                elements.Add(XmlConvert.ToString(curveKey.TangentIn));
                elements.Add(XmlConvert.ToString(curveKey.TangentOut));
                elements.Add(curveKey.Continuity.ToString());
            }
            var str = PackedElementsHelper.JoinElements(elements);
            output.Xml.WriteString(str);
        }
    }
}