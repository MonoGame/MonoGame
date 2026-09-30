// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

/// <summary>
/// Writes the dictionary to the output.
/// </summary>
[ContentTypeWriter]
class DictionaryWriter<TKey, TValue> : BuiltInContentWriter<Dictionary<TKey, TValue>> where TKey : notnull
{
    private ContentTypeWriter _keyWriter = null!;
    private ContentTypeWriter _valueWriter = null!;

    /// <inheritdoc/>
    internal override void OnAddedToContentWriter(ContentWriter output)
    {
        base.OnAddedToContentWriter(output);

        _keyWriter = output.GetTypeWriter(typeof(TKey));
        _valueWriter = output.GetTypeWriter(typeof(TValue));
    }

    public override bool CanDeserializeIntoExistingObject => true;

    protected override void Write(ContentWriter output, Dictionary<TKey, TValue> value)
    {
        ArgumentNullException.ThrowIfNull(value);

        output.Write(value.Count);
        foreach (var element in value)
        {
            output.WriteObject(element.Key, _keyWriter);
            output.WriteObject(element.Value, _valueWriter);
        }
    }
}
