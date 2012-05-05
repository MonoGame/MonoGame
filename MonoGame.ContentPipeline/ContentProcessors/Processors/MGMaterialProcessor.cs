namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Content.Pipeline.Processors;


[ContentProcessor(DisplayName = "MGMaterialProcessor")]
public class MGMaterialProcessor : MaterialProcessor
{

    protected override ExternalReference<TextureContent>  BuildTexture(string textureName, ExternalReference<TextureContent> texture, ContentProcessorContext context)
    {
        // Fall back to the default if we aren't using IOS
        if (!context.BuildConfiguration.ToUpper().Contains("IOS"))
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
