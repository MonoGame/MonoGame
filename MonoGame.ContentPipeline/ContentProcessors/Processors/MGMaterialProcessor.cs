using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace MonoGameContentProcessors.Processors
{ 
    [ContentProcessor(DisplayName = "MonoGame Material")]
    public class MGMaterialProcessor : MaterialProcessor
    {
        protected override ExternalReference<TextureContent>  BuildTexture(string textureName, ExternalReference<TextureContent> texture, ContentProcessorContext context)
        {
            // Fallback if we aren't buiding for iOS.
            var platform = ContentHelper.GetMonoGamePlatform();
            if (platform != MonoGamePlatform.iOS)
                return base.BuildTexture(textureName, texture, context);
        
            var processorParameters = new OpaqueDataDictionary();
            processorParameters.Add("ColorKeyColor", this.ColorKeyColor);
            processorParameters.Add("ColorKeyEnabled", this.ColorKeyEnabled);
            processorParameters.Add("TextureFormat", this.TextureFormat);
            processorParameters.Add("GenerateMipmaps", this.GenerateMipmaps);
            processorParameters.Add("ResizeToPowerOfTwo", this.ResizeTexturesToPowerOfTwo);
            processorParameters.Add("PremultiplyAlpha", this.PremultiplyTextureAlpha);

            return context.BuildAsset<TextureContent, TextureContent>(texture, typeof(MGTextureProcessor).Name, processorParameters, null, null);
        }
    }
}
