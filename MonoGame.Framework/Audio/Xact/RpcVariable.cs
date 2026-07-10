// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Audio
{
    struct RpcVariable 
    {
        public string Name;
        public float Value;
        public byte Flags;
        public float InitValue;
        public float MaxValue;
        public float MinValue;

        public bool IsPublic
        {
            get { return (Flags & 0x1) != 0; }
        }

        public bool IsReadOnly
        {
            get { return (Flags & 0x2) != 0; }
        }

        public bool IsGlobal
        {
            get { return (Flags & 0x4) == 0; }
        }

        public bool IsReserved
        {
            get { return (Flags & 0x8) != 0; }
        }

        public void SetValue(float value)
        {
            if (value < MinValue)
                Value = MinValue;
            else if (value > MaxValue)
                Value = MaxValue;
            else
                Value = value;
        }
    }
}