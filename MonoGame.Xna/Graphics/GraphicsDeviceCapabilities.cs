#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed class GraphicsDeviceCapabilities : IDisposable
    {
        public void Dispose()
        {
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(GraphicsDeviceCapabilities left, GraphicsDeviceCapabilities right)
        {
            throw new NotImplementedException();
        }

        public static bool operator !=(GraphicsDeviceCapabilities left, GraphicsDeviceCapabilities right)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        public int AdapterOrdinalInGroup
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public CompareCaps AlphaCompareCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public FilterCaps CubeTextureFilterCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public CursorCaps CursorCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public DeclarationTypeCaps DeclarationTypeCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public CompareCaps DepthBufferCompareCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public BlendCaps DestinationBlendCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public DeviceCaps DeviceCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public DriverCaps DriverCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public float ExtentsAdjust
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public float GuardBandBottom
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public float GuardBandLeft
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public float GuardBandRight
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public float GuardBandTop
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public LineCaps LineCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int MasterAdapterOrdinal
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int MaxAnisotropy
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int MaxPixelShader30InstructionSlots
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public float MaxPointSize
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int MaxPrimitiveCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int MaxSimultaneousRenderTargets
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int MaxSimultaneousTextures
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int MaxStreams
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int MaxStreamStride
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int MaxTextureAspectRatio
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int MaxTextureHeight
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int MaxTextureRepeat
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int MaxTextureWidth
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int MaxUserClipPlanes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int MaxVertexIndex
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int MaxVertexShader30InstructionSlots
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int MaxVertexShaderConstants
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public float MaxVertexW
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int MaxVolumeExtent
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int NumberOfAdaptersInGroup
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public float PixelShader1xMaxValue
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public PixelShaderCaps PixelShaderCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Version PixelShaderVersion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public PrimitiveCaps PrimitiveCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public RasterCaps RasterCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ShadingCaps ShadingCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public BlendCaps SourceBlendCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public StencilCaps StencilCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public AddressCaps TextureAddressCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public TextureCaps TextureCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public FilterCaps TextureFilterCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public VertexFormatCaps VertexFormatCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public VertexProcessingCaps VertexProcessingCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public VertexShaderCaps VertexShaderCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Version VertexShaderVersion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public FilterCaps VertexTextureFilterCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public AddressCaps VolumeTextureAddressCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public FilterCaps VolumeTextureFilterCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public struct AddressCaps
        {
            public override string ToString()
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public bool SupportsWrap
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMirror
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsClamp
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsBorder
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsIndependentUV
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMirrorOnce
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public struct BlendCaps
        {
            public override string ToString()
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public bool SupportsZero
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsOne
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsSourceColor
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsInverseSourceColor
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsSourceAlpha
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsInverseSourceAlpha
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsDestinationAlpha
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsInverseDestinationAlpha
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsDestinationColor
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsInverseDestinationColor
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsSourceAlphaSat
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsBothSourceAlpha
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsBothInverseSourceAlpha
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsBlendFactor
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public struct CompareCaps
        {
            public override string ToString()
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public bool SupportsNever
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsLess
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsEqual
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsLessEqual
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsGreater
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsNotEqual
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsGreaterEqual
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsAlways
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public struct CursorCaps
        {
            public override string ToString()
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public bool SupportsColor
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsLowResolution
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public struct DeclarationTypeCaps
        {
            public override string ToString()
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public bool SupportsByte4
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsRgba32
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsNormalizedShort2
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsNormalizedShort4
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsRg32
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsRgba64
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsUInt101010
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsNormalized101010
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsHalfVector2
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsHalfVector4
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public struct DeviceCaps
        {
            public override string ToString()
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public bool SupportsExecuteSystemMemory
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsExecuteVideoMemory
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsTransformedVertexSystemMemory
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsTransformedVertexVideoMemory
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsTextureSystemMemory
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsTextureVideoMemory
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsDrawPrimitivesTransformedVertex
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool CanRenderAfterFlip
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsTextureNonLocalVideoMemory
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsDrawPrimitives2
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsSeparateTextureMemories
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsDrawPrimitives2Ex
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsHardwareTransformAndLight
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool CanDrawSystemToNonLocal
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsHardwareRasterization
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool IsDirect3D9Driver
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsStreamOffset
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool VertexElementScanSharesStreamOffset
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public struct DriverCaps
        {
            public override string ToString()
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public bool ReadScanLine
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsFullScreenGamma
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool CanCalibrateGamma
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool CanManageResource
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsDynamicTextures
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool CanAutoGenerateMipMap
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsAlphaFullScreenFlipOrDiscard
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsLinearToSrgbPresentation
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsCopyToVideoMemory
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsCopyToSystemMemory
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public struct FilterCaps
        {
            public override string ToString()
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public bool SupportsMinifyPoint
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMinifyLinear
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMinifyAnisotropic
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMipMapPoint
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMipMapLinear
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMagnifyPoint
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMagnifyLinear
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMagnifyAnisotropic
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMagnifyPyramidalQuad
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMagnifyGaussianQuad
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMinifyPyramidalQuad
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMinifyGaussianQuad
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public struct LineCaps
        {
            public override string ToString()
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public bool SupportsTextureMapping
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsDepthBufferTest
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsBlend
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsAlphaCompare
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsFog
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsAntiAlias
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public struct PixelShaderCaps
        {
            public const int MaxDynamicFlowControlDepth = 0x18;
            public const int MinDynamicFlowControlDepth = 0;
            public const int MaxNumberTemps = 0x20;
            public const int MinNumberTemps = 12;
            public const int MaxStaticFlowControlDepth = 4;
            public const int MinStaticFlowControlDepth = 0;
            public const int MaxNumberInstructionSlots = 0x200;
            public const int MinNumberInstructionSlots = 0x60;
            public override string ToString()
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public bool SupportsPredication
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsArbitrarySwizzle
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsGradientInstructions
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsNoDependentReadLimit
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsNoTextureInstructionLimit
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public int DynamicFlowControlDepth
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public int NumberTemps
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public int StaticFlowControlDepth
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public int NumberInstructionSlots
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public struct PrimitiveCaps
        {
            public override string ToString()
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public bool SupportsMaskZ
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsCullNone
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsCullClockwiseFace
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsCullCounterClockwiseFace
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsColorWrite
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsClipTransformedVertices
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsBlendOperation
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool IsNullReference
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsIndependentWriteMasks
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsFogAndSpecularAlpha
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsSeparateAlphaBlend
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMultipleRenderTargetsIndependentBitDepths
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMultipleRenderTargetsPostPixelShaderBlending
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool HasFogVertexClamped
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public struct RasterCaps
        {
            public override string ToString()
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public bool SupportsDepthBufferTest
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsFogVertex
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsFogTable
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMipMapLevelOfDetailBias
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsDepthBufferLessHsr
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsFogRange
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsAnisotropy
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsWFog
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsDepthFog
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsColorPerspective
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsScissorTest
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsSlopeScaleDepthBias
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsDepthBias
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMultisampleToggle
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public struct ShadingCaps
        {
            public override string ToString()
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public bool SupportsColorGouraudRgb
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsSpecularGouraudRgb
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsAlphaGouraudBlend
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsFogGouraud
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public struct StencilCaps
        {
            public override string ToString()
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public bool SupportsKeep
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsZero
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsReplace
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsIncrementSaturation
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsDecrementSaturation
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsInvert
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsIncrement
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsDecrement
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsTwoSided
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public struct TextureCaps
        {
            public override string ToString()
            {

                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public bool SupportsPerspective
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsAlpha
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool RequiresPower2
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool RequiresSquareOnly
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsTextureRepeatNotScaledBySize
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsNonPower2Conditional
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsProjected
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsCubeMap
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsVolumeMap
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMipMap
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMipVolumeMap
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsMipCubeMap
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool RequiresCubeMapPower2
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool RequiresVolumeMapPower2
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsNoProjectedBumpEnvironment
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public struct VertexFormatCaps
        {
            public override string ToString()
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public short NumberSimultaneousTextureCoordinates
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsDoNotStripElements
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsPointSize
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public struct VertexProcessingCaps
        {
            public override string ToString()
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public bool SupportsTextureGeneration
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsLocalViewer
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsTextureGenerationSphereMap
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public bool SupportsNoTextureGenerationNonLocalViewer
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public struct VertexShaderCaps
        {
            public const int MaxDynamicFlowControlDepth = 0x18;
            public const int MinDynamicFlowControlDepth = 0;
            public const int MaxNumberTemps = 0x20;
            public const int MinNumberTemps = 12;
            public const int MaxStaticFlowControlDepth = 4;
            public const int MinStaticFlowControlDepth = 1;
            public override string ToString()
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public bool SupportsPredication
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public int DynamicFlowControlDepth
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public int NumberTemps
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public int StaticFlowControlDepth
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}

