
namespace Microsoft.Xna.Framework
{
    using Microsoft.Xna.Framework.Graphics;

    static internal class SharpDXHelper
    {
        static public SharpDX.DXGI.SwapEffect ToSwapEffect(PresentationParameters present)
        {
            SharpDX.DXGI.SwapEffect effect;

            switch (present.PresentationInterval)
            {
                case PresentInterval.One:
                case PresentInterval.Two:
                default:
                    effect = SharpDX.DXGI.SwapEffect.FlipSequential;
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
                    return SharpDX.DXGI.Format.B4G4R4A4_UNorm;
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
            }
        }

        static public SharpDX.Vector2 ToVector2(this Vector2 vec)
        {
            return new SharpDX.Vector2(vec.X, vec.Y);
        }

        static public SharpDX.Vector3 ToVector3(this Vector3 vec)
        {
            return new SharpDX.Vector3(vec.X, vec.Y, vec.Z);
        }

        static public SharpDX.Vector4 ToVector4(this Vector4 vec)
        {
            return new SharpDX.Vector4(vec.X, vec.Y, vec.Z, vec.W);
        }

        static public SharpDX.X3DAudio.Emitter ToEmitter(this Audio.AudioEmitter emitter)
        {           
            // Pulling out Vector properties for efficiency.
            var pos = emitter.Position;
            var vel = emitter.Velocity;
            var forward = emitter.Forward;
            var up = emitter.Up;

            // From MSDN:
            //  X3DAudio uses a left-handed Cartesian coordinate system, 
            //  with values on the x-axis increasing from left to right, on the y-axis from bottom to top, 
            //  and on the z-axis from near to far. 
            //  Azimuths are measured clockwise from a given reference direction. 
            //
            // From MSDN:
            //  The XNA Framework uses a right-handed coordinate system, 
            //  with the positive z-axis pointing toward the observer when the positive x-axis is pointing to the right, 
            //  and the positive y-axis is pointing up. 
            //
            // Programmer Notes:         
            //  According to this description the z-axis (forward vector) is inverted between these two coordinate systems.
            //  Therefore, we need to negate the z component of any position/velocity values, and negate any forward vectors.

            forward *= -1.0f;
            pos.Z *= -1.0f;
            vel.Z *= -1.0f;

            return new SharpDX.X3DAudio.Emitter()
            {
                Position = new SharpDX.Vector3( pos.X, pos.Y, pos.Z ),
                Velocity = new SharpDX.Vector3( vel.X, vel.Y, vel.Z ),
                OrientFront = new SharpDX.Vector3( forward.X, forward.Y, forward.Z ),
                OrientTop = new SharpDX.Vector3( up.X, up.Y, up.Z ),
                DopplerScaler = emitter.DopplerScale,                   
            };
        }

        static public SharpDX.X3DAudio.Listener ToListener(this Audio.AudioListener listener)
        {
            // Pulling out Vector properties for efficiency.
            var pos = listener.Position;
            var vel = listener.Velocity;
            var forward = listener.Forward;
            var up = listener.Up;

            // From MSDN:
            //  X3DAudio uses a left-handed Cartesian coordinate system, 
            //  with values on the x-axis increasing from left to right, on the y-axis from bottom to top, 
            //  and on the z-axis from near to far. 
            //  Azimuths are measured clockwise from a given reference direction. 
            //
            // From MSDN:
            //  The XNA Framework uses a right-handed coordinate system, 
            //  with the positive z-axis pointing toward the observer when the positive x-axis is pointing to the right, 
            //  and the positive y-axis is pointing up. 
            //
            // Programmer Notes:         
            //  According to this description the z-axis (forward vector) is inverted between these two coordinate systems.
            //  Therefore, we need to negate the z component of any position/velocity values, and negate any forward vectors.

            forward *= -1.0f;
            pos.Z *= -1.0f;
            vel.Z *= -1.0f;

            return new SharpDX.X3DAudio.Listener()
            {
                Position = new SharpDX.Vector3(pos.X, pos.Y, pos.Z),
                Velocity = new SharpDX.Vector3(vel.X, vel.Y, vel.Z),
                OrientFront = new SharpDX.Vector3(forward.X, forward.Y, forward.Z),
                OrientTop = new SharpDX.Vector3(up.X, up.Y, up.Z),                
            };
        }
    }
}
