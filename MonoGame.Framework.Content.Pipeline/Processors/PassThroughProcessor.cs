// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// As the name implies, this processor simply passes data through as-is.
    /// </summary>
    [ContentProcessor(DisplayName = "No Processing Required")]
    public class PassThroughProcessor : ContentProcessor<object, object>
    {
        public override object Process(object input, ContentProcessorContext context)
        {
            return input;
        }
    }
}
