// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.Interop;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics;

partial struct VertexElement
{
    private static readonly nint POSITION = Marshal.StringToHGlobalAnsi("POSITION");
    private static readonly nint COLOR = Marshal.StringToHGlobalAnsi("COLOR");
    private static readonly nint NORMAL = Marshal.StringToHGlobalAnsi("NORMAL");
    private static readonly nint TEXCOORD = Marshal.StringToHGlobalAnsi("TEXCOORD");
    private static readonly nint BLENDINDICES = Marshal.StringToHGlobalAnsi("BLENDINDICES");
    private static readonly nint BLENDWEIGHT = Marshal.StringToHGlobalAnsi("BLENDWEIGHT");
    private static readonly nint BINORMAL = Marshal.StringToHGlobalAnsi("BINORMAL");
    private static readonly nint TANGENT = Marshal.StringToHGlobalAnsi("TANGENT");
    private static readonly nint PSIZE = Marshal.StringToHGlobalAnsi("PSIZE");

    internal MGG_InputElement GetInputElement(int slot, int instanceFrequency)
    {
        var element = new MGG_InputElement();

        switch (_usage)
        {
            case VertexElementUsage.Position:
                element.SemanticName = POSITION;
                break;
            case VertexElementUsage.Color:
                element.SemanticName = COLOR;
                break;
            case VertexElementUsage.Normal:
                element.SemanticName = NORMAL;
                break;
            case VertexElementUsage.TextureCoordinate:
                element.SemanticName = TEXCOORD;
                break;
            case VertexElementUsage.BlendIndices:
                element.SemanticName = BLENDINDICES;
                break;
            case VertexElementUsage.BlendWeight:
                element.SemanticName = BLENDWEIGHT;
                break;
            case VertexElementUsage.Binormal:
                element.SemanticName = BINORMAL;
                break;
            case VertexElementUsage.Tangent:
                element.SemanticName = TANGENT;
                break;
            case VertexElementUsage.PointSize:
                element.SemanticName = PSIZE;
                break;
            default:
                throw new NotSupportedException("Unknown vertex element usage!");
        }

        element.SemanticIndex = (uint)_usageIndex;
        element.Format = _format;
        element.InputSlot = (uint)slot;
        element.AlignedByteOffset = (uint)_offset;
        element.InstanceDataStepRate = (uint)instanceFrequency;

        return element;
    }
}
