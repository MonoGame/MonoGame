// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;
using System.Text;

namespace MonoGame.Framework.Utilities
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

        /// <summary>
        /// Used to read data written with <see cref="BinaryWriterEx.Write7BitEncodedInt(int)"/>
        /// </summary>
        /// <returns>An integer value</returns>
        public new int Read7BitEncodedInt()
        {
            return base.Read7BitEncodedInt();
        }

        /// <summary>
        /// Used to read data written with <see cref="BinaryWriterEx.WriteCount(int)"/>
        /// </summary>
        /// <returns>An integer value</returns>
        public int ReadCount()
        {
            return ReadInt32();
        }

        /// <summary>
        /// Used to read data written with <see cref="BinaryWriterEx.WriteIndex(int)"/>
        /// </summary>
        /// <returns>An integer value</returns>
        public int ReadIndex()
        {
            return ReadInt32();
        }

        /// <summary>
        /// Used to read data written with <see cref="BinaryWriterEx.WriteSignedIndex(int)"/>
        /// </summary>
        /// <returns>An integer value</returns>
        public int ReadSignedIndex()
        {
            return ReadInt32();            
        }        
    }
}
