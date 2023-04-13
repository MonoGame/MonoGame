using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Xna.Framework.Content;

/// <summary>
/// Mark the target assembly as one that requires content readers assemblies at runtime.
/// </summary>
/// <remarks>
/// By default, when using NativeAOT with MonoGame, trimmer will cut off content reader types from the native output, because they are used
/// in the consumer project, but not by MonoGame itself. The attribute will tell the linker that all MonoGame-bundled content readers should be presented in the output.
/// </remarks>
/// <example>[assembly: ContentReadersConsumer]</example>
[AttributeUsage(AttributeTargets.Assembly)]
public class ContentReadersConsumerAttribute : Attribute
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.ByteReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.SByteReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.DateTimeReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.DecimalReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.BoundingSphereReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.BoundingFrustumReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.RayReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.ListReader`1", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.ArrayReader`1", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.SpriteFontReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.Texture2DReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.CharReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.RectangleReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.StringReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.Vector2Reader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.Vector3Reader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.Vector4Reader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.CurveReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.IndexBufferReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.BoundingBoxReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.MatrixReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.BasicEffectReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.VertexBufferReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.AlphaTestEffectReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.EnumReader`1", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.ArrayReader`1", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.EnumReader`1", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.NullableReader`1", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.EffectMaterialReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.ExternalReferenceReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.SoundEffectReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.SongReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.ModelReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.Int32Reader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.EffectReader", "MonoGame.Framework")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Microsoft.Xna.Framework.Content.SingleReader", "MonoGame.Framework")]
    static ContentReadersConsumerAttribute()
    {
    }
}
