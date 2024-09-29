// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.Interop;

namespace Microsoft.Xna.Framework.Graphics;

partial struct VertexElement
{
    internal MGG_InputElement AsInputElement(int vbSlot, int instanceFrequency)
    {
        var element = new MGG_InputElement();

        element.VertexBufferSlot = (uint)vbSlot;
        element.Format = _format;
        element.AlignedByteOffset = (uint)_offset;
        element.InstanceDataStepRate = (uint)instanceFrequency;

        return element;
    }
}
