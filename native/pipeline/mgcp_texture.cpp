#include <stdlib.h>
#include <string.h>
#include <float.h>

#include "stb_image.h"
#include "stb_image_write.h"
#include "stb_image_resize2.h"

#include "api_MGCP.h"

void* MP_ImportBitmap(const char* importPath, MGCP_Bitmap& bitmap)
{
    FILE* f;
    int width, height, channels;
    void* data = nullptr;

    f = stbi__fopen(importPath, "rb");
    if (!f) goto err;

    if (stbi_is_hdr_from_file(f))
    {
        bitmap.type = MGTextureType::RgbaF;
        data = stbi_loadf_from_file(f, &width, &height, &channels, 4);
    }
    else if (stbi_is_16_bit_from_file(f))
    {
        bitmap.type = MGTextureType::Rgba16;
        data = stbi_load_from_file_16(f, &width, &height, &channels, 4);
    }
    else
    {
        bitmap.type = MGTextureType::Rgba8;
        data = stbi_load_from_file(f, &width, &height, &channels, 4);
    }

    fclose(f);

    if (!data)
        goto err;

    bitmap.width = width;
    bitmap.height = height;

    bitmap.data = data;
    return nullptr;

err:
    return (void*)stbi_failure_reason();
}

void MP_FreeBitmap(MGCP_Bitmap& bitmap)
{
    if (bitmap.data)
        stbi_image_free(bitmap.data);
    bitmap.data = nullptr;
}

inline stbir_filter MP_HeuristicFilter(mgint srcMeasure, mgint dstMeasure)
{
    if (dstMeasure != 1 && (srcMeasure == srcMeasure / dstMeasure * dstMeasure || srcMeasure == dstMeasure + 1))
        return STBIR_FILTER_BOX;

    mgint power = powf(2.0f, floorf(log2f((float)srcMeasure / dstMeasure)));
    if (power > 1 && srcMeasure / power == dstMeasure)
        return STBIR_FILTER_CATMULLROM;

    return STBIR_FILTER_DEFAULT;
}

inline mgint MP_GetBpp(MGTextureType type)
{
    switch (type)
    {
    case MGTextureType::Rgba8:
        return 4;
    case MGTextureType::Rgba16:
        return 8;
    case MGTextureType::RgbaF:
        return 16;
    default:
        return 0; // Unsupported type
    }
}

void* MP_ResizeBitmap(MGCP_Bitmap& srcBitmap, MGCP_Bitmap& dstBitmap)
{
    stbir_datatype data_type;
    size_t bpp;

    if (!srcBitmap.data || srcBitmap.width <= 0 || srcBitmap.height <= 0 || dstBitmap.width <= 0 || dstBitmap.height <= 0)
    {
        return (void*)"Invalid input bitmap or dimensions for resizing.";
    }

    bpp = MP_GetBpp(srcBitmap.type);

    switch (srcBitmap.type)
    {
    case MGTextureType::Rgba8:
        data_type = STBIR_TYPE_UINT8;
        break;
    case MGTextureType::Rgba16:
        data_type = STBIR_TYPE_UINT16;
        break;
    case MGTextureType::RgbaF:
        data_type = STBIR_TYPE_FLOAT;
        break;
    default:
        return (void*)"Unsupported source bitmap pixel format for resizing.";
    }

    int dst_bytes = dstBitmap.width * dstBitmap.height * bpp;
    dstBitmap.data = malloc(dst_bytes);
    dstBitmap.type = srcBitmap.type;

    if (!dstBitmap.data)
    {
        return (void*)"Failed to allocate memory for resized bitmap.";
    }

    STBIR_RESIZE resize;

    stbir_resize_init(&resize,
        srcBitmap.data, srcBitmap.width, srcBitmap.height, 0,
        dstBitmap.data, dstBitmap.width, dstBitmap.height, 0,
        STBIR_4CHANNEL, data_type);

    resize.horizontal_edge = STBIR_EDGE_CLAMP;
    resize.vertical_edge = STBIR_EDGE_CLAMP;

    resize.horizontal_filter = MP_HeuristicFilter(srcBitmap.width, dstBitmap.width);
    resize.vertical_filter = MP_HeuristicFilter(srcBitmap.height, dstBitmap.height);

    if (!stbir_resize_extended(&resize))
    {
        free(dstBitmap.data);
        return (void*)stbi_failure_reason();
    }

    return nullptr;

}

void* MP_ExportBitmap(MGCP_Bitmap& bitmap, const char* exportPath)
{
    int bpp;
    int errno;

    if (!bitmap.data || bitmap.width <= 0 || bitmap.height <= 0)
    {
        return (void*)"Invalid bitmap data or dimensions for export.";
    }

    bpp = MP_GetBpp(bitmap.type);
    if (bpp == 0)
    {
        return (void*)"Unsupported bitmap pixel format for export.";
    }

    switch (bitmap.format)
    {
    case MGTextureFormat::Png:
        if (bitmap.type == MGTextureType::RgbaF)
            return (void*)"Exporting float textures to PNG is not supported.";
        errno = stbi_write_png(exportPath, bitmap.width, bitmap.height, 4, bitmap.data, bitmap.width * bpp);
        break;
    case MGTextureFormat::Jpeg:
        if (bitmap.type != MGTextureType::Rgba8)
            return (void*)"Exporting non-RGBA8 textures to JPEG is not supported.";
        errno = stbi_write_jpg(exportPath, bitmap.width, bitmap.height, 4, bitmap.data, 100);
        break;
    case MGTextureFormat::Tga:
        if (bitmap.type != MGTextureType::Rgba8)
            return (void*)"Exporting non-RGBA8 textures to TGA is not supported.";
        errno = stbi_write_tga(exportPath, bitmap.width, bitmap.height, 4, bitmap.data);
        break;
    case MGTextureFormat::Hdr:
        if (bitmap.type != MGTextureType::RgbaF)
            return (void*)"Exporting non-RGBAF textures to HDR is not supported.";
        errno = stbi_write_hdr(exportPath, bitmap.width, bitmap.height, 4, (float*)bitmap.data);
        break;
    case MGTextureFormat::Bmp:
        if (bitmap.type != MGTextureType::Rgba8)
            return (void*)"Exporting non-RGBA8 textures to BMP is not supported.";
        errno = stbi_write_bmp(exportPath, bitmap.width, bitmap.height, 4, bitmap.data);
        break;
    default:
        return (void*)"Unsupported bitmap format for export.";
    }

    if (errno == 0)
    {
        return (void*)stbi_failure_reason();
    }

    return nullptr;
}
