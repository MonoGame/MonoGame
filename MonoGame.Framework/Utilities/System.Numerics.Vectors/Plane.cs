#pragma warning disable CS1591
namespace System.Numerics
{
    /// <summary>
    /// This is a dummy Plane type for platforms where System.Numerics.Vectors is not supported or available
    /// </summary>
    public struct Plane
    {
        public Vector3 Normal;
        public float D;
 
        public Plane(float x, float y, float z, float d)
        {
            Normal = new Vector3(x, y, z);
            D = d;
        }
 
        public Plane(Vector3 normal, float d)
        {
            Normal = normal;
            D = d;
        }
    }
}
