// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors;

/// <summary>
/// As the name implies, this processor simply passes data through as-is.
/// </summary>
[ContentProcessor(DisplayName = "No Processing Required")]
public class PassThroughProcessor : ContentProcessor<object, object>
{
    /// <inheritdoc/>
    public override object Process(object input, ContentProcessorContext context) => input;
}
