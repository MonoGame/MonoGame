namespace Microsoft.Xna.Framework.Graphics
{
    public interface IEffectTechnique
    {
        IEffectPassCollection Passes { get; }

        IEffectAnnotationCollection Annotations { get; }

        string Name { get; }
    }
}