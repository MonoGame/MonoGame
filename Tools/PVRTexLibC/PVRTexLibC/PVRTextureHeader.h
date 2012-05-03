/******************************************************************************

 @File         PVRTextureHeader.h

 @Title        

 @Version      

 @Copyright    Copyright (c) Imagination Technologies Limited. All Rights Reserved. Strictly Confidential.

 @Platform     

 @Description  

******************************************************************************/
#ifndef _PVRTEXTURE_HEADER_H
#define _PVRTEXTURE_HEADER_H

#include "PVRTextureDefines.h"
#include "PVRTextureFormat.h"
#include "PVRTString.h"
#include "PVRTMap.h"

namespace pvrtexture
{
	//Wrapper class for PVRTextureHeaderV3, adds 'smart' accessor functions.
	class PVR_DLL CPVRTextureHeader
	{	
	protected:
		PVRTextureHeaderV3											m_sHeader;		//Texture header as laid out in a file.
		mutable CPVRTMap<uint32, CPVRTMap<uint32,MetaDataBlock> >	m_MetaData;		//Map of all the meta data stored for a texture.

	public:
	/*******************************************************************************
	* Construction methods for a texture header.
	*******************************************************************************/
		/*!***********************************************************************
		 @Function		CPVRTextureHeader
		 @Return		CPVRTextureHeader A new texture header.
		 @Description	Default constructor for a CPVRTextureHeader. Returns an empty header.
		*************************************************************************/
		CPVRTextureHeader();

		/*!***********************************************************************
		 @Function		CPVRTextureHeader
		 @Input			fileHeader
		 @Input			metaDataCount
		 @Input			metaData
		 @Return		CPVRTextureHeader A new texture header.
		 @Description	Creates a new texture header from a PVRTextureHeaderV3, 
						and appends Meta data if any is supplied.
		*************************************************************************/
		CPVRTextureHeader(	PVRTextureHeaderV3	fileHeader,
							uint32				metaDataCount=0,
							MetaDataBlock*		metaData=NULL);

		/*!***********************************************************************
		 @Function		CPVRTextureHeader
		 @Input			u64PixelFormat
		 @Input			u32Height
		 @Input			u32Width
		 @Input			u32Depth
		 @Input			u32NumMipMaps
		 @Input			u32NumArrayMembers
		 @Input			u32NumFaces
		 @Input			eColourSpace
		 @Input			eChannelType
		 @Input			bPreMultiplied
		 @Return		CPVRTextureHeader A new texture header.
		 @Description	Creates a new texture header based on individual header
						variables.
		*************************************************************************/
		CPVRTextureHeader(	uint64				u64PixelFormat,
							uint32				u32Height=1,
							uint32				u32Width=1,
							uint32				u32Depth=1,
							uint32				u32NumMipMaps=1,
							uint32				u32NumArrayMembers=1,
							uint32				u32NumFaces=1,
							EPVRTColourSpace	eColourSpace=ePVRTCSpacelRGB,
							EPVRTVariableType	eChannelType=ePVRTVarTypeUnsignedByteNorm,
							bool				bPreMultiplied=false);

		/*!***********************************************************************
		 @Function		operator=
		 @Input			rhs
		 @Return		CPVRTextureHeader& This header.
		 @Description	Will copy the contents and information of another header into this one.
		*************************************************************************/
		CPVRTextureHeader& operator=(const CPVRTextureHeader& rhs);
		
	/*******************************************************************************
	* Accessor Methods for a texture's properties - getters.
	*******************************************************************************/

		/*!***********************************************************************
		 @Function		getFileHeader
		 @Return		PVRTextureHeaderV3		The file header.
		 @Description	Gets the file header structure.
		*************************************************************************/
		const PVRTextureHeaderV3 getFileHeader() const;

		/*!***********************************************************************
		 @Function		getPixelType
		 @Return		PixelType		64-bit pixel type ID.
		 @Description	Gets the 64-bit pixel type ID of the texture.
		*************************************************************************/
		const PixelType getPixelType() const;

		/*!***********************************************************************
		 @Function		getBitsPerPixel
		 @Return		uint32		Number of bits per pixel
		 @Description	Gets the bits per pixel of the texture format.
		*************************************************************************/
		const uint32 getBitsPerPixel() const;

		/*!***********************************************************************
		 @Function		getColourSpace
		 @Return		EPVRTColourSpace	enum representing colour space.
		 @Description	Returns the colour space of the texture.
		*************************************************************************/
		const EPVRTColourSpace getColourSpace() const;

		/*!***********************************************************************
		 @Function		getChannelType
		 @Return		EPVRTVariableType	enum representing the type of the texture.
		 @Description	Returns the variable type that the texture's data is stored in.
		*************************************************************************/
		const EPVRTVariableType getChannelType() const;

		/*!***********************************************************************
		 @Function		getWidth
		 @Input				uiMipLevel	MIP level that user is interested in.
		 @Return		uint32		Width of the specified MIP-Map level.
		 @Description	Gets the width of the user specified MIP-Map 
						level for the texture
		*************************************************************************/
		const uint32 getWidth(uint32 uiMipLevel=PVRTEX_TOPMIPLEVEL) const;

		/*!***********************************************************************
		 @Function		getHeight
		 @Input				uiMipLevel	MIP level that user is interested in.
		 @Return		uint32		Height of the specified MIP-Map level.
		 @Description	Gets the height of the user specified MIP-Map 
						level for the texture
		*************************************************************************/
		const uint32 getHeight(uint32 uiMipLevel=PVRTEX_TOPMIPLEVEL) const;

		/*!***********************************************************************
		 @Function		getDepth
		 @Input				uiMipLevel	MIP level that user is interested in.
		 @Return		Depth of the specified MIP-Map level.
		 @Description	Gets the depth of the user specified MIP-Map 
						level for the texture
		*************************************************************************/
		const uint32 getDepth(uint32 uiMipLevel=PVRTEX_TOPMIPLEVEL) const;

		/*!***********************************************************************
		 @Function		getTextureSize
		 @Input				iMipLevel		Specifies a MIP level to check, 
										'PVRTEX_ALLMIPLEVELS' can be passed to get 
										the size of all MIP levels. 
		 @Input				bAllSurfaces	Size of all surfaces is calculated if true, 
										only a single surface if false.
		 @Input				bAllFaces		Size of all faces is calculated if true, 
										only a single face if false.
		 @Return		uint32			Size in PIXELS of the specified texture area.
		 @Description	Gets the size in PIXELS of the texture, given various input 
						parameters.	User can retrieve the total size of either all 
						surfaces or a single surface, all faces or a single face and
						all MIP-Maps or a single specified MIP level. All of these
		*************************************************************************/
		const uint32 getTextureSize(int32 iMipLevel=PVRTEX_ALLMIPLEVELS, bool bAllSurfaces = true, bool bAllFaces = true) const;

		/*!***********************************************************************
		 @Function		getDataSize
		 @Input				iMipLevel		Specifies a mip level to check, 
										'PVRTEX_ALLMIPLEVELS' can be passed to get 
										the size of all MIP levels. 
		 @Input				bAllSurfaces	Size of all surfaces is calculated if true, 
										only a single surface if false.
		 @Input				bAllFaces		Size of all faces is calculated if true, 
										only a single face if false.
		 @Return		uint32			Size in BYTES of the specified texture area.
		 @Description	Gets the size in BYTES of the texture, given various input 
						parameters.	User can retrieve the size of either all 
						surfaces or a single surface, all faces or a single face 
						and all MIP-Maps or a single specified MIP level.
		*************************************************************************/
		const uint32 getDataSize(int32 iMipLevel=PVRTEX_ALLMIPLEVELS, bool bAllSurfaces = true, bool bAllFaces = true) const;

		/*!***********************************************************************
		 @Function		getNumArrayMembers
		 @Return		uint32		Number of array members in this texture.
		 @Description	Gets the number of array members stored in this texture.
		*************************************************************************/
		const uint32 getNumArrayMembers() const;

		/*!***********************************************************************
		 @Function		getNumMIPLevels
		 @Return		uint32		Number of MIP-Map levels in this texture.
		 @Description	Gets the number of MIP-Map levels stored in this texture.
		*************************************************************************/
		const uint32 getNumMIPLevels() const;

		/*!***********************************************************************
		 @Function		getNumFaces
		 @Return		uint32		Number of faces in this texture.
		 @Description	Gets the number of faces stored in this texture.
		*************************************************************************/
		const uint32 getNumFaces() const;

		/*!***********************************************************************
		 @Function		getOrientation
		 @Input				axis			EPVRTAxis type specifying the axis to examine.
		 @Return		EPVRTOrientation	Enum orientation of the axis.
		 @Description	Gets the data orientation for this texture.
		*************************************************************************/
		const EPVRTOrientation getOrientation(EPVRTAxis axis) const;

		/*!***********************************************************************
		 @Function		isFileCompressed
		 @Return		bool	True if it is file compressed.
		 @Description	Returns whether or not the texture is compressed using
						PVRTexLib's FILE compression - this is independent of 
						any texture compression.
		*************************************************************************/
		const bool isFileCompressed() const;
				
		/*!***********************************************************************
		 @Function		isPreMultiplied
		 @Return		bool	True if texture is premultiplied.
		 @Description	Returns whether or not the texture's colour has been
						pre-multiplied by the alpha values.
		*************************************************************************/
		const bool isPreMultiplied() const;

		/*!***********************************************************************
		 @Function		getMetaDataSize
		 @Return		const uint32 Size, in bytes, of the meta data stored in the header.
		 @Description	Returns the total size of the meta data stored in the header. 
						This includes the size of all information stored in all MetaDataBlocks.
		*************************************************************************/
		const uint32 getMetaDataSize() const;

		/*!***********************************************************************
		@Function		getOGLFormat
		@Modified		internalformat
		@Modified		format
		@Modified		type
		@Description	Gets the OpenGL equivalent values of internal format, format
						and type for this texture. This will return any supported
						OpenGL texture values, it is up to the user to decide if 
						these are valid for their current platform.
		*************************************************************************/
		const void getOGLFormat(uint32& internalformat, uint32& format, uint32& type) const;

		/*!***********************************************************************
		 @Function		getOGLESFormat
		 @Modified		internalformat
		 @Modified		format
		 @Modified		type
		 @Description	Gets the OpenGLES equivalent values of internal format, 
						format and type for this texture. This will return any 
						supported OpenGLES texture values, it is up to the user 
						to decide if these are valid for their current platform.
		*************************************************************************/
		const void getOGLESFormat(uint32& internalformat, uint32& format, uint32& type) const;

		/*!***********************************************************************
		 @Function		getD3DFormat
		 @Return		const uint32 
		 @Description	Gets the D3DFormat (up to DirectX 9 and Direct 3D Mobile)
						equivalent values for this texture. This will return any 
						supported D3D texture formats, it is up to the user to
						decide if this is valid for their current platform.
		*************************************************************************/
		const uint32 getD3DFormat() const;
		
		/*!***********************************************************************
		 @Function		getDXGIFormat
		 @Return		const uint32 
		 @Description	Gets the DXGIFormat (DirectX 10 onward) equivalent values 
						for this texture. This will return any supported DX texture
						formats, it is up to the user to decide if this is valid 
						for their current platform.
		*************************************************************************/
		const uint32 getDXGIFormat() const;

	/*!***********************************************************************
	* Accessor Methods for a texture's properties - setters.
	*************************************************************************/

		/*!***********************************************************************
		 @Function		setPixelFormat
		 @Input				uPixelFormat	The format of the pixel.
		 @Description	Sets the pixel format for this texture.
		*************************************************************************/
		void setPixelFormat(PixelType uPixelFormat);

		/*!***********************************************************************
		 @Function		setColourSpace
		 @Input				eColourSpace	A colour space enum.
		 @Description	Sets the colour space for this texture. Default is lRGB.
		*************************************************************************/
		void setColourSpace(EPVRTColourSpace eColourSpace);

		/*!***********************************************************************
		 @Function		setChannelType
		 @Input				eVarType	A variable type enum.
		 @Description	Sets the variable type for the channels in this texture.
		*************************************************************************/
		void setChannelType(EPVRTVariableType eVarType);

		/*!***********************************************************************
		 @Function		setOGLFormat
		 @Input			internalformat
		 @Input			format
		 @Input			type
		 @Return		bool Whether the format is valid or not.
		 @Description	Sets the format of the texture to PVRTexLib's internal
						representation of the OGL format.
		*************************************************************************/
		bool setOGLFormat(const uint32& internalformat, const uint32& format, const uint32& type);

		/*!***********************************************************************
		 @Function		setOGLESFormat
		 @Input			internalformat
		 @Input			format
		 @Input			type
		 @Return		bool Whether the format is valid or not.
		 @Description	Sets the format of the texture to PVRTexLib's internal
						representation of the OGLES format.
		*************************************************************************/
		bool setOGLESFormat(const uint32& internalformat, const uint32& format, const uint32& type);

		/*!***********************************************************************
		 @Function		setD3DFormat
		 @Return		bool Whether the format is valid or not.
		 @Description	Sets the format of the texture to PVRTexLib's internal
						representation of the D3D format.
		*************************************************************************/
		bool setD3DFormat(const uint32& DWORD_D3D_FORMAT);
		
		/*!***********************************************************************
		 @Function		setDXGIFormat
		 @Return		bool Whether the format is valid or not.
		 @Description	Sets the format of the texture to PVRTexLib's internal
						representation of the DXGI format.
		*************************************************************************/
		bool setDXGIFormat(const uint32& DWORD_DXGI_FORMAT);

		/*!***********************************************************************
		 @Function		setWidth
		 @Input				newWidth	The new width.
		 @Description	Sets the width.
		*************************************************************************/
		void setWidth(uint32 newWidth);

		/*!***********************************************************************
		 @Function		setHeight
		 @Input				newHeight	The new height.
		 @Description	Sets the height.
		*************************************************************************/
		void setHeight(uint32 newHeight);

		/*!***********************************************************************
		 @Function		setDepth
		 @Input				newDepth	The new depth.
		 @Description	Sets the depth.
		*************************************************************************/
		void setDepth(uint32 newDepth);

		/*!***********************************************************************
		 @Function		setNumArrayMembers
		 @Input				newNumMembers	The new number of members in this array.
		 @Description	Sets the depth.
		*************************************************************************/
		void setNumArrayMembers(uint32 newNumMembers);

		/*!***********************************************************************
		 @Function		setNumMIPLevels
		 @Input				newNumMIPLevels		New number of MIP-Map levels.
		 @Description	Sets the number of MIP-Map levels in this texture.
		*************************************************************************/
		void setNumMIPLevels(uint32 newNumMIPLevels);

		/*!***********************************************************************
		 @Function		setNumFaces
		 @Input				newNumFaces New number of faces for this texture.
		 @Description	Sets the number of faces stored in this texture.
		*************************************************************************/
		void setNumFaces(uint32 newNumFaces);

		/*!***********************************************************************
		 @Function		setOrientation
		 @Input				eAxisOrientation Enum specifying axis and orientation.
		 @Description	Sets the data orientation for a given axis in this texture.
		*************************************************************************/
		void setOrientation(EPVRTOrientation eAxisOrientation);

		/*!***********************************************************************
		 @Function		setIsFileCompressed
		 @Input				isFileCompressed	Sets file compression to true/false.
		 @Description	Sets whether or not the texture is compressed using
						PVRTexLib's FILE compression - this is independent of 
						any texture compression. Currently unsupported.
		*************************************************************************/
		void setIsFileCompressed(bool isFileCompressed);
		
		/*!***********************************************************************
		 @Function		isPreMultiplied
		 @Return		isPreMultiplied	Sets if texture is premultiplied.
		 @Description	Sets whether or not the texture's colour has been
						pre-multiplied by the alpha values.
		*************************************************************************/
		void setIsPreMultiplied(bool isPreMultiplied);

	/*!***********************************************************************
	 Meta Data functions - Getters.
	*************************************************************************/	

		/*!***********************************************************************
		 @Function		isBumpMap
		 @Return		bool	True if it is a bump map.
		 @Description	Returns whether the texture is a bump map or not.
		*************************************************************************/
		const bool isBumpMap() const;

		/*!***********************************************************************
		 @Function		getBumpMapScale
		 @Return		float	Returns the bump map scale.
		 @Description	Gets the bump map scaling value for this texture. If the
						texture is not a bump map, 0.0f is returned. If the
						texture is a bump map but no meta data is stored to
						specify its scale, then 1.0f is returned.
		*************************************************************************/
		const float getBumpMapScale() const;

		/*!***********************************************************************
		 @Function		getBumpMapOrder
		 @Return		CPVRTString		Returns bump map order relative to rgba.
		 @Description	Gets the bump map channel order relative to rgba. For
						example, an RGB texture with bumps mapped to XYZ returns 
						'xyz'. A BGR texture with bumps in the order ZYX will also 
						return 'xyz' as the mapping is the same: R=X, G=Y, B=Z.
						If the letter 'h' is present in the string, it means that
						the height map has been stored here.
						Other characters are possible if the bump map was created
						manually, but PVRTexLib will ignore these characters. They
						are returned simply for completeness.
		*************************************************************************/
		const CPVRTString getBumpMapOrder() const;

		/*!***********************************************************************
		 @Function		getNumTextureAtlasMembers
		 @Return		int		Returns number of sub textures defined by meta data.
		 @Description	Works out the number of possible texture atlas members in
						the texture based on the w/h/d and the data size.
						TODO: Is this the right way to do things? Should I return number of floats? Or just data size? Hmm. Also need to make it TWO floats per dimension, and also possibly a rotated value. Not sure.
		*************************************************************************/
		const int getNumTextureAtlasMembers() const;

		/*!***********************************************************************
		 @Function		getTextureAtlasData
		 @Return		float*		Returns a pointer directly to the texture atlas data.
		 @Description	Returns a pointer to the texture atlas data.
						TODO: Maybe I should return a copy rather than the original.
		*************************************************************************/
		const float* getTextureAtlasData() const;

		/*!***********************************************************************
		 @Function		getCubeMapOrder
		 @Return		CPVRTString		Returns cube map order.
		 @Description	Gets the cube map face order. Returned string will be in 
						the form "ZzXxYy" with capitals representing positive and
						small letters representing negative. I.e. Z=Z-Positive,
						z=Z-Negative.
		*************************************************************************/
		const CPVRTString getCubeMapOrder() const;

		/*!***********************************************************************
		 @Function		getBorder
		 @Input			uiBorderWidth
		 @Input			uiBorderHeight
		 @Input			uiBorderDepth
		 @Description	Obtains the border size in each dimension for this texture.
		*************************************************************************/
		void getBorder(uint32& uiBorderWidth, uint32& uiBorderHeight, uint32& uiBorderDepth) const;

		/*!***********************************************************************
		 @Function		getMetaData
		 @Input			DevFOURCC
		 @Input			u32Key
		 @Return		pvrtexture::MetaDataBlock A copy of the meta data from the texture.
		 @Description	Returns a block of meta data from the texture. If the meta data doesn't exist, a block with data size 0 will be returned.
		*************************************************************************/
		MetaDataBlock getMetaData(uint32 DevFOURCC, uint32 u32Key) const;

		/*!***********************************************************************
		 @Function		hasMetaData
		 @Input			DevFOURCC
		 @Input			u32Key
		 @Return		bool Whether or not the meta data bock specified exists
		 @Description	Returns whether or not the specified meta data exists as 
						part of this texture header.
		*************************************************************************/
		bool hasMetaData(uint32 DevFOURCC, uint32 u32Key) const;

	/*!***********************************************************************
	 Meta Data functions - Setters.
	*************************************************************************/	
		
		/*!***********************************************************************
		 @Function		setBumpMap
		 @Input				bumpScale	Floating point "height" value to scale the bump map.
		 @Input				bumpOrder	Up to 4 character string, with values x,y,z,h in 
									some combination. Not all values need to be present.
									Denotes channel order; x,y,z refer to the 
									corresponding axes, h indicates presence of the
									original height map. It is possible to have only some
									of these values rather than all. For example if 'h'
									is present alone it will be considered a height map.
									The values should be presented in RGBA order, regardless
									of the texture format, so a zyxh order in a bgra texture
									should still be passed as 'xyzh'. Capitals are allowed.
									Any character stored here that is not one of x,y,z,h
									or a NULL character	will be ignored when PVRTexLib 
									reads the data,	but will be preserved. This is useful
									if you wish to define a custom data channel for instance.
									In these instances PVRTexLib will assume it is simply
									colour data.
		 @Description	Sets a texture's bump map data.
		*************************************************************************/
		void setBumpMap(float bumpScale, CPVRTString bumpOrder="xyz");

		/*!***********************************************************************
		 @Function		setTextureAtlas
		 @Input				pAtlasData	Pointer to an array of atlas data.
		 @Input				dataSize	Number of floats that the data pointer contains.
		 @Description	Sets the texture atlas coordinate meta data for later display.
						It is up to the user to make sure that this texture atlas
						data actually makes sense in the context of the header. It is
						suggested that the "generateTextureAtlas" method in the tools
						is used to create a texture atlas, manually setting one up is 
						possible but should be done with care.
		*************************************************************************/
		void setTextureAtlas(float* pAtlasData, uint32 dataSize);

		/*!***********************************************************************
		 @Function		setCubeMapOrder
		 @Input				cubeMapOrder	Up to 6 character string, with values 
										x,X,y,Y,z,Z in some combination. Not all 
										values need to be present. Denotes face 
										order; Capitals refer to positive axis 
										positions and small letters refer to 
										negative axis positions. E.g. x=X-Negative,
										X=X-Positive. It is possible to have only 
										some of these values rather than all, as 
										long as they are NULL terminated.
										NB: Values past the 6th character are not read.
		 @Description	Sets a texture's bump map data.
		*************************************************************************/
		void setCubeMapOrder(CPVRTString cubeMapOrder="XxYyZz");

		/*!***********************************************************************
		 @Function		setBorder
		 @Input			uiBorderWidth
		 @Input			uiBorderHeight
		 @Input			uiBorderDepth
		 @Return		void 
		 @Description	Sets a texture's border size data. This value is subtracted 
						from the current texture height/width/depth to get the valid 
						texture data.
		*************************************************************************/
		void setBorder(uint32 uiBorderWidth, uint32 uiBorderHeight, uint32 uiBorderDepth);

		/*!***********************************************************************
		 @Function		addMetaData
		 @Input				MetaBlock	Meta data block to be added.
		 @Description	Adds an arbitrary piece of meta data.
		*************************************************************************/
		void addMetaData(const MetaDataBlock& MetaBlock);
				
		/*!***********************************************************************
		 @Function		removeMetaData
		 @Input			DevFourCC
		 @Input			u32Key
		 @Return		void 
		 @Description	Removes a specified piece of meta data, if it exists.
		*************************************************************************/
		void removeMetaData(const uint32& DevFourCC, const uint32& u32Key);
	};
};

#endif
