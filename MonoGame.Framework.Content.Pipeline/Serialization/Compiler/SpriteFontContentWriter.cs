// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

/// <summary>
/// Provides methods and properties to write a <see cref="SpriteFontContent"/> to binary format.
/// </summary>
[ContentTypeWriter]
public class SpriteFontContentWriter : ContentTypeWriter<SpriteFontContent>
{
    /// <inheritdoc/>
    protected override void Write(ContentWriter output, SpriteFontContent value)
    {
        output.WriteObject(value.Texture);
        output.WriteObject(value.Glyphs);
        output.WriteObject(value.Cropping);
        output.WriteObject(value.CharacterMap);
        output.Write(value.VerticalLineSpacing);
        output.Write(value.HorizontalSpacing);
        output.WriteObject(value.Kerning);
        if (value.DefaultCharacter != null)
        {
            output.Write(true);
            output.Write((char)value.DefaultCharacter);
        }
        else
        {
            output.Write(false);
        }
    }

    /// <summary>
    /// Gets the assembly qualified name of the runtime loader for this type.
    /// </summary>
    /// <param name="targetPlatform">Name of the platform.</param>
    /// <returns>Name of the runtime loader.</returns>
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
        => $"{typeof(ContentReader).Namespace}.SpriteFontReader, {typeof(ContentReader).Assembly.FullName}";

    /// <summary>
    /// Gets the assembly qualified name of the runtime target type. The runtime target type often matches the design time type, but may differ.
    /// </summary>
    /// <param name="targetPlatform">The target platform.</param>
    /// <returns>The qualified name.</returns>
    public override string GetRuntimeType(TargetPlatform targetPlatform)
        => $"{typeof(ContentReader).Namespace}.SpriteFontReader, {typeof(ContentReader).AssemblyQualifiedName}";

    /// <summary>
    /// Indicates whether a given type of content should be compressed.
    /// </summary>
    /// <param name="targetPlatform">The target platform of the content build.</param>
    /// <param name="value">The object about to be serialized, or null if a collection of objects is to be serialized.</param>
    /// <returns>true if the content of the requested type should be compressed; false otherwise.</returns>
    /// <remarks>This base class implementation of this method always returns true. It should be overridden
    /// to return false if there would be little or no useful reduction in size of the content type's data
    /// from a general-purpose lossless compression algorithm.
    /// The implementations for Song Class and SoundEffect Class data return false because data for these
    /// content types is already in compressed form.</remarks>
    protected internal override bool ShouldCompressContent(TargetPlatform targetPlatform, object value) => false;
}
