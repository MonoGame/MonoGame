// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Text;

namespace MonoGame.Framework.Utilities
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

        public void WriteCount(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException("count");
            this.Write(count);
        }

        public void WriteIndex(int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException("index");
            this.Write(index);
        }

        public void WriteSignedIndex(int index)
        {
            this.Write(index);
        }
    }
}
