#include "stb_image.h"
#include "api_MGCP.h"
#include <stdlib.h>
#include <string.h>

void* MP_ImportBitmap(const char* fullpathToFile, MGCP_Bitmap& bitmap)
{
    FILE* f;
    int is_16_bit;
    int width, height, channels;
    void* data = nullptr;

    f = stbi__fopen(fullpathToFile, "rb");
    if (!f) goto err;
    is_16_bit = stbi_is_16_bit_from_file(f);

    if (is_16_bit)
        data = stbi_load_from_file_16(f, &width, &height, &channels, 4);
    else
        data = stbi_load_from_file(f, &width, &height, &channels, 4);

    fclose(f);

    if (!data)
        goto err;

    bitmap.width = width;
    bitmap.height = height;
    bitmap.is_16_bit = is_16_bit ? true : false;

    bitmap.data = data;
    return nullptr;

err:
    return (void*)stbi_failure_reason();
}

void MP_FreeBitmap(MGCP_Bitmap& bitmap)
{
    if (bitmap.data)
        stbi_image_free(bitmap.data);
}
