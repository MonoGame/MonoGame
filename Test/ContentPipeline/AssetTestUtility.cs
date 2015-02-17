// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Tests.ContentPipeline
{
    internal static class AssetTestUtility
    {
        public static Effect CompileEffect(GraphicsDevice graphicsDevice, params string[] pathParts)
        {
            var effectProcessor = new EffectProcessor();
            var context = new TestProcessorContext(TargetPlatform.Windows, "notused.xnb");
            var compiledEffect = effectProcessor.Process(new EffectContent
            {
                Identity = new ContentIdentity(Paths.Effect(pathParts))
            }, context);

            return new Effect(graphicsDevice, compiledEffect.GetEffectCode());
        }
    }
}