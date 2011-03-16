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
    }
}
