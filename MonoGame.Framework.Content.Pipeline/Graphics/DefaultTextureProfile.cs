// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    internal class DefaultTextureProfile : TextureProfile
    {
        public override bool Supports(TargetPlatform platform)
        {
            return  platform == TargetPlatform.Android ||
                    platform == TargetPlatform.DesktopGL ||
                    platform == TargetPlatform.DesktopVK ||
                    platform == TargetPlatform.MacOSX ||
                    platform == TargetPlatform.NativeClient ||
                    platform == TargetPlatform.RaspberryPi ||
                    platform == TargetPlatform.Windows ||
                    platform == TargetPlatform.WindowsDX12 ||
                    platform == TargetPlatform.iOS ||
                    platform == TargetPlatform.Web;
        }

        private static bool IsCompressedTextureFormat(TextureProcessorOutputFormat format)
        {
            switch (format)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                case TextureProcessorOutputFormat.Etc1Compressed:
#pragma warning restore CS0618 // Type or member is obsolete

                case TextureProcessorOutputFormat.AtcCompressed:
                case TextureProcessorOutputFormat.DxtCompressed:
                case TextureProcessorOutputFormat.EtcCompressed:
                case TextureProcessorOutputFormat.PvrCompressed:
                case TextureProcessorOutputFormat.AstcCompressed:
                    return true;
            }
            return false;
        }

        private static TextureProcessorOutputFormat GetTextureFormatForPlatform(TextureProcessorOutputFormat format, TargetPlatform platform)
        {
            // Select the default texture compression format for the target platform
            if (format == TextureProcessorOutputFormat.Compressed)
            {
                format = platform switch
                {
                    TargetPlatform.iOS => TextureProcessorOutputFormat.PvrCompressed,
                    TargetPlatform.Android => TextureProcessorOutputFormat.EtcCompressed,
                    _ => TextureProcessorOutputFormat.DxtCompressed
                };
            }
           
            if (IsCompressedTextureFormat(format))
            {
                // Make sure the target platform supports the selected texture compression format
                if (platform == TargetPlatform.iOS)
                {
                    if (format != TextureProcessorOutputFormat.PvrCompressed)
                        throw new PlatformNotSupportedException("iOS platform only supports PVR texture compression");
                }
                else if (   platform == TargetPlatform.Windows ||
                            platform == TargetPlatform.WindowsDX12 ||
                            platform == TargetPlatform.DesktopGL ||
                            platform == TargetPlatform.DesktopVK ||
                            platform == TargetPlatform.MacOSX ||
                            platform == TargetPlatform.NativeClient ||
                            platform == TargetPlatform.Web)
                {
                    if (format != TextureProcessorOutputFormat.DxtCompressed)
                        throw new PlatformNotSupportedException(platform + " platform only supports DXT texture compression");
                }
            }

            return format;
        }

        public override void Requirements(ContentProcessorContext context, TextureProcessorOutputFormat format, out bool requiresPowerOfTwo, out bool requiresSquare)
        {
            if (format == TextureProcessorOutputFormat.Compressed)
                format = GetTextureFormatForPlatform(format, context.TargetPlatform);

            // Does it require POT textures?
            switch (format)
            {
                default:
                    requiresPowerOfTwo = false;
                    break;

                case TextureProcessorOutputFormat.DxtCompressed:
                    requiresPowerOfTwo = context.TargetProfile == GraphicsProfile.Reach;
                    break;

#pragma warning disable CS0618 // Type or member is obsolete
                case TextureProcessorOutputFormat.Etc1Compressed:
#pragma warning restore CS0618 // Type or member is obsolete

                case TextureProcessorOutputFormat.PvrCompressed:
                case TextureProcessorOutputFormat.EtcCompressed:
                    requiresPowerOfTwo = true;
                    break;
            }

            // Does it require square textures?
            requiresSquare = format switch
            {
                TextureProcessorOutputFormat.PvrCompressed => true,
                _ => false
            };
        }

        protected override void PlatformCompressTexture(ContentProcessorContext context, TextureContent content, TextureProcessorOutputFormat format, bool isSpriteFont)
        {
            format = GetTextureFormatForPlatform(format, context.TargetPlatform);

            // Make sure we're in a floating point format
            content.ConvertBitmapType(typeof(PixelBitmapContent<Vector4>));

            switch (format)
            {
                case TextureProcessorOutputFormat.AtcCompressed:
                    GraphicsUtil.CompressAti(context, content, isSpriteFont);
                    break;

                case TextureProcessorOutputFormat.AstcCompressed:
                case TextureProcessorOutputFormat.AstcCompressed4x4:
                case TextureProcessorOutputFormat.AstcCompressed5x5:
                case TextureProcessorOutputFormat.AstcCompressed6x6:
                case TextureProcessorOutputFormat.AstcCompressed8x8:
                case TextureProcessorOutputFormat.AstcCompressed10x10:
                case TextureProcessorOutputFormat.AstcCompressed12x12:
                    GraphicsUtil.CompressAstc(context, content, isSpriteFont, format);
                    break;

                case TextureProcessorOutputFormat.Color16Bit:
                    GraphicsUtil.CompressColor16Bit(context, content);
                    break;

                case TextureProcessorOutputFormat.DxtCompressed:
                    GraphicsUtil.CompressDxt(context, content, isSpriteFont);
                    break;

#pragma warning disable CS0618 // Type or member is obsolete
                case TextureProcessorOutputFormat.Etc1Compressed:
#pragma warning restore CS0618 // Type or member is obsolete
                    GraphicsUtil.CompressEtc1(context, content, isSpriteFont);
                    break;

                case TextureProcessorOutputFormat.EtcCompressed:
                    GraphicsUtil.CompressEtc(context, content, isSpriteFont);
                    break;

                case TextureProcessorOutputFormat.PvrCompressed:
                    GraphicsUtil.CompressPvrtc(context, content, isSpriteFont);
                    break;
            }
        }
    }
}
