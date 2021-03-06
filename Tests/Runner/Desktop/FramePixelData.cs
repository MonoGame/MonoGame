using System;
using System.IO;
using Microsoft.Xna.Framework;
using StbImageSharp;
using StbImageWriteSharp;

namespace MonoGame.Tests
{
    partial class FramePixelData
    {
        public static unsafe FramePixelData FromFile(string filename)
        {
            var result = ImageResult.FromMemory(File.ReadAllBytes(filename), StbImageSharp.ColorComponents.RedGreenBlueAlpha);
			var frame = new FramePixelData(result.Width, result.Height);
			
			fixed (byte* b = &result.Data[0])
            {
                for (var i = 0; i < result.Data.Length; i += 4)
                {
					frame.Data[i / 4] = new Color(b[i + 0], b[i + 1], b[i + 2], b[i + 3]);
                }
            }
            
			return frame;
        }

        public unsafe void Save(string filename, string attachmentDescription = null)
        {
			using (var stream = new FileStream(filename, FileMode.Create))
			{
				fixed (Color* ptr = &Data[0])
				{
					var writer = new ImageWriter();
					writer.WriteBmp(ptr, Width, Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
				}
			}

            if (attachmentDescription != null)
            {
                NUnit.Framework.TestContext.AddTestAttachment(filename, attachmentDescription);
            }
        }
    }
}
