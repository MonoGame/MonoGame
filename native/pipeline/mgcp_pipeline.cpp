#define STB_IMAGE_IMPLEMENTATION
#include "stb_image.h"
#include "api_MGCP.h"
#include <stdlib.h>
#include <string.h>

MGCP_Bitmap* MP_ImportBitmap(const char* filename)
{
    FILE* f = stbi__fopen(filename, "rb");
    int is_16_bit;
    if (!f) return NULL;
    is_16_bit = stbi_is_16_bit_from_file(f);

    int width = 0, height = 0, channels = 0;
    void* data = NULL;

    if (is_16_bit)
        data = stbi_load_from_file_16(f, &width, &height, &channels, 4);
    else
		data = stbi_load_from_file(f, &width, &height, &channels, 4);

    fclose(f);

    if (!data)
        return NULL;

    MGCP_Bitmap* bmp = (MGCP_Bitmap*)malloc(sizeof(MGCP_Bitmap));
    if (!bmp) {
        stbi_image_free(data);
        return NULL;
    }
    bmp->width = width;
    bmp->height = height;
	bmp->is_16_bit = is_16_bit ? true : false;

    size_t size = width * height * 4;
    if (is_16_bit)
		size *= 2; // 16-bit images have double the size per pixel

    bmp->data = (mgbyte*)malloc(size);
    if (!bmp->data) {
        free(bmp);
        stbi_image_free(data);
        return NULL;
    }
    memcpy(bmp->data, data, size);
    stbi_image_free(data);
    return bmp;
}

void MP_FreeBitmap(MGCP_Bitmap* bitmap)
{
    if (bitmap) {
        if (bitmap->data)
            free(bitmap->data);
        free(bitmap);
    }
}
