﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public abstract class TextureProfile
    {
        private static readonly LoadedTypeCollection<TextureProfile> _profiles = new LoadedTypeCollection<TextureProfile>();

        /// <summary>
        /// Find the profile for this target platform.
        /// </summary>
        /// <param name="platform">The platform target for textures.</param>
        /// <returns></returns>
        public static TextureProfile ForPlatform(TargetPlatform platform)
        {
            var profile = _profiles.FirstOrDefault(h => h.Supports(platform));
            if (profile != null)
                return profile;

            throw new PipelineException("There is no supported texture profile for the '" + platform + "' platform!");
        }

        /// <summary>
        /// Returns true if this profile supports texture processing for this platform.
        /// </summary>
        public abstract bool Supports(TargetPlatform platform);

        /// <summary>
        /// Determines if the texture format will require power-of-two dimensions and/or equal width and height.
        /// </summary>
        /// <param name="context">The processor context.</param>
        /// <param name="format">The desired texture format.</param>
        /// <param name="requiresPowerOfTwo">True if the texture format requires power-of-two dimensions.</param>
        /// <param name="requiresSquare">True if the texture format requires equal width and height.</param>
        /// <returns>True if the texture format requires power-of-two dimensions.</returns>
        public abstract void Requirements(ContentProcessorContext context, TextureProcessorOutputFormat format, out bool requiresPowerOfTwo, out bool requiresSquare);

        /// <summary>
        /// Performs conversion of the texture content to the correct format.
        /// </summary>
        /// <param name="context">The processor context.</param>
        /// <param name="content">The content to be compressed.</param>
        /// <param name="format">The user requested format for compression.</param>
        /// <param name="generateMipmaps">If mipmap generation is required.</param>
        /// <param name="sharpAlpha">If the texture has sharp alpha cutouts.</param>
        public void ConvertTexture(ContentProcessorContext context, TextureContent content, TextureProcessorOutputFormat format, bool generateMipmaps, bool sharpAlpha)
        {
            // We do nothing in this case.
            if (format == TextureProcessorOutputFormat.NoChange)
                return;

            // If this is color just make sure the format is right and return it.
            if (format == TextureProcessorOutputFormat.Color)
            {
                content.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
                if (generateMipmaps)
                    content.GenerateMipmaps(false);
                return;
            }

            // Handle this common compression format.
            if (format == TextureProcessorOutputFormat.Color16Bit)
            {
                GraphicsUtil.CompressColor16Bit(content, generateMipmaps);
                return;
            }

            try
            {
                // All other formats require platform specific choices.
                PlatformCompressTexture(context, content, format, generateMipmaps, false);
            }
            catch (EntryPointNotFoundException ex)
            {
                context.Logger.LogImportantMessage("Could not find the entry point to compress the texture. " + ex.ToString());
                throw ex;
            }
            catch (DllNotFoundException ex)
            {
                context.Logger.LogImportantMessage("Could not compress texture. Required shared lib is missing. " + ex.ToString());
                throw ex;
            }
            catch (Exception ex)
            {
                context.Logger.LogImportantMessage("Could not convert texture. " + ex.ToString());
                throw ex;
            }
        }

        protected abstract void PlatformCompressTexture(ContentProcessorContext context, TextureContent content, TextureProcessorOutputFormat format, bool generateMipmaps, bool sharpAlpha);
    }
}
