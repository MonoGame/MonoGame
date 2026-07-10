#pragma warning disable CS1591
namespace System.Numerics
{
    /// <summary>
    /// This is a dummy Quaternion type for platforms where System.Numerics.Vectors is not supported or available
    /// </summary>
    public struct Quaternion
    {
        public float X;
        public float Y;
        public float Z;
        public float W;
 
        public Quaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }
}
