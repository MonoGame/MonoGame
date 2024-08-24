#pragma warning disable CS1591
namespace System.Numerics
{
    /// <summary>
    /// This is a dummy Vector2 type for platforms where System.Numerics.Vectors is not supported or available
    /// </summary>
    public struct Vector2
    {
        /// <summary>The X component of the vector.</summary>
        public Single X;

        /// <summary>The Y component of the vector.</summary>
        public Single Y;

        /// <summary>Creates a vector with the specified values.</summary>
        /// <param name="x">The value assigned to the <see cref="X"/> field.</param>
        /// <param name="y">The value assigned to the <see cref="Y"/> field.</param>
        public Vector2(Single x, Single y)
        {
            X = x;
            Y = y;
        }
    }
}
