// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;
using System.Text;

namespace Microsoft.Xna.Framework.Utilities
{
    public class BinaryReaderEx : BinaryReader
    {
        public BinaryReaderEx(Stream input) : base(input)
        {
        }

        public BinaryReaderEx(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        public BinaryReaderEx(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        public new int Read7BitEncodedInt()
        {
            return base.Read7BitEncodedInt();
        }
    }
}
