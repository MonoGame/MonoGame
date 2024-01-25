namespace System.Numerics
{
    /// <summary>
    /// This is a dummy Vector2 type for platforms where System.Numerics.Vectors is not supported or available
    /// </summary>
    public struct Vector2
    {
        public Single X;
        public Single Y;
 
        public Vector2(Single x, Single y)
        {
            X = x;
            Y = y;
        }
    }
}
