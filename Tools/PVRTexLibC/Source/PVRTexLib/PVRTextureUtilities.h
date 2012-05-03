/******************************************************************************

 @File         PVRTextureUtilities.h

 @Title        

 @Version      

 @Copyright    Copyright (c) Imagination Technologies Limited. All Rights Reserved. Strictly Confidential.

 @Platform     

 @Description  

******************************************************************************/
#ifndef _PVRTEXTURE_UTILITIES_H
#define _PVRTEXTURE_UTILITIES_H

#include "PVRTextureFormat.h"
#include "PVRTexture.h"

namespace pvrtexture
{
	/*!***********************************************************************
	 @Function		Resize
	 @Input			sTexture
	 @Input			u32NewWidth
	 @Input			u32NewHeight
	 @Input			u32NewDepth
	 @Input			eResizeMode
	 @Return		bool Whether the method succeeds or not.
	 @Description	Resizes the texture to new specified dimensions. Filtering 
					mode is specified with "eResizeMode".
	*************************************************************************/
	bool PVR_DLL Resize(CPVRTexture& sTexture, const uint32& u32NewWidth, const uint32& u32NewHeight, const uint32& u32NewDepth, const EResizeMode eResizeMode);

	/*!***********************************************************************
	 @Function		Rotate90
	 @Input			sTexture
	 @Input			eRotationAxis
	 @Input			bForward
	 @Return		bool Whether the method succeeds or not.
	 @Description	Rotates a texture by 90 degrees around the given axis. bForward controls direction of rotation.
	*************************************************************************/
	bool PVR_DLL Rotate90(CPVRTexture& sTexture, const EPVRTAxis eRotationAxis, const bool bForward);

	/*!***********************************************************************
	 @Function		Flip
	 @Input			sTexture
	 @Input			eFlipDirection
	 @Return		bool Whether the method succeeds or not.
	 @Description	Flips a texture in a given direction.
	*************************************************************************/
	bool PVR_DLL Flip(CPVRTexture& sTexture, const EPVRTAxis eFlipDirection);

	/*!***********************************************************************
	 @Function		Border
	 @Input			sTexture
	 @Input			uiBorderX
	 @Input			uiBorderY
	 @Input			uiBorderZ
	 @Return		bool Whether the method succeeds or not.
	 @Description	Adds a user specified border to the texture.
	*************************************************************************/
	bool PVR_DLL Border(CPVRTexture& sTexture, uint32 uiBorderX, uint32 uiBorderY, uint32 uiBorderZ);

	/*!***********************************************************************
	 @Function		PreMultiplyAlpha
	 @Input			sTexture
	 @Return		bool Whether the method succeeds or not.
	 @Description	Pre-multiplies a texture's colours by its alpha values.
	*************************************************************************/
	bool PVR_DLL PreMultiplyAlpha(CPVRTexture& sTexture);

	/*!***********************************************************************
	 @Function		Bleed
	 @Input			sTexture
	 @Return		bool Whether the method succeeds or not.
	 @Description	Allows a texture's colours to run into any fully transparent areas.
	*************************************************************************/
	bool PVR_DLL Bleed(CPVRTexture& sTexture);

	/*!***********************************************************************
	 @Function		SetChannels
	 @Input			sTexture
	 @Input			uiNumChannelSets
	 @Input			eChannels
	 @Input			pValues
	 @Return		bool Whether the method succeeds or not.
	 @Description	Sets the specified number of channels to values specified in pValues.
	 *************************************************************************/
	bool PVR_DLL SetChannels(CPVRTexture& sTexture, uint32 uiNumChannelSets, EChannelName *eChannels, uint32 *pValues);
	bool PVR_DLL SetChannelsFloat(CPVRTexture& sTexture, uint32 uiNumChannelSets, EChannelName *eChannels, float *pValues);

	/*!***********************************************************************
	 @Function		CopyChannels
	 @Input			sTexture
	 @Input			sTextureSource
	 @Input			uiNumChannelCopies
	 @Input			eChannels
	 @Input			eChannelsSource
	 @Return		bool Whether the method succeeds or not.
	 @Description	Copies the specified channels from sTextureSource into sTexture. 
					sTextureSource is not modified so it is possible to use the
					same texture as both input and output. When using the same 
					texture as source and destination, channels are preserved
					between swaps (e.g. copying Red to Green and then Green to Red
					will result in the two channels trading places correctly).
					Channels in eChannels are set to the value of the channels 
					in eChannelSource.
	*************************************************************************/
	bool PVR_DLL CopyChannels(CPVRTexture& sTexture, const CPVRTexture& sTextureSource, uint32 uiNumChannelCopies, EChannelName *eChannels, EChannelName *eChannelsSource);

	/*!***********************************************************************
	 @Function		GenerateNormalMap
	 @Input			sTexture
	 @Input			fScale
	 @Input			sChannelOrder
	 @Return		bool Whether the method succeeds or not.
	 @Description	Generates a Normal Map from a given height map.
					Assumes the red channel has the height values.
					By default outputs to red/green/blue = x/y/z,
					this can be overridden by specifying a channel
					order in sChannelOrder. The channels specified
					will output to red/green/blue/alpha in that order.
					So "xyzh" maps x to red, y to green, z to blue
					and h to alpha. 'h' is used to specify that the
					original height map data should be preserved in
					the given channel.
	*************************************************************************/
	bool PVR_DLL GenerateNormalMap(CPVRTexture& sTexture, const float fScale, CPVRTString sChannelOrder);

	/*!***********************************************************************
	 @Function		GenerateMIPMaps
	 @Input			sTexture
	 @Input			eFilterMode
	 @Input			uiMIPMapsToDo
	 @Return		bool Whether the method succeeds or not.
	 @Description	Generates MIPMaps for a source texture. Default is to
					create a complete MIPMap chain, however this can be
					overridden with uiMIPMapsToDo.
	*************************************************************************/
	bool PVR_DLL GenerateMIPMaps(CPVRTexture& sTexture, const EResizeMode eFilterMode, const uint32 uiMIPMapsToDo=PVRTEX_ALLMIPLEVELS);
	
	/*!***********************************************************************
	 @Function		ColourMIPMaps
	 @Input			sTexture
	 @Return		bool Whether the method succeeds or not.
	 @Description	Colours a texture's MIPMap levels with artificial colours 
					for debugging. MIP levels are coloured in the order:
					Red, Green, Blue, Cyan, Magenta and Yellow
					in a repeating pattern.
	*************************************************************************/
	bool PVR_DLL ColourMIPMaps(CPVRTexture& sTexture);
	
	/*!***********************************************************************
	 @Function		Transcode
	 @Input			sTexture
	 @Input			ptFormat
	 @Input			eChannelType
	 @Input			eColourspace
	 @Input			eQuality
	 @Input			bDoDither
	 @Return		bool Whether the method succeeds or not.
	 @Description	Transcodes a texture from its original format into a newly specified format.
					Will either quantise or dither to lower precisions based on bDoDither.
					uiQuality specifies the quality for PVRTC and ETC compression.					
	*************************************************************************/
	bool PVR_DLL Transcode(CPVRTexture& sTexture, const PixelType ptFormat, const EPVRTVariableType eChannelType, const EPVRTColourSpace eColourspace, const ECompressorQuality eQuality=ePVRTCNormal, const bool bDoDither=false);
};
#endif //_PVRTEXTURE_UTILTIES_H
