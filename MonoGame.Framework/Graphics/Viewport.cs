// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a view field used by <see cref="GraphicsDevice"/>. 
    /// </summary>
    [DataContract]
    public struct Viewport
    {
        #region Private Fields

        private int x;
		private int y;
		private int width;
		private int height;
		private float minDepth;
		private float maxDepth;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the X coordinate of top-left corner of the viewport.
        /// </summary>
        [DataMember]
        public int X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        /// <summary>
        /// Gets or sets the Y coordinate of top-left corner of the viewport.
        /// </summary>
        [DataMember]
        public int Y
        {
            get
            {
                return this.y;

            }
            set
            {
                y = value;
            }
        }

        /// <summary>
        /// Gets or sets the width of the viewport.
        /// </summary>
        [DataMember]
        public int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                width = value;
            }
        }

        /// <summary>
        /// Gets or sets the height of the viewport.
        /// </summary>
        [DataMember]
        public int Height
        {
			get 
            {
				return this.height;
			}
			set 
            {
				height = value;
			}
		}

        /// <summary>
        /// Gets or sets the minimal depth of the viewport. 
        /// </summary>
        [DataMember]
        public float MinDepth
        {
            get
            {
                return this.minDepth;
            }
            set
            {
                minDepth = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximal depth of the viewport. 
        /// </summary>
        [DataMember]
        public float MaxDepth
        {
			get 
            {
				return this.maxDepth;
			}
			set 
            {
				maxDepth = value;
			}
		}		

        /// <summary>
        /// Gets aspect ratio of the viewport.
        /// </summary>
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
		
        /// <summary>
        /// Gets or sets the size of the viewport.
        /// </summary>
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
		
        /// <summary>
        /// Gets the tile safe area of the viewport.
        /// </summary>
		public Rectangle TitleSafeArea 
		{
			get
			{
				return new Rectangle(x,y,width,height);
			}
		}

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a new instance of <see cref="Viewport"/> struct. Assumes MinDepth as 0.0f and MaxDepth as 1.0f.
        /// </summary>
        /// <param name="x">X coordinate of top-left corner of the viewport in pixels.</param>
        /// <param name="y">Y coordinate of top-left corner of the viewport in pixels.</param>
        /// <param name="width">Width of the viewport in pixels.</param>
        /// <param name="height">Height of the viewport in pixels.</param>
        public Viewport(int x, int y, int width, int height)
		{
			this.x = x;
		    this.y = y;
		    this.width = width;
		    this.height = height;
		    this.minDepth = 0.0f;
		    this.maxDepth = 1.0f;
		}

        /// <summary>
        /// Creates a new instance of <see cref="Viewport"/> struct.
        /// </summary>
        /// <param name="x">X coordinate of top-left corner of the viewport in pixels.</param>
        /// <param name="y">Y coordinate of top-left corner of the viewport in pixels.</param>
        /// <param name="width">Width of the viewport in pixels.</param>
        /// <param name="height">Height of the viewport in pixels.</param>
        /// <param name="minDepth">Minimal depth of the viewport.</param>
        /// <param name="maxDepth">Maximal depth of the viewport.</param>
        public Viewport(int x, int y, int width, int height, float minDepth, float maxDepth)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.minDepth = minDepth;
            this.maxDepth = maxDepth;
        }
		
        /// <summary>
        /// Creates a new instance of <see cref="Viewport"/> struct.
        /// </summary>
        /// <param name="bounds">Bounds that describes top-left corner and size for the viewport.</param>
		public Viewport(Rectangle bounds) : this(bounds.X, bounds.Y, bounds.Width, bounds.Height)
		{
		}

        /// <summary>
        /// Creates a new instance of <see cref="Viewport"/> struct.
        /// </summary>
        /// <param name="bounds">Bounds that describes top-left corner and size for the viewport.</param>
        /// <param name="minDepth">Minimal depth of the viewport.</param>
        /// <param name="maxDepth">Maximal depth of the viewport.</param>
        public Viewport(Rectangle bounds,float minDepth,float maxDepth) : this(bounds.X, bounds.Y, bounds.Width, bounds.Height, minDepth, maxDepth)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Projects <see cref="Vector3"/> from object space to screen space.
        /// </summary>
        /// <param name="source">The <see cref="Vector3"/> needs to be projected.</param>
        /// <param name="projection">Projection matrix.</param>
        /// <param name="view">View matrix.</param>
        /// <param name="world">World matrix.</param>
        /// <returns>The <see cref="Vector3"/> in screen space.</returns>
        public Vector3 Project(Vector3 source, Matrix projection, Matrix view, Matrix world)
        {
            Matrix matrix = Matrix.Multiply(Matrix.Multiply(world, view), projection);
		    Vector3 vector = Vector3.Transform(source, matrix);
		    float a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
		    if (!WithinEpsilon(a, 1f))
		    {
		        vector.X = vector.X / a;
		        vector.Y = vector.Y / a;
		        vector.Z = vector.Z / a;
		    }
		    vector.X = (((vector.X + 1f) * 0.5f) * this.width) + this.x;
		    vector.Y = (((-vector.Y + 1f) * 0.5f) * this.height) + this.y;
		    vector.Z = (vector.Z * (this.maxDepth - this.minDepth)) + this.minDepth;
		    return vector;
        }

        /// <summary>
        /// Unprojects <see cref="Vector3"/> from screen space to object space.
        /// </summary>
        /// <param name="source">The <see cref="Vector3"/> needs to be unprojected.</param>
        /// <param name="projection">Projection matrix.</param>
        /// <param name="view">View matrix.</param>
        /// <param name="world">World matrix.</param>
        /// <returns>The <see cref="Vector3"/> in object space.</returns>
        public Vector3 Unproject(Vector3 source, Matrix projection, Matrix view, Matrix world)
        {
            Matrix matrix = Matrix.Invert(Matrix.Multiply(Matrix.Multiply(world, view), projection));
		    source.X = (((source.X - this.x) / ((float) this.width)) * 2f) - 1f;
		    source.Y = -((((source.Y - this.y) / ((float) this.height)) * 2f) - 1f);
		    source.Z = (source.Z - this.minDepth) / (this.maxDepth - this.minDepth);
		    Vector3 vector = Vector3.Transform(source, matrix);
		    float a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
		    if (!WithinEpsilon(a, 1f))
		    {
		        vector.X = vector.X / a;
		        vector.Y = vector.Y / a;
		        vector.Z = vector.Z / a;
		    }
		    return vector;
        }

        #endregion

        #region Private Methods

        private static bool WithinEpsilon(float a, float b)
		{
		    float num = a - b;
		    return ((-1.401298E-45f <= num) && (num <= float.Epsilon));
		}

        #endregion

        #region Object Overrided Methods

        /// <summary>
        /// Converts the viewport values of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation of the viewport values of this instance.</returns>
        public override string ToString ()
	{
            return "{X:" + x + " Y:" + y + " Width:" + width + " Height:" + height + " MinDepth:" + minDepth + " MaxDepth:" + maxDepth + "}";
        }

        #endregion
    }
}

