// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate
{
    [ContentTypeSerializer]
    class MatrixSerializer : ElementSerializer<Matrix>
    {
        public MatrixSerializer() :
            base("Matrix", 16)
        {
        }

        protected internal override Matrix Deserialize(string[] inputs, ref int index)
        {
            return new Matrix(XmlConvert.ToSingle(inputs[index++]),
                                XmlConvert.ToSingle(inputs[index++]),
                                XmlConvert.ToSingle(inputs[index++]),
                                XmlConvert.ToSingle(inputs[index++]),
                                XmlConvert.ToSingle(inputs[index++]),
                                XmlConvert.ToSingle(inputs[index++]),
                                XmlConvert.ToSingle(inputs[index++]),
                                XmlConvert.ToSingle(inputs[index++]),
                                XmlConvert.ToSingle(inputs[index++]),
                                XmlConvert.ToSingle(inputs[index++]),
                                XmlConvert.ToSingle(inputs[index++]),
                                XmlConvert.ToSingle(inputs[index++]),
                                XmlConvert.ToSingle(inputs[index++]),
                                XmlConvert.ToSingle(inputs[index++]),
                                XmlConvert.ToSingle(inputs[index++]),
                                XmlConvert.ToSingle(inputs[index++]));
        }

        protected internal override void Serialize(Matrix value, List<string> results)
        {
            results.Add(XmlConvert.ToString(value.M11));
            results.Add(XmlConvert.ToString(value.M12));
            results.Add(XmlConvert.ToString(value.M13));
            results.Add(XmlConvert.ToString(value.M14));
            results.Add(XmlConvert.ToString(value.M21));
            results.Add(XmlConvert.ToString(value.M22));
            results.Add(XmlConvert.ToString(value.M23));
            results.Add(XmlConvert.ToString(value.M24));
            results.Add(XmlConvert.ToString(value.M31));
            results.Add(XmlConvert.ToString(value.M32));
            results.Add(XmlConvert.ToString(value.M33));
            results.Add(XmlConvert.ToString(value.M34));
            results.Add(XmlConvert.ToString(value.M41));
            results.Add(XmlConvert.ToString(value.M42));
            results.Add(XmlConvert.ToString(value.M43));
            results.Add(XmlConvert.ToString(value.M44));
        }
    }
}