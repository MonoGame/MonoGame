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

using Microsoft.Xna.Framework;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Graphics
{
#if WINRT
    [DataContract]
#else
    [Serializable]
#endif
    public struct Viewport
    {
		/// <summary>
		/// Attributes 
		/// </summary>
		private int x;
		private int y;
		private int width;
		private int height;
		private float minDepth;
		private float maxDepth;
		
		#region Properties
		public int Height {
			get {
				return this.height;
			}
			set {
				height = value;
			}
		}

		public float MaxDepth {
			get {
				return this.maxDepth;
			}
			set {
				maxDepth = value;
			}
		}

		public float MinDepth {
			get {
				return this.minDepth;
			}
			set {
				minDepth = value;
			}
		}

		public int Width {
			get {
				return this.width;
			}
			set {
				width = value;
			}
		}

		public int Y {
			get {
				return this.y;

			}
			set {
				y = value;
			}
		}
		public int X 
		{
			get{ return x;}
			set{ x = value;}
		}
		#endregion
		
		public float AspectRatio 
		{
			get
			{
				if ((height != 0) && (width != 0))
				{
					return (((float) width)/((float)height));
				}
				return 0f;
			}
		}
		
		public Rectangle Bounds 
		{ 
			get 
			{
				Rectangle rectangle;
				rectangle.X = x;
				rectangle.Y = y;
				rectangle.Width = width;
				rectangle.Height = height;
				return rectangle;
			}
				
			set
			{				
				x = value.X;
				y = value.Y;
				width = value.Width;
				height = value.Height;
			}
		}
		
		public Rectangle TitleSafeArea 
		{
			get
			{
				return new Rectangle(x,y,width,height);
			}
		}
		
		public Viewport(int x, int y, int width, int height)
		{
			this.x = x;
		    this.y = y;
		    this.width = width;
		    this.height = height;
		    this.minDepth = 0.0f;
		    this.maxDepth = 1.0f;
		}
		
		public Viewport(Rectangle bounds) : this(bounds.X, bounds.Y, bounds.Width, bounds.Height)
		{
		}

        public Vector3 Project(Vector3 source, Matrix projection, Matrix view, Matrix world)
        {
            Matrix matrix = Matrix.Multiply(Matrix.Multiply(world, view), projection);
		    Vector3 vector = Vector3.Transform(source, matrix);
		    float a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
		    if (!WithinEpsilon(a, 1f))
		    {
		        vector = (Vector3) (vector / a);
		    }
		    vector.X = (((vector.X + 1f) * 0.5f) * this.Width) + this.X;
		    vector.Y = (((-vector.Y + 1f) * 0.5f) * this.Height) + this.Y;
		    vector.Z = (vector.Z * (this.MaxDepth - this.MinDepth)) + this.MinDepth;
		    return vector;
        }

        public Vector3 Unproject(Vector3 source, Matrix projection, Matrix view, Matrix world)
        {
            Matrix matrix = Matrix.Invert(Matrix.Multiply(Matrix.Multiply(world, view), projection));
		    source.X = (((source.X - this.X) / ((float) this.Width)) * 2f) - 1f;
		    source.Y = -((((source.Y - this.Y) / ((float) this.Height)) * 2f) - 1f);
		    source.Z = (source.Z - this.MinDepth) / (this.MaxDepth - this.MinDepth);
		    Vector3 vector = Vector3.Transform(source, matrix);
		    float a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
		    if (!WithinEpsilon(a, 1f))
		    {
		        vector = (Vector3) (vector / a);
		    }
		    return vector;

        }
		
		private static bool WithinEpsilon(float a, float b)
		{
		    float num = a - b;
		    return ((-1.401298E-45f <= num) && (num <= float.Epsilon));
		}


        public override string ToString ()
		{
			return string.Format ("[Viewport: X={0} Y={1} Width={2} Height={3}]", X,Y, Width,Height);
		}
    }
}

