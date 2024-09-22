// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "api_MGI.h"

#define STBI_NO_PSD
#define STBI_NO_BMP
#define STBI_NO_TGA
#define STBI_NO_HDR
#define STBI_NO_PIC
#define STBI_NO_PNM

#if defined(_WIN32)
#define __STDC_LIB_EXT1__
#endif

#define STB_IMAGE_IMPLEMENTATION
#include "stb_image.h"

#define STB_IMAGE_WRITE_IMPLEMENTATION
#include "stb_image_write.h"


void MGI_ReadRGBA(mgbyte* data, mgint dataBytes, mgbool zeroTransparentPixels, mgint& width, mgint& height, mgbyte*& rgba)
{
	width = 0;
	height = 0;
	rgba = nullptr;

	int c, w, h;
	auto image = stbi_load_from_memory(data, dataBytes, &w, &h, &c, 4);
	if (image == nullptr)
	{
		width = 0;
		height = 0;
		return;
	}

	// If the original image before conversion had alpha...
	if (zeroTransparentPixels && c == 4)
	{
		// XNA blacks out any pixels with an alpha of zero.
		for (int i = 0; i < w * h; i += 4)
		{
			if (image[i + 3] == 0)
			{
				image[i + 0] = 0;
				image[i + 1] = 0;
				image[i + 2] = 0;
			}
		}
	}

	rgba = image;
	width = w;
	height = h;
}

struct mem_image
{
	static const size_t grow = 4096;

	mem_image()
	{
		dataBytes = grow;
		data = (mgbyte*)malloc(grow);
		offset = 0;
	}

	mgbyte* data;
	mgint dataBytes;
	size_t offset;
};

static void mem_image_write(void* context, void* data, int size)
{
	mem_image* image = (mem_image*)context;

	size_t offset = image->offset + size;


	if (offset > image->dataBytes)
	{
		// TODO:  Need a better strategy for memory growth.
		// TODO:  Need to handle allocation failure gracefully.
		image->dataBytes = (mgint)(offset + mem_image::grow); // Need a better algo!
		image->data = (mgbyte*)realloc(image->data, image->dataBytes);
	}

	memcpy(image->data + image->offset, data, size);
	image->offset = offset;
}

void MGI_WriteJpg(mgbyte* data, mgint dataBytes, mgint width, mgint height, mgint quality, mgbyte*& jpg, mgint& jpgBytes)
{
	jpg = nullptr;
	jpgBytes = 0;

	mem_image image;
	stbi_write_jpg_to_func(mem_image_write, &image, width, height, 4, data, quality);

	jpg = (mgbyte*)realloc(image.data, image.offset);
	jpgBytes = image.dataBytes;
}

void MGI_WritePng(mgbyte* data, mgint dataBytes, mgint width, mgint height, mgbyte*& png, mgint& pngBytes)
{
	png = nullptr;
	pngBytes = 0;

	mem_image image;
	stbi_write_png_to_func(mem_image_write, &image, width, height, 4, data, 4);

	png = (mgbyte*)realloc(image.data, image.offset);
	pngBytes = image.dataBytes;
}
