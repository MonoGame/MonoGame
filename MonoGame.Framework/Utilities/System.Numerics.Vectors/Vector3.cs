#pragma warning disable CS1591
namespace System.Numerics
{
    /// <summary>
    /// This is a dummy Vector3 type for platforms where System.Numerics.Vectors is not supported or available
    /// </summary>
    public struct Vector3
    {
        public Single X;
        public Single Y;
        public Single Z;
 
        public Vector3(Single x, Single y, Single z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}