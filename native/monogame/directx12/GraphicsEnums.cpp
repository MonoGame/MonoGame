// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "directx12.h"

#include "api_enums.h"

#include "GraphicsEnums.h"

DXGI_FORMAT Graphics::TextureFormatToDXGI_FORMAT(MGSurfaceFormat tf)
{
    switch (tf) {
    case MGSurfaceFormat::Color:
        return DXGI_FORMAT_R8G8B8A8_UNORM;
    case MGSurfaceFormat::Bgr565:
        return DXGI_FORMAT_B5G6R5_UNORM;
    case MGSurfaceFormat::Bgra5551:
        return DXGI_FORMAT_B5G5R5A1_UNORM;
    case MGSurfaceFormat::Bgra4444:
        return DXGI_FORMAT_B4G4R4A4_UNORM;
    case MGSurfaceFormat::Dxt1:
        return DXGI_FORMAT_BC1_UNORM;
    case MGSurfaceFormat::Dxt3:
        return DXGI_FORMAT_BC2_UNORM;
    case MGSurfaceFormat::Dxt5:
        return DXGI_FORMAT_BC3_UNORM;
    case MGSurfaceFormat::NormalizedByte2:
        return DXGI_FORMAT_R8G8_SNORM;
    case MGSurfaceFormat::NormalizedByte4:
        return DXGI_FORMAT_R8G8B8A8_SNORM;
    case MGSurfaceFormat::Rgba1010102:
        return DXGI_FORMAT_R10G10B10A2_UNORM;
    case MGSurfaceFormat::Rg32:
        return DXGI_FORMAT_R16G16_UNORM;
    case MGSurfaceFormat::Rgba64:
        return DXGI_FORMAT_R16G16B16A16_UNORM;
    case MGSurfaceFormat::Alpha8:
        return DXGI_FORMAT_A8_UNORM;
    case MGSurfaceFormat::Single:
        return DXGI_FORMAT_R32_FLOAT;
    case MGSurfaceFormat::Vector2:
        return DXGI_FORMAT_R32G32_FLOAT;
    case MGSurfaceFormat::Vector4:
        return DXGI_FORMAT_R32G32B32A32_FLOAT;
    case MGSurfaceFormat::HalfSingle:
        return DXGI_FORMAT_R16_FLOAT;
    case MGSurfaceFormat::HalfVector2:
        return DXGI_FORMAT_R16G16_FLOAT;
    case MGSurfaceFormat::HalfVector4:
        return DXGI_FORMAT_R16G16B16A16_FLOAT;
    case MGSurfaceFormat::HdrBlendable:
        return DXGI_FORMAT_R16G16B16A16_FLOAT;
    }
    throw std::out_of_range("invalid TextureFormat");
}

DXGI_FORMAT Graphics::ConvertSRVtoResourceFormat(DXGI_FORMAT format) noexcept
{
    switch (format) {
    case DXGI_FORMAT_R32G32B32A32_FLOAT:
    case DXGI_FORMAT_R32G32B32A32_UINT:
    case DXGI_FORMAT_R32G32B32A32_SINT:
        return DXGI_FORMAT_R32G32B32A32_TYPELESS;

    case DXGI_FORMAT_R16G16B16A16_FLOAT:
    case DXGI_FORMAT_R16G16B16A16_UNORM:
    case DXGI_FORMAT_R16G16B16A16_UINT:
    case DXGI_FORMAT_R16G16B16A16_SNORM:
    case DXGI_FORMAT_R16G16B16A16_SINT:
        return DXGI_FORMAT_R16G16B16A16_TYPELESS;

    case DXGI_FORMAT_R32G32_FLOAT:
    case DXGI_FORMAT_R32G32_UINT:
    case DXGI_FORMAT_R32G32_SINT:
        return DXGI_FORMAT_R32G32_TYPELESS;

    case DXGI_FORMAT_R10G10B10A2_UNORM:
    case DXGI_FORMAT_R10G10B10A2_UINT:
        return DXGI_FORMAT_R10G10B10A2_TYPELESS;

    case DXGI_FORMAT_R8G8B8A8_UNORM:
    case DXGI_FORMAT_R8G8B8A8_UINT:
    case DXGI_FORMAT_R8G8B8A8_SNORM:
    case DXGI_FORMAT_R8G8B8A8_SINT:
        return DXGI_FORMAT_R8G8B8A8_TYPELESS;

    case DXGI_FORMAT_R16G16_FLOAT:
    case DXGI_FORMAT_R16G16_UNORM:
    case DXGI_FORMAT_R16G16_UINT:
    case DXGI_FORMAT_R16G16_SNORM:
    case DXGI_FORMAT_R16G16_SINT:
        return DXGI_FORMAT_R16G16_TYPELESS;

    case DXGI_FORMAT_R32_FLOAT:
    case DXGI_FORMAT_R32_UINT:
    case DXGI_FORMAT_R32_SINT:
        return DXGI_FORMAT_R32_TYPELESS;

    case DXGI_FORMAT_R8G8_UNORM:
    case DXGI_FORMAT_R8G8_UINT:
    case DXGI_FORMAT_R8G8_SNORM:
    case DXGI_FORMAT_R8G8_SINT:
        return DXGI_FORMAT_R8G8_TYPELESS;

    case DXGI_FORMAT_R16_FLOAT:
    case DXGI_FORMAT_R16_UNORM:
    case DXGI_FORMAT_R16_UINT:
    case DXGI_FORMAT_R16_SNORM:
    case DXGI_FORMAT_R16_SINT:
        return DXGI_FORMAT_R16_TYPELESS;

    case DXGI_FORMAT_R8_UNORM:
    case DXGI_FORMAT_R8_UINT:
    case DXGI_FORMAT_R8_SNORM:
    case DXGI_FORMAT_R8_SINT:
        return DXGI_FORMAT_R8_TYPELESS;

    default:
        return format;
    }
}
