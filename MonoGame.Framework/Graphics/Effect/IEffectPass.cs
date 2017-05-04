namespace Microsoft.Xna.Framework.Graphics
{
    public interface IEffectPass
    {
        string Name { get; }

        IEffectAnnotationCollection Annotations { get; }

        void Apply();
    }
}