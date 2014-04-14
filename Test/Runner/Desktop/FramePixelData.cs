using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace MonoGame.Tests {
	partial class FramePixelData {
		public static unsafe FramePixelData FromFile (string filename)
		{
			using (Bitmap bitmap = (Bitmap) Bitmap.FromFile (filename)) {
				var frame = new FramePixelData (bitmap.Width, bitmap.Height);

				var bitmapData = bitmap.LockBits (
					new Rectangle (0, 0, bitmap.Width, bitmap.Height),
					ImageLockMode.ReadOnly,
					PixelFormat.Format32bppArgb);
				try {
					byte* pSourceRow = (byte*) bitmapData.Scan0;
					int indexFrame = 0;
					for (int y = 0; y < frame.Height; ++y) {
						PixelArgb* pPixel = (PixelArgb*) pSourceRow;
						for (int x = 0; x < frame.Width; ++x) {
							frame.Data [indexFrame] = new Microsoft.Xna.Framework.Color (
								pPixel->R, pPixel->G, pPixel->B, pPixel->A);

							indexFrame++;
							pPixel++;
						}
						pSourceRow += bitmapData.Stride;
					}
				} finally {
					bitmap.UnlockBits (bitmapData);
				}
				return frame;
			}
		}

		public unsafe void Save (string filename)
		{
			using (var bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb)) {
				var bitmapData = bitmap.LockBits (
					new Rectangle (0, 0, bitmap.Width, bitmap.Height),
					ImageLockMode.WriteOnly,
					PixelFormat.Format32bppArgb);

				try {
					byte* pDestRow = (byte*) bitmapData.Scan0;
					int indexFrame = 0;
					for (int y = 0; y < Height; ++y) {
						PixelArgb* pPixel = (PixelArgb*) pDestRow;
						for (int x = 0; x < Width; ++x) {
							pPixel->R = Data [indexFrame].R;
							pPixel->G = Data [indexFrame].G;
							pPixel->B = Data [indexFrame].B;
							pPixel->A = Data [indexFrame].A;

							indexFrame++;
							pPixel++;
						}
						pDestRow += bitmapData.Stride;
					}
				} finally {
					bitmap.UnlockBits (bitmapData);
				}

				bitmap.Save (filename);
			}
		}
	}
}
