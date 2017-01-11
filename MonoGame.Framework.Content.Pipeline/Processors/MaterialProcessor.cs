// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// Provides methods and properties for maintaining a collection of named texture references.
    /// </summary>
    /// <remarks>In addition to texture references, opaque data values are stored in the OpaqueData property of the base class.</remarks>
    [ContentProcessor(DisplayName = "Material - MonoGame")]
    public class MaterialProcessor : ContentProcessor<MaterialContent, MaterialContent>
    {
        Color colorKeyColor;
        bool colorKeyEnabled;
        MaterialProcessorDefaultEffect defaultEffect;
        bool generateMipmaps;
        bool premultiplyTextureAlpha;
        bool resizeTexturesToPowerOfTwo;
        TextureProcessorOutputFormat textureFormat;

        /// <summary>
        /// Gets or sets the color value to replace with transparent black.
        /// </summary>
        /// <value>Color value of the material to replace with transparent black.</value>
        [DefaultValue(typeof(Color), "255, 0, 255, 255")]
        [DisplayName("Color Key Color")]
        [Description("If the texture is color-keyed, pixels of this color are replaced with transparent black.")]
        public virtual Color ColorKeyColor { get { return colorKeyColor; } set { colorKeyColor = value; } }

        /// <summary>
        /// Specifies whether color keying of a texture is enabled.
        /// </summary>
        /// <value>true if color keying is enabled; false otherwise.</value>
        [DefaultValue(true)]
        [DisplayName("Color Key Enabled")]
        [Description("If enabled, the source texture is color-keyed. Pixels matching the value of \"Color Key Color\" are replaced with transparent black.")]
        public virtual bool ColorKeyEnabled { get { return colorKeyEnabled; } set { colorKeyEnabled = value; } }

        /// <summary>
        /// The default effect type for this instance of MaterialProcessor.
        /// </summary>
        /// <value>The default effect type.</value>
        /// <remarks>When MaterialProcessor is instantiated, DefaultEffect is set to default to BasicEffect Class.</remarks>
        [DefaultValue(typeof(MaterialProcessorDefaultEffect), "BasicEffect")]
        [DisplayName("Default Effect")]
        [Description("The default effect type.")]
        public virtual MaterialProcessorDefaultEffect DefaultEffect { get { return defaultEffect; } set { defaultEffect = value; } }

        /// <summary>
        /// Specifies if a full chain of mipmaps are generated from the source material. Existing mipmaps of the material are not replaced.
        /// </summary>
        /// <value>true if mipmap generation is enabled; false otherwise.</value>
        [DefaultValue(false)]
        [DisplayName("Generate Mipmaps")]
        [Description("If enabled, a full chain of mipmaps are generated from the source material. Existing mipmaps of the material are not replaced.")]
        public virtual bool GenerateMipmaps { get { return generateMipmaps; } set { generateMipmaps = value; } }

        /// <summary>
        /// Specifies whether alpha premultiply of textures is enabled.
        /// </summary>
        /// <value>true if alpha premultiply is enabled; false otherwise.</value>
        [DefaultValue(true)]
        [DisplayName("Premultiply Alpha")]
        [Description("If enabled, the texture is converted to premultiplied alpha format.")]
        public virtual bool PremultiplyTextureAlpha { get { return premultiplyTextureAlpha; } set { premultiplyTextureAlpha = value; } }

        /// <summary>
        /// Specifies whether resizing of a material is enabled. Typically used to maximize compatability with a graphics card because many graphics cards do not support a material size that is not a power of two. If ResizeTexturesToPowerOfTwo is enabled, the material is resized to the next largest power of two.
        /// </summary>
        /// <value>true if resizing is enabled; false otherwise.</value>
        [DefaultValue(true)]
        [DisplayName("Resize to Power of Two")]
        [Description("If enabled, the texture is resized to the next largest power of two, maximizing compatibility. Many graphics cards do not support texture sizes that are not a power of two.")]
        public virtual bool ResizeTexturesToPowerOfTwo { get { return resizeTexturesToPowerOfTwo; } set { resizeTexturesToPowerOfTwo = value; } }

        /// <summary>
		/// Specifies the texture format of output materials. Materials can either be left unchanged from the source asset, converted to a corresponding Color, or compressed using the appropriate DxtCompressed format.
        /// </summary>
        /// <value>The texture format of the output.</value>
        [DefaultValue(typeof(TextureProcessorOutputFormat), "Color")]
        [DisplayName("Texture Format")]
        [Description("Specifies the SurfaceFormat type of processed textures. Textures can either remain unchanged from the source asset, converted to the Color format, or DXT compressed.")]
        public virtual TextureProcessorOutputFormat TextureFormat { get { return textureFormat; } set { textureFormat = value; } }

        /// <summary>
        /// Initializes a new instance of the MaterialProcessor class.
        /// </summary>
        public MaterialProcessor()
        {
            colorKeyColor = Color.Magenta;
            colorKeyEnabled = true;
            defaultEffect = MaterialProcessorDefaultEffect.BasicEffect;
            generateMipmaps = false;
            premultiplyTextureAlpha = true;
            resizeTexturesToPowerOfTwo = true;
            textureFormat = TextureProcessorOutputFormat.Color;
        }

        /// <summary>
        /// Builds effect content.
        /// </summary>
        /// <param name="effect">An external reference to the effect content.</param>
        /// <param name="context">Context for the specified processor.</param>
        /// <returns>A platform-specific compiled binary effect.</returns>
        /// <remarks>If the input to process is of type EffectMaterialContent, this function will be called to request that the EffectContent be built. The EffectProcessor is used to process the EffectContent. Subclasses of MaterialProcessor can override this function to modify the parameters used to build EffectContent. For example, a different version of this function could request a different processor for the EffectContent.</remarks>
        protected virtual ExternalReference<CompiledEffectContent> BuildEffect(ExternalReference<EffectContent> effect, ContentProcessorContext context)
        {
            return context.BuildAsset<EffectContent, CompiledEffectContent>(effect, "EffectProcessor");
        }

        /// <summary>
        /// Builds texture content.
        /// </summary>
        /// <param name="textureName">The name of the texture. This should correspond to the key used to store the texture in Textures.</param>
        /// <param name="texture">The asset to build. This should be a member of Textures.</param>
        /// <param name="context">Context for the specified processor.</param>
        /// <returns>The built texture content.</returns>
        /// <remarks>textureName can be used to determine which processor to use. For example, if a texture is being used as a normal map, the user may not want to use the ModelTextureProcessor on it, which compresses textures.</remarks>
        protected virtual ExternalReference<TextureContent> BuildTexture(string textureName, ExternalReference<TextureContent> texture, ContentProcessorContext context)
        {
            var parameters = new OpaqueDataDictionary();
            parameters.Add("ColorKeyColor", ColorKeyColor);
            parameters.Add("ColorKeyEnabled", ColorKeyEnabled);
            parameters.Add("GenerateMipmaps", GenerateMipmaps);
            parameters.Add("PremultiplyAlpha", PremultiplyTextureAlpha);
            parameters.Add("ResizeToPowerOfTwo", ResizeTexturesToPowerOfTwo);
            parameters.Add("TextureFormat", TextureFormat);

            return context.BuildAsset<TextureContent, TextureContent>(texture, "TextureProcessor", parameters, "TextureImporter", null);
        }

        /// <summary>
        /// Builds the texture and effect content for the material.
        /// </summary>
        /// <param name="input">The material content to build.</param>
        /// <param name="context">Context for the specified processor.</param>
        /// <returns>The built material.</returns>
        /// <remarks>If the MaterialContent is of type EffectMaterialContent, a build is requested for Effect, and validation will be performed on the OpaqueData to ensure that all parameters are valid input to SetValue or SetValueTranspose. If the MaterialContent is a BasicMaterialContent, no validation will be performed on OpaqueData. Process requests builds for all textures in Textures, unless the MaterialContent is of type BasicMaterialContent, in which case a build will only be requested for DiffuseColor. The textures in Textures will be ignored.</remarks>
        public override MaterialContent Process(MaterialContent input, ContentProcessorContext context)
        {
            // Apply specified default effect.
            if (input is BasicMaterialContent && DefaultEffect != MaterialProcessorDefaultEffect.BasicEffect)
            {
                var newMaterial = CreateDefaultMaterial(DefaultEffect);
                
                // Preserve material properties.
                newMaterial.Name = input.Name;
                newMaterial.Identity = input.Identity;
                foreach (var item in input.OpaqueData)
                    newMaterial.OpaqueData.Add(item.Key, item.Value);
                foreach (var item in input.Textures)
                    newMaterial.Textures.Add(item.Key, item.Value);
                
                input = newMaterial;
            }

            // Docs say that if it's a basic effect, only build the diffuse texture.
            var basic = input as BasicMaterialContent;
            if (basic != null)
            {
                ExternalReference<TextureContent> texture;
                if (basic.Textures.TryGetValue(BasicMaterialContent.TextureKey, out texture))
                    basic.Texture = BuildTexture(texture.Filename, texture, context);

                return basic;
            }

            // Build custom effects
            var effectMaterial = input as EffectMaterialContent;
            if (effectMaterial != null && effectMaterial.CompiledEffect == null)
            {
                if (effectMaterial.Effect == null)
                    throw new PipelineException("EffectMaterialContent.Effect or EffectMaterialContent.CompiledEffect should be set for materials with a custom effect.");
                effectMaterial.CompiledEffect = BuildEffect(effectMaterial.Effect, context);
                // TODO: Docs say to validate OpaqueData for SetValue/SetValueTranspose
                // Does that mean to match up with effect param names??
            }

            // Build all textures
            var keys = new List<string>(input.Textures.Keys);
            foreach (string key in keys)
            {
                var texture = input.Textures[key];
                var builtTexture = BuildTexture(texture.Filename, texture, context);
                input.Textures[key] = builtTexture;
            }

            return input;
        }

        /// <summary>
        /// Helper method which returns the material for a default effect.
        /// </summary>
        /// <returns>A material.</returns>
        public static MaterialContent CreateDefaultMaterial(MaterialProcessorDefaultEffect effect)
        {
            switch (effect)
            {
                case MaterialProcessorDefaultEffect.BasicEffect:
                    return new BasicMaterialContent();
                case MaterialProcessorDefaultEffect.SkinnedEffect:
                    return new SkinnedMaterialContent();
                case MaterialProcessorDefaultEffect.EnvironmentMapEffect:
                    return new EnvironmentMapMaterialContent();
                case MaterialProcessorDefaultEffect.DualTextureEffect:
                    return new DualTextureMaterialContent();
                case MaterialProcessorDefaultEffect.AlphaTestEffect:
                    return new AlphaTestMaterialContent();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Helper method which returns the default effect for a material.
        /// </summary>
        /// <returns>The default effect.</returns>
        public static MaterialProcessorDefaultEffect GetDefaultEffect(MaterialContent content)
        {
            if (content is AlphaTestMaterialContent)
                return MaterialProcessorDefaultEffect.AlphaTestEffect;
            if (content is BasicMaterialContent)
                return MaterialProcessorDefaultEffect.BasicEffect;
            if (content is DualTextureMaterialContent)
                return MaterialProcessorDefaultEffect.DualTextureEffect;
            if (content is EnvironmentMapMaterialContent)
                return MaterialProcessorDefaultEffect.EnvironmentMapEffect;
            if (content is SkinnedMaterialContent)
                return MaterialProcessorDefaultEffect.SkinnedEffect;

            throw new ArgumentOutOfRangeException("Unknown material content type!");
        }
    }
}
