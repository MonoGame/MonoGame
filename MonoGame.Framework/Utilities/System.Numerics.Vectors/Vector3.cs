namespace System.Numerics
{
    /// <summary>
    /// This is a dummy Vector3 type for platforms where System.Numerics.Vectors is not supported or available
    /// </summary>
    public struct Vector3
    {
        /// <summary>The X component of the vector.</summary>
        public Single X;
        /// <summary>The Y component of the vector.</summary>
        public Single Y;
        /// <summary>The Z component of the vector.</summary>
        public Single Z;

        /// <summary>Creates a vector with the specified values.</summary>
        /// <param name="x">The value assigned to the <see cref="X"/> field.</param>
        /// <param name="y">The value assigned to the <see cref="Y"/> field.</param>
        /// <param name="z">The value assigned to the <see cref="Z"/> field.</param>
        public Vector3(Single x, Single y, Single z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
