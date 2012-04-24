/******************************************************************************

 @File         PVRTextureDefines.h

 @Title        

 @Version      

 @Copyright    Copyright (c) Imagination Technologies Limited. All Rights Reserved. Strictly Confidential.

 @Platform     

 @Description  

******************************************************************************/
#ifndef _PVRTEXTURE_DEFINES_H
#define _PVRTEXTURE_DEFINES_H

//To use the PVRTexLib .dll on Windows, you need to define _WINDLL_IMPORT
#ifndef PVR_DLL
#ifdef _WINDLL_EXPORT
#define PVR_DLL __declspec(dllexport)
//Forward declaration of PVRTexture Header and CPVRTMap. This exports their interfaces for DLLs.
struct PVR_DLL PVRTextureHeaderV3;
template <typename KeyType, typename DataType>
class PVR_DLL CPVRTMap;
template<typename T>
class PVR_DLL CPVRTArray;
#elif _WINDLL_IMPORT
#define PVR_DLL __declspec(dllimport)
//Forward declaration of PVRTexture Header and CPVRTMap. This exports their interfaces for DLLs.
struct PVR_DLL PVRTextureHeaderV3;
template <typename KeyType, typename DataType>
class PVR_DLL CPVRTMap;
template<typename T>
class PVR_DLL CPVRTArray;
#else
#define PVR_DLL
#endif
#endif


#include "PVRTTexture.h"

namespace pvrtexture
{
	/*****************************************************************************
	* Type defines for standard variable sizes.
	*****************************************************************************/
	typedef	signed char			int8;
	typedef	signed short		int16;
	typedef	signed int			int32;
	typedef	signed long long    int64;
	typedef unsigned char		uint8;
	typedef unsigned short		uint16;
	typedef unsigned int		uint32;
	typedef	unsigned long long	uint64;
	
	/*****************************************************************************
	* Texture related constants and enumerations. 
	*****************************************************************************/
	enum ECompressorQuality
	{
		ePVRTCFast=0,
		ePVRTCNormal,
		ePVRTCHigh,
		ePVRTCBest,
		eNumPVRTCModes,

		eETCFast=0,
		eETCFastPerceptual,
		eETCMedium,
		eETCMediumPerceptual,
		eETCSlow,
		eETCSlowPerceptual,
		eNumETCModes
	};

	enum EResizeMode
	{
		eResizeNearest,
		eResizeLinear,
		eResizeCubic,
		eNumResizeModes
	};

	// Legacy - API enums.
	enum ELegacyApi
	{
		eOGLES=1,
		eOGLES2,
		eD3DM,
		eOGL,
		eDX9,
		eDX10,
		eOVG,
		eMGL,
	};

	/*****************************************************************************
	* Useful macros.
	*****************************************************************************/
	#define TEXOFFSET2D(x,y,width) ( ((x)+(y)*(width)) )
	#define TEXOFFSET3D(x,y,z,width,height) ( ((x)+(y)*(width)+(z)*(width)*(height)) )
};
#endif //_PVRTEXTURE_DEFINES_H
