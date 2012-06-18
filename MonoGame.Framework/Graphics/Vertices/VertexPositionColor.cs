using System;
#if WINRT
using System.Runtime.Serialization;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    #if WINRT
    [DataContract]
    #else
    [Serializable]
    #endif
	public struct VertexPositionColor : IVertexType
	{
#if WINRT
        [DataMember]
#endif
		public Vector3 Position;
#if WINRT
        [DataMember]
#endif
		public VertexElementColor Color;

		public static readonly VertexDeclaration VertexDeclaration;

		public VertexPositionColor (Vector3 position, Color color)
		{
			this.Position = position;
			Color = color;
		}

		VertexDeclaration IVertexType.VertexDeclaration {
			get {
				return VertexDeclaration;
			}
		}

		public override int GetHashCode ()
		{
			// TODO: Fix gethashcode
			return 0;
		}

		public override string ToString ()
		{
			return string.Format ("{{Position:{0} Color:{1}}}", new object[] { this.Position, this.Color });
		}

		public static bool operator == (VertexPositionColor left, VertexPositionColor right)
		{
			return ((left.Color == right.Color) && (left.Position == right.Position));
		}

		public static bool operator != (VertexPositionColor left, VertexPositionColor right)
		{
			return !(left == right);
		}

		public override bool Equals (object obj)
		{
			if (obj == null) {
				return false;
			}
			if (obj.GetType () != base.GetType ()) {
				return false;
			}
			return (this == ((VertexPositionColor)obj));
		}

		static VertexPositionColor ()
		{
			VertexElement[] elements = new VertexElement[] { new VertexElement (0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement (12, VertexElementFormat.Color, VertexElementUsage.Color, 0) };
			VertexDeclaration declaration = new VertexDeclaration (elements);
			VertexDeclaration = declaration;
		}
	}
}
