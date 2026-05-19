// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework
{
    using System;
    using Graphics;
    using SharpDX.Mathematics.Interop;

    static internal class SharpDXHelper
    {
        static public SharpDX.DXGI.SwapEffect ToSwapEffect(PresentInterval presentInterval)
        {
            SharpDX.DXGI.SwapEffect effect;

            switch (presentInterval)
            {
                case PresentInterval.One:
                case PresentInterval.Two:
                default:
                    effect = SharpDX.DXGI.SwapEffect.Discard;
                    break;

                case PresentInterval.Immediate:
                    effect = SharpDX.DXGI.SwapEffect.Sequential;
                    break;
            }

            //if (present.RenderTargetUsage != RenderTargetUsage.PreserveContents && present.MultiSampleCount == 0)
            //effect = SharpDX.DXGI.SwapEffect.Discard;

            return effect;
        }

        static public SharpDX.DXGI.Format ToFormat(DepthFormat format)
        {
            switch (format)
            {
                default:
                case DepthFormat.None:
                    return SharpDX.DXGI.Format.Unknown;

                case DepthFormat.Depth16:
                    return SharpDX.DXGI.Format.D16_UNorm;

                case DepthFormat.Depth24:
                case DepthFormat.Depth24Stencil8:
                    return SharpDX.DXGI.Format.D24_UNorm_S8_UInt;
            }
        }

        static public SharpDX.DXGI.Format ToFormat(SurfaceFormat format)
        {
            switch (format)
            {
                case SurfaceFormat.Color:
                default:
                    return SharpDX.DXGI.Format.R8G8B8A8_UNorm;

                case SurfaceFormat.Bgr565:
                    return SharpDX.DXGI.Format.B5G6R5_UNorm;
                case SurfaceFormat.Bgra5551:
                    return SharpDX.DXGI.Format.B5G5R5A1_UNorm;
                case SurfaceFormat.Bgra4444:
                    return (SharpDX.DXGI.Format)115;
                case SurfaceFormat.Dxt1:
                    return SharpDX.DXGI.Format.BC1_UNorm;
                case SurfaceFormat.Dxt3:
                    return SharpDX.DXGI.Format.BC2_UNorm;
                case SurfaceFormat.Dxt5:
                    return SharpDX.DXGI.Format.BC3_UNorm;
                case SurfaceFormat.NormalizedByte2:
                    return SharpDX.DXGI.Format.R8G8_SNorm;
                case SurfaceFormat.NormalizedByte4:
                    return SharpDX.DXGI.Format.R8G8B8A8_SNorm;
                case SurfaceFormat.Rgba1010102:
                    return SharpDX.DXGI.Format.R10G10B10A2_UNorm;
                case SurfaceFormat.Rg32:
                    return SharpDX.DXGI.Format.R16G16_UNorm;
                case SurfaceFormat.Rgba64:
                    return SharpDX.DXGI.Format.R16G16B16A16_UNorm;
                case SurfaceFormat.Alpha8:
                    return SharpDX.DXGI.Format.A8_UNorm;
                case SurfaceFormat.Single:
                    return SharpDX.DXGI.Format.R32_Float;
                case SurfaceFormat.HalfSingle:
                    return SharpDX.DXGI.Format.R16_Float;
                case SurfaceFormat.HalfVector2:
                    return SharpDX.DXGI.Format.R16G16_Float;
                case SurfaceFormat.Vector2:
                    return SharpDX.DXGI.Format.R32G32_Float;
                case SurfaceFormat.Vector4:
                    return SharpDX.DXGI.Format.R32G32B32A32_Float;
                case SurfaceFormat.HalfVector4:
                    return SharpDX.DXGI.Format.R16G16B16A16_Float;

                case SurfaceFormat.HdrBlendable:
                    // TODO: This needs to check the graphics device and 
                    // return the best hdr blendable format for the device.
                    return SharpDX.DXGI.Format.R16G16B16A16_Float;

                case SurfaceFormat.Bgr32:
                    return SharpDX.DXGI.Format.B8G8R8X8_UNorm;
                case SurfaceFormat.Bgra32:
                    return SharpDX.DXGI.Format.B8G8R8A8_UNorm;

                case SurfaceFormat.ColorSRgb:
                    return SharpDX.DXGI.Format.R8G8B8A8_UNorm_SRgb;
                case SurfaceFormat.Bgr32SRgb:
                    return SharpDX.DXGI.Format.B8G8R8X8_UNorm_SRgb;
                case SurfaceFormat.Bgra32SRgb:
                    return SharpDX.DXGI.Format.B8G8R8A8_UNorm_SRgb;
                case SurfaceFormat.Dxt1SRgb:
                    return SharpDX.DXGI.Format.BC1_UNorm_SRgb;
                case SurfaceFormat.Dxt3SRgb:
                    return SharpDX.DXGI.Format.BC2_UNorm_SRgb;
                case SurfaceFormat.Dxt5SRgb:
                    return SharpDX.DXGI.Format.BC3_UNorm_SRgb;
            }
        }

		static public RawVector2 ToVector2(this Vector2 vec)
        {
            return new RawVector2(vec.X, vec.Y);
        }

        static public RawVector3 ToVector3(this Vector3 vec)
        {
            return new RawVector3(vec.X, vec.Y, vec.Z);
        }

        static public RawVector4 ToVector4(this Vector4 vec)
        {
            return new RawVector4(vec.X, vec.Y, vec.Z, vec.W);
        }

        static public RawColor4 ToColor4(this Color color)
        {
            return new RawColor4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
        }

        static public SharpDX.Direct3D11.Comparison ToComparison(this CompareFunction compare)
        {
            switch (compare)
            {
                case CompareFunction.Always:
                    return SharpDX.Direct3D11.Comparison.Always;

                case CompareFunction.Equal:
                    return SharpDX.Direct3D11.Comparison.Equal;

                case CompareFunction.Greater:
                    return SharpDX.Direct3D11.Comparison.Greater;

                case CompareFunction.GreaterEqual:
                    return SharpDX.Direct3D11.Comparison.GreaterEqual;

                case CompareFunction.Less:
                    return SharpDX.Direct3D11.Comparison.Less;

                case CompareFunction.LessEqual:
                    return SharpDX.Direct3D11.Comparison.LessEqual;

                case CompareFunction.Never:
                    return SharpDX.Direct3D11.Comparison.Never;

                case CompareFunction.NotEqual:
                    return SharpDX.Direct3D11.Comparison.NotEqual;

                default:
                    throw new ArgumentException("Invalid comparison!");
            }
        }
    }
}
