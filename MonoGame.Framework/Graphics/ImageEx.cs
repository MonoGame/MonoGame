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
        internal static void RGBToBGR(this Image bmp)
        {
            System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();
            System.Drawing.Imaging.ColorMatrix cm = new System.Drawing.Imaging.ColorMatrix(rgbtobgr);

            ia.SetColorMatrix(cm);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.DrawImage(bmp, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, bmp.Width, bmp.Height, System.Drawing.GraphicsUnit.Pixel, ia);
            }
        }
#endif

    }
}
