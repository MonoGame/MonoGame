// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
    struct DspParameter
    {
        public float Value;
        public readonly float MinValue;
        public readonly float MaxValue;

        public DspParameter(BinaryReader reader)
        {
            // Is always 0 or 1... not sure what it is for.
            reader.ReadByte();

            // The value and the min/max range for limiting the 
            // results from the RPC curve when animated.
            Value = reader.ReadSingle();
            MinValue = reader.ReadSingle();
            MaxValue = reader.ReadSingle();

            // Looks to always be zero...  maybe some padding
            // for future expansion that never occured?
            reader.ReadUInt16();
        }

        public override string ToString()
        {
            return "Value:" + Value + " MinValue:" + MinValue + " MaxValue:" + MaxValue;
        }
    }
}