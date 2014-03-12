using System;

namespace System.Drawing
{
    internal static class ImageEx
    {

        // RGB to BGR convert Matrix
        private static float[][] rgbtobgr = new float[][]
	      {
		     new float[] {0, 0, 1, 0, 0},
		     new float[] {0, 1, 0, 0, 0},
		     new float[] {1, 0, 0, 0, 0},
		     new float[] {0, 0, 0, 1, 0},
		     new float[] {0, 0, 0, 0, 1}
	      };

#if WINRT

#else
        internal static void RGBToBGR(ref Bitmap bmp)
        {
            Bitmap bmpCopy;
            if (bmp.PixelFormat == Imaging.PixelFormat.Format8bppIndexed)
            {
                Bitmap bmpConvert = new Bitmap(bmp.Width, bmp.Height, Imaging.PixelFormat.Format32bppArgb);
                Graphics gr = Graphics.FromImage(bmpConvert);
                gr.DrawImage(bmp, 0, 0);
                gr.Dispose();

                bmpCopy = bmp;
                bmp = bmpConvert;
            }
            else
            {
                bmpCopy = (Bitmap)bmp.Clone();
            }

            try
            {
                System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();
                System.Drawing.Imaging.ColorMatrix cm = new System.Drawing.Imaging.ColorMatrix(rgbtobgr);

                ia.SetColorMatrix(cm);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImage(bmpCopy, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, bmp.Width, bmp.Height, System.Drawing.GraphicsUnit.Pixel, ia);
                }
            }
            finally
            {
                bmpCopy.Dispose();
            }
        }
#endif

    }
}
