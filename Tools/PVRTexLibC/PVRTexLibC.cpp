// This is the main DLL file.

#include "stdafx.h"

#include "PVRTexLibC.h"
#include "../../ThirdParty/Libs/PVRTexLib/PVRTexture.h"
#include "../../ThirdParty/Libs/PVRTexLib/PVRTextureUtilities.h"

using namespace pvrtexture;

extern void* __cdecl CompressTexture(unsigned char* data, int height, int width, int mipLevels, bool preMultiplied, bool fourbppCompression, int** dataSizes)
{   
   PVRTextureHeaderV3 pvrHeader; 

   pvrHeader.u32Version = PVRTEX_CURR_IDENT; 
   pvrHeader.u32Flags = preMultiplied ? PVRTEX3_PREMULTIPLIED : 0; 
   pvrHeader.u64PixelFormat = PVRStandard8PixelType.PixelTypeID; 
   pvrHeader.u32ColourSpace = ePVRTCSpacelRGB; 
   pvrHeader.u32ChannelType = ePVRTVarTypeUnsignedByteNorm; 
   pvrHeader.u32Height = height; 
   pvrHeader.u32Width = width; 
   pvrHeader.u32Depth = 1; 
   pvrHeader.u32NumSurfaces = 1; 
   pvrHeader.u32NumFaces = 1; 
   pvrHeader.u32MIPMapCount = 1; 
   pvrHeader.u32MetaDataSize = 0;

   CPVRTexture sTexture( pvrHeader, data );

   if (mipLevels > 1)
      GenerateMIPMaps(sTexture, eResizeLinear, mipLevels);

   auto bpp =  fourbppCompression ? ePVRTPF_PVRTCI_4bpp_RGBA : ePVRTPF_PVRTCI_2bpp_RGBA;

   PixelType pixType(bpp);
   Transcode(sTexture,
            pixType, 
            ePVRTVarTypeUnsignedByteNorm,
            ePVRTCSpacelRGB );

   *dataSizes = new int[mipLevels];

   for(int x = 0; x < mipLevels; x++)
     (*dataSizes)[x] = sTexture.getDataSize(x);

   int totalDataSize = sTexture.getDataSize();
   auto returnData = new unsigned char[totalDataSize];

   memcpy(returnData, sTexture.getDataPtr(), totalDataSize);
   
   return returnData;
}
