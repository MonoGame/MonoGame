using System;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using MonoGameContentProcessors.Content;

namespace MonoGameContentProcessors.Processors
{
    [ContentProcessor(DisplayName = "MonoGame Texture")]
    public class MGTextureProcessor : TextureProcessor
    {
        [DllImport("PVRTexLibC.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CompressTexture(byte[] data, int height, int width, int mipLevels, bool preMultiplied, bool pvrtc4bppCompression, ref IntPtr dataSizes);

        private MGCompressionMode compressionMode = MGCompressionMode.PVRTCFourBitsPerPixel;

        [DisplayName("Compression Mode")]
        [Description("Specifies the type of compression to use, if any.")]
        [DefaultValue(MGCompressionMode.PVRTCFourBitsPerPixel)]
        public MGCompressionMode CompressionMode
        {
            get { return this.compressionMode; }
            set { this.compressionMode = value; }
        }

        public override TextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            // Fallback if we aren't buiding for iOS.
            var platform = ContentHelper.GetMonoGamePlatform();
            if (platform != MonoGamePlatform.iOS)
                return base.Process(input, context);

            // Only go this path if we are compressing the texture
            if (TextureFormat != TextureProcessorOutputFormat.DxtCompressed)
                return base.Process(input, context);

            // TODO: Reflector ResizeToPowerOfTwo(TextureContent tex)
            // Resize the first face and let mips get generated from the dll.
            /*if (ResizeToPowerOfTwo)
            {

            }*/

            var height = input.Faces[0][0].Height;
            var width = input.Faces[0][0].Width;
            var mipLevels = 1;

            var invalidBounds = height != width || !(isPowerOfTwo(height) && isPowerOfTwo(width));

            // Only PVR compress square, power of two textures.
            if (invalidBounds || compressionMode == MGCompressionMode.NoCompression)
            {
                if (compressionMode != MGCompressionMode.NoCompression)
                {
                    context.Logger.LogImportantMessage("WARNING: PVR Texture {0} must be a square, power of two texture. Skipping Compression.",
                                                        Path.GetFileName(context.OutputFilename));
                }

                // Skip compressing this texture and process it normally.
                this.TextureFormat = TextureProcessorOutputFormat.Color;

                return base.Process(input, context);
            }

            // Calculate how many mip levels will be created, and pass that to our DLL.
            if (GenerateMipmaps)
            {
                while (height != 1 || width != 1)
                {
                    height = Math.Max(height / 2, 1);
                    width = Math.Max(width / 2, 1);
                    mipLevels++;
                }
            }

            ConvertToPVRTC(input, mipLevels, PremultiplyAlpha, compressionMode);

            return input;
        }

        public static void ConvertToPVRTC(TextureContent sourceContent, int mipLevels, bool premultipliedAlpha, MGCompressionMode bpp)
        {
            IntPtr dataSizesPtr = IntPtr.Zero;

            var texDataPtr = CompressTexture(sourceContent.Faces[0][0].GetPixelData(), 
                                            sourceContent.Faces[0][0].Height, 
                                            sourceContent.Faces[0][0].Width, 
                                            mipLevels, 
                                            premultipliedAlpha,
                                            bpp == MGCompressionMode.PVRTCFourBitsPerPixel,
                                            ref dataSizesPtr);

            // Store the size of each mipLevel
            var dataSizesArray = new int[mipLevels];
            Marshal.Copy(dataSizesPtr, dataSizesArray, 0, dataSizesArray.Length);

            var levelSize = 0;
            byte[] levelData;
            var sourceWidth = sourceContent.Faces[0][0].Width;
            var sourceHeight = sourceContent.Faces[0][0].Height;

            // Set the pixel data for each mip level.
            sourceContent.Faces[0].Clear();

            for (int x = 0; x < mipLevels; x++)
            {
                levelSize = dataSizesArray[x];
                levelData = new byte[levelSize];

                Marshal.Copy(texDataPtr, levelData, 0, levelSize);

                var levelWidth = Math.Max(sourceWidth  >> x, 1);
                var levelHeight = Math.Max(sourceHeight >> x, 1);

                sourceContent.Faces[0].Add(new MGBitmapContent(levelData, levelWidth, levelHeight, bpp));

                texDataPtr = IntPtr.Add(texDataPtr, levelSize);
            }
        }

        private bool isPowerOfTwo(int x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }
    }


}