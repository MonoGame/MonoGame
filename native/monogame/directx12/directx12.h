// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include <winsdkver.h>
#define _WIN32_WINNT 0x0A00
#include <sdkddkver.h>

// Use the C++ standard templated min/max
#define NOMINMAX

// DirectX apps don't need GDI
#define NODRAWTEXT
#define NOGDI
#define NOBITMAP

// Include <mcx.h> if you need this
#define NOMCX

// Include <winsvc.h> if you need this
#define NOSERVICE

// WinHelp is deprecated
#define NOHELP

#include <Windows.h>

#include <wrl/client.h>
#include <wrl/event.h>

#if defined(_GRDK_VER)
#include <grdk.h>

#if _GRDK_VER < 0x55F00C58 /* GDK Edition 220300 */
#error This sample requires the March 2022 GDK or later
#endif
#endif

#define D3DX12_NO_STATE_OBJECT_HELPERS

#ifdef _GAMING_XBOX_SCARLETT
#include <d3d12_xs.h>
#include <d3dx12_xs.h>
#elif defined(_GAMING_XBOX)
#include <d3d12_x.h>
#include <d3dx12_x.h>
#else
#include <d3d12.h>
#include <dxgi1_6.h>

#ifdef _DEBUG
#include <dxgidebug.h>
#endif

#include "d3dx12.h"
#endif

#define _XM_NO_XMVECTOR_OVERLOADS_

#include <DirectXColors.h>
#include <DirectXMath.h>

#include <algorithm>
#include <cassert>
#include <cmath>
#include <cstddef>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <cwchar>
#include <exception>
#include <iterator>
#include <memory>
#include <stdexcept>
#include <string>
#include <system_error>
#include <tuple>

#ifdef _GAMING_XBOX
#include <pix3.h>
#else
// To use graphics markup events with the latest version of PIX, change this to include <pix3.h>
// then add the NuGet package WinPixEventRuntime to the project.
#include <pix.h>
#endif

#ifdef _GAMING_XBOX
#include <XGame.h>
#include <XSystem.h>
#include <XTaskQueue.h>
#endif

#define D3D12MA_D3D12_HEADERS_ALREADY_INCLUDED
#include "D3D12MemAlloc.h"

#include <fstream>
#include <system_error>
#include <vector>
#include <unordered_map>

#ifndef IID_GRAPHICS_PPV_ARGS
#define IID_GRAPHICS_PPV_ARGS(x) IID_PPV_ARGS(x)
#endif

namespace DX {
    // Helper class for COM exceptions
    class com_exception : public std::exception {
    public:
        com_exception(HRESULT hr) noexcept
            : result(hr)
        {
        }

        const char* what() const noexcept override
        {
            static char s_str[64] = {};
            sprintf_s(s_str, "Failure with HRESULT of %08X", static_cast<unsigned int>(result));
            return s_str;
        }

    private:
        HRESULT result;
    };

    // Helper utility converts D3D API failures into exceptions.
    inline void ThrowIfFailed(HRESULT hr)
    {
        if (FAILED(hr)) {
#ifdef _DEBUG
            char str[64] = {};
            sprintf_s(str, "**ERROR** Fatal Error with HRESULT of %08X\n", static_cast<unsigned int>(hr));
            OutputDebugStringA(str);
            __debugbreak();
#endif
            throw com_exception(hr);
        }
    }

    // Helper to check for power-of-2
    template<typename T>
    constexpr bool IsPowerOf2(T x) noexcept { return ((x != 0) && !(x & (x - 1))); }

    // Helpers for aligning values by a power of 2
    template<typename T>
    inline T AlignDown(T size, size_t alignment) noexcept
    {
        if (alignment > 0)
        {
            assert(((alignment - 1) & alignment) == 0);
            auto mask = static_cast<T>(alignment - 1);
            return size & ~mask;
        }
        return size;
    }

    template<typename T>
    inline T AlignUp(T size, size_t alignment) noexcept
    {
        if (alignment > 0)
        {
            assert(((alignment - 1) & alignment) == 0);
            auto mask = static_cast<T>(alignment - 1);
            return (size + mask) & ~mask;
        }
        return size;
    }
}

// Enable off by default warnings to improve code conformance
#pragma warning(default : 4061 4062 4191 4242 4263 4264 4265 4266 4289 4365 4746 4826 4841 4986 4987 5029 5038 5042)
