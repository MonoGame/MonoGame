#pragma warning disable CS1591
namespace System.Numerics
{
    /// <summary>
    /// This is a dummy Vector4 type for platforms where System.Numerics.Vectors is not supported or available
    /// </summary>
    public struct Vector4
    {
        public Single X;
        public Single Y;
        public Single Z;
        public Single W;
 
        public Vector4(Single x, Single y, Single z, Single w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }
}