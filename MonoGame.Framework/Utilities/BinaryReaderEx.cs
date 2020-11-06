// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;
using System.Text;

namespace MonoGame.Framework.Utilities
{
    public class BinaryReaderEx : BinaryReader
    {
        private readonly int _EffectMgfxVersion;

        public int MGFXVersion { get { return _EffectMgfxVersion; } }

        public BinaryReaderEx(Stream input, int mgfxVersion) : base(input)
        {
            _EffectMgfxVersion = mgfxVersion;
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
            if (_EffectMgfxVersion == 9)
            {
                return (int)ReadByte();
            }

            return ReadInt32();
        }

        /// <summary>
        /// Used to read data written with <see cref="BinaryWriterEx.WriteIndex(int)"/>
        /// </summary>
        /// <returns>An integer value</returns>
        public int ReadIndex()
        {
            if (_EffectMgfxVersion == 9)
            {
                return (int)ReadByte();
            }

            return ReadInt32();
        }

        /// <summary>
        /// Used to read data written with <see cref="BinaryWriterEx.WriteSignedIndex(int)"/>
        /// </summary>
        /// <returns>An integer value</returns>
        public int ReadSignedIndex()
        {            
            if (_EffectMgfxVersion == 9)
            {
                int idx = (int)ReadByte();
                if (idx == 255) idx = -1;
                return idx;
            }

            return ReadInt32();            
        }        
    }
}
