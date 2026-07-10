// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    partial struct VertexElement
    {
        /// <summary>
        /// Gets the DirectX <see cref="SharpDX.Direct3D11.InputElement"/>.
        /// </summary>
        /// <param name="slot">The input resource slot.</param>
        /// <param name="instanceFrequency">
        /// The number of instances to draw using the same per-instance data before advancing in the
        /// buffer by one element. This value must be 0 for an element that contains per-vertex
        /// data.
        /// </param>
        /// <returns><see cref="SharpDX.Direct3D11.InputElement"/>.</returns>
        /// <exception cref="NotSupportedException">
        /// Unknown vertex element format or usage!
        /// </exception>
        internal SharpDX.Direct3D11.InputElement GetInputElement(int slot, int instanceFrequency)
        {
            var element = new SharpDX.Direct3D11.InputElement();

            switch (_usage)
            {
                case VertexElementUsage.Position:
                    element.SemanticName = "POSITION";
                    break;
                case VertexElementUsage.Color:
                    element.SemanticName = "COLOR";
                    break;
                case VertexElementUsage.Normal:
                    element.SemanticName = "NORMAL";
                    break;
                case VertexElementUsage.TextureCoordinate:
                    element.SemanticName = "TEXCOORD";
                    break;
                case VertexElementUsage.BlendIndices:
                    element.SemanticName = "BLENDINDICES";
                    break;
                case VertexElementUsage.BlendWeight:
                    element.SemanticName = "BLENDWEIGHT";
                    break;
                case VertexElementUsage.Binormal:
                    element.SemanticName = "BINORMAL";
                    break;
                case VertexElementUsage.Tangent:
                    element.SemanticName = "TANGENT";
                    break;
                case VertexElementUsage.PointSize:
                    element.SemanticName = "PSIZE";
                    break;
                default:
                    throw new NotSupportedException("Unknown vertex element usage!");
            }

            element.SemanticIndex = _usageIndex;

            switch (_format)
            {
                case VertexElementFormat.Single:
                    element.Format = SharpDX.DXGI.Format.R32_Float;
                    break;
                case VertexElementFormat.Vector2:
                    element.Format = SharpDX.DXGI.Format.R32G32_Float;
                    break;
                case VertexElementFormat.Vector3:
                    element.Format = SharpDX.DXGI.Format.R32G32B32_Float;
                    break;
                case VertexElementFormat.Vector4:
                    element.Format = SharpDX.DXGI.Format.R32G32B32A32_Float;
                    break;
                case VertexElementFormat.Color:
                    element.Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
                    break;
                case VertexElementFormat.Byte4:
                    element.Format = SharpDX.DXGI.Format.R8G8B8A8_UInt;
                    break;
                case VertexElementFormat.Short2:
                    element.Format =  SharpDX.DXGI.Format.R16G16_SInt;
                    break;
                case VertexElementFormat.Short4:
                    element.Format =  SharpDX.DXGI.Format.R16G16B16A16_SInt;
                    break;
                case VertexElementFormat.NormalizedShort2:
                    element.Format =  SharpDX.DXGI.Format.R16G16_SNorm;
                    break;
                case VertexElementFormat.NormalizedShort4:
                    element.Format =  SharpDX.DXGI.Format.R16G16B16A16_SNorm;
                    break;
                case VertexElementFormat.HalfVector2:
                    element.Format =  SharpDX.DXGI.Format.R16G16_Float;
                    break;
                case VertexElementFormat.HalfVector4:
                    element.Format =  SharpDX.DXGI.Format.R16G16B16A16_Float;
                    break;                
                default:
                    throw new NotSupportedException("Unknown vertex element format!");
            }

            element.Slot = slot;
            element.AlignedByteOffset = _offset;
            
            // Note that instancing is only supported in feature level 9.3 and above.
            element.Classification = (instanceFrequency == 0) 
                                     ? SharpDX.Direct3D11.InputClassification.PerVertexData
                                     : SharpDX.Direct3D11.InputClassification.PerInstanceData;
            element.InstanceDataStepRate = instanceFrequency;

            return element;
        }
    }
}
