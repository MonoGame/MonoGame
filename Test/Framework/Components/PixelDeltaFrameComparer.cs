// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;


namespace MonoGame.Tests.Components 
{
	class PixelDeltaFrameComparer : IFrameComparer 
    {
        private static int[] GreyScale(FramePixelData pixelData, int newWidth, int newHeight)
        {
            var resized = new int[newWidth * newHeight];

            var stepX = pixelData.Width / (float)newWidth;
            var stepY = pixelData.Height / (float)newHeight;

            for (var y = 0; y < newHeight; y++)
            {
                for (var x = 0; x < newWidth; x++)
                {
                    // We use a simple nearest neighbor sample as any blending
                    // would only serve to increase the differences between the images.

                    var sx = (int)(x * stepX);
                    var sy = (int)(y * stepY);
                    var si = sx + (sy * pixelData.Width);
                    var scolor = pixelData.Data[si];
                    
                    // Convert to a greyscale value which removes small
                    // color differences that the eye cannot spot.
                    var grayScale = (int)((scolor.R * 0.3) + (scolor.G * 0.59) + (scolor.B * 0.11));

                    var di = x + (y * newWidth);
                    resized[di] = grayScale;
                }
            }

            return resized;
        }

        public float Compare(FramePixelData image, FramePixelData referenceImage)
		{        
            // Conver the images down to a common sized greyscale image.
            var width = Math.Min(image.Width, referenceImage.Width);
            var height = Math.Min(image.Height, referenceImage.Height);
            var img = GreyScale(image, width, height);
            var imgRef = GreyScale(referenceImage, width, height);

            // Find the differences between the greyscale images.
            var absDiff = new int[width * height];
            for (var i = 0; i < absDiff.Length; i++)
                absDiff[i] = Math.Abs(img[i] - imgRef[i]);

            // Find all the differences over the threshold.
		    const int threshold = 3;
            var diffPixels = 0;
            for (var i = 0; i < absDiff.Length; i++)
            {
                if (absDiff[i] > threshold) 
                    diffPixels++;
            }

            // Calculate the difference percentage.
            var diff = diffPixels / (float)absDiff.Length;
		    return 1.0f - diff;
		}
	}
}
