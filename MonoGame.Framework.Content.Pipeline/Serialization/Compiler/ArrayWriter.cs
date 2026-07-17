// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

/// <summary>
/// Writes the array value to the output.
/// </summary>
[ContentTypeWriter]
class ArrayWriter<T> : BuiltInContentWriter<T[]>
{
    private ContentTypeWriter _elementWriter = null!;

    /// <inheritdoc/>
    internal override void OnAddedToContentWriter(ContentWriter output)
    {
        base.OnAddedToContentWriter(output);

        _elementWriter = output.GetTypeWriter(typeof(T));
    }

    public override string GetRuntimeReader(TargetPlatform targetPlatform)
        => string.Concat(typeof(ContentTypeReader).Namespace, ".", "ArrayReader`1[[", _elementWriter.GetRuntimeType(targetPlatform), "]]");

    protected override void Write(ContentWriter output, T[] value)
    {
        ArgumentNullException.ThrowIfNull(value);
        output.Write(value.Length);
        foreach (var element in value)
            output.WriteObject(element, _elementWriter);
    }
}
