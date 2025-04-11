#region File Description
//-----------------------------------------------------------------------------
// NormalMappingModelProcessor.cs
//
// This file is subject to the terms and conditions defined in file 'LICENSE.txt', which is part of this source code package.
// MonoGame - Copyright (C) The MonoGame Team
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System;

#endregion

namespace MonoGame.Tests.ContentPipeline
{
    /// <summary>
    /// The NormalMappingModelProcessor is used to change the material/effect applied
    /// to a model. After going through this processor, the output model will be set
    /// up to be rendered with NormalMapping.fx.
    /// </summary>
    [ContentProcessor(DisplayName = "Animation Validation")]
    public class AnimatedModelProcessor : ModelProcessor
    {
        public override ModelContent Process(NodeContent input,
            ContentProcessorContext context)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            context.Logger.LogImportantMessage("processing: " + input.Name);
            return base.Process(input, context);
        }
    }
}
