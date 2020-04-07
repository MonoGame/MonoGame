// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;
using System.Text;

namespace Microsoft.Xna.Framework.Utilities
{
    public class BinaryWriterEx : BinaryWriter
    {
        protected BinaryWriterEx()
        {
        }

        public BinaryWriterEx(Stream output) : base(output)
        {
        }

        public BinaryWriterEx(Stream output, Encoding encoding) : base(output, encoding)
        {
        }

        public BinaryWriterEx(Stream output, Encoding encoding, bool leaveOpen) : base(output, encoding, leaveOpen)
        {
        }

        public new void Write7BitEncodedInt(int value)
        {
            base.Write7BitEncodedInt(value);
        }
    }
}
