using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public struct VertexElement
    {
        internal int _offset;
        internal VertexElementFormat _format;
        internal VertexElementUsage _usage;
        internal int _usageIndex;

        public int Offset
        {
            get
            {
                return this._offset;
            }
            set
            {
                this._offset = value;
            }
        }

        public VertexElementFormat VertexElementFormat
        {
            get
            {
                return this._format;
            }
            set
            {
                this._format = value;
            }
        }

        public VertexElementUsage VertexElementUsage
        {
            get
            {
                return this._usage;
            }
            set
            {
                this._usage = value;
            }
        }
        
        public int UsageIndex
        {
            get
            {
                return this._usageIndex;
            }
            set
            {
                this._usageIndex = value;
            }
        }
        
        public VertexElement(int offset, VertexElementFormat elementFormat, VertexElementUsage elementUsage, int usageIndex)
        {
            this._offset = offset;
            this._usageIndex = usageIndex;
            this._format = elementFormat;
            this._usage = elementUsage;
        }

        public override int GetHashCode()
        {
            // TODO: Fix hashes
            return 0;
        }

        public override string ToString()
        {
            return string.Format("{{Offset:{0} Format:{1} Usage:{2} UsageIndex:{3}}}", new object[] { this.Offset, this.VertexElementFormat, this.VertexElementUsage, this.UsageIndex });
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != base.GetType())
            {
                return false;
            }
            return (this == ((VertexElement)obj));
        }

        public static bool operator ==(VertexElement left, VertexElement right)
        {
            return ((((left._offset == right._offset) && (left._usageIndex == right._usageIndex)) && (left._usage == right._usage)) && (left._format == right._format));
        }

        public static bool operator !=(VertexElement left, VertexElement right)
        {
            return !(left == right);
        }

        #if DIRECTX

        internal SharpDX.Direct3D11.InputElement GetInputElement()
        {
            var element = new SharpDX.Direct3D11.InputElement();

            switch (_usage)
            {
                case Graphics.VertexElementUsage.Position:
                    element.SemanticName = "SV_Position";
                    break;

                case Graphics.VertexElementUsage.Color:
                    element.SemanticName = "COLOR";
                    break;

                case Graphics.VertexElementUsage.Normal:
                    element.SemanticName = "NORMAL";
                    break;

                case Graphics.VertexElementUsage.TextureCoordinate:
                    element.SemanticName = "TEXCOORD";
                    break;

                default:
                    throw new NotImplementedException("Unknown vertex element usage!");
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
                    throw new NotImplementedException("Unknown vertex element format!");
            }

            element.AlignedByteOffset = _offset;
            element.Slot = 0;

            return element;
        }

        #endif
    }
}
