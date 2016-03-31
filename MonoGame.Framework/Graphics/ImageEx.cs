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
        internal static Image RGBToBGR(this Image bmp)
        {
            Image newBmp;
            if ((bmp.PixelFormat & Imaging.PixelFormat.Indexed) != 0)
            {
                newBmp = new Bitmap(bmp.Width, bmp.Height, Imaging.PixelFormat.Format32bppArgb);
            }
            else
            {
                // Need to clone so the call to Clear() below doesn't clear the source before trying to draw it to the target.
                newBmp = (Image)bmp.Clone();
            }
        
            try
            {
                System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();
                System.Drawing.Imaging.ColorMatrix cm = new System.Drawing.Imaging.ColorMatrix(rgbtobgr);

                ia.SetColorMatrix(cm);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBmp))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImage(bmp, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, bmp.Width, bmp.Height, System.Drawing.GraphicsUnit.Pixel, ia);
                }
            }
            finally
            {
                if (newBmp != bmp)
                {
                    bmp.Dispose();
                }
            }
            
            return newBmp;
        }
#endif

    }
}
