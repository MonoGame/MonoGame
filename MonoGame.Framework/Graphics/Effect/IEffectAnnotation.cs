using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public interface IEffectAnnotation
    {
        EffectParameterClass ParameterClass { get; }

        EffectParameterType ParameterType { get; }

        string Name { get; }

        int RowCount { get; }

        int ColumnCount { get; }

        string Semantic { get; }
    }
}