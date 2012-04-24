// This is the main DLL file.

#include "stdafx.h"

#include "PVRTexLibC.h"
#include "PVRTexture.h"
#include "PVRTextureUtilities.h"

using namespace pvrtexture;

extern void* __cdecl CompressTexture(unsigned char* data, int height, int width, int mipLevels, bool preMultiplied, int** dataSizes)
{   
   CPVRTextureHeader texHeader(PVRStandard8PixelType.PixelTypeID,
							         height,
							         width,
							         1,
							         mipLevels,
							         1,
							         1,
							         ePVRTCSpacelRGB,
							         ePVRTVarTypeUnsignedByteNorm,
							         preMultiplied);

   CPVRTexture sTexture(texHeader, data);

   if (mipLevels > 1)
      GenerateMIPMaps(sTexture, eResizeLinear);

   PixelType PVRTC4BPP (ePVRTPF_PVRTCI_4bpp_RGBA);
   
   Transcode(sTexture,
            PVRTC4BPP, 
            ePVRTVarTypeUnsignedInteger,
            ePVRTCSpacelRGB);

   *dataSizes = new int[mipLevels];

   for(int x = 0; x < mipLevels; x++)
     (*dataSizes)[x] = sTexture.getDataSize(x);

   auto dataPtr = sTexture.getDataPtr();
   return dataPtr;
}