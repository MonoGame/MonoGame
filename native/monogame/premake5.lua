-- MonoGame - Copyright (C) MonoGame Foundation, Inc
-- This file is subject to the terms and conditions defined in
-- file 'LICENSE.txt', which is part of this source code package.

local vulkan_sdk = os.getenv("VULKAN_SDK")

if vulkan_sdk == nil and os.target() == "macosx" then
    error("Error: VULKAN_SDK environment variable is not set. Please set it to your Vulkan SDK installation path.")
end

newoption {
    trigger = "arch",
    value = "ARCH",
    description = "Target architecture (x64 or arm64)",
    default = "x64",
    allowed = {
        { "x64", "64-bit x86" },
        { "arm64", "64-bit ARM" }
    }
}

function common(project_name)
    if os.target() == "windows" then
        filter "platforms:x64"
        architecture "x86_64"
        filter "platforms:arm64"
        architecture "ARM64"
        filter {}
        platform_target_path = "../../Artifacts/native/mgruntime/" .. project_name .. "/%{cfg.system}/%{cfg.platform}/%{cfg.buildcfg}"
    else
        local target_arch = _OPTIONS["arch"] or "x64"
        architecture(target_arch == "arm64" and "ARM64" or "x64")
        if os.target() == "macosx" then
            platform_target_path = "../../Artifacts/native/mgruntime/" .. project_name .. "/%{cfg.system}/%{cfg.buildcfg}"
        else
            platform_target_path = "../../Artifacts/native/mgruntime/" .. project_name .. "/%{cfg.system}/" .. target_arch .. "/%{cfg.buildcfg}"
        end
    end
    kind "SharedLib"
    language "C++"
    filter "system:linux"
    pic "On"
    filter {}
    defines {"DLL_EXPORT"}
    targetdir(platform_target_path)
    targetname "mgruntime"
    cppdialect "C++17"

    files {"include/**.h", "common/**.h", "common/**.cpp"}
    includedirs {"include", "../../external/stb"}
end

-- SDL is supported on all desktop platforms.
function sdl2()
    defines {"MG_SDL2"}

    files {"sdl/**.h", "sdl/**.cpp"}

    includedirs {"external/sdl2/sdl/include"}

    filter {"system:windows"}
    links {"external/sdl2/sdl/build/%{cfg.platform}/Release/SDL2-static.lib", "winmm", "imm32", "user32", "gdi32", "advapi32",
           "setupapi", "ole32", "oleaut32", "version", "shell32"}
    filter {"system:macosx"}
    libdirs {"external/sdl2/sdl/build"}
    linkoptions {"-Wl,-force_load,external/sdl2/sdl/build/libSDL2.a"}
    links {"SDL2"}
    links {"Cocoa.framework", "IOKit.framework", "ForceFeedback.framework", "CoreAudio.framework",
        "AudioToolbox.framework", "CoreGraphics.framework", "CoreFoundation.framework", "Metal.framework",
        "CoreVideo.framework", "GameController.framework", "CoreHaptics.framework", "Carbon.framework", "iconv"}

    filter {"system:linux"}
    linkoptions {"external/sdl2/sdl/build/libSDL2.a"}
    links {"dl", "pthread", "m", "rt"}
    filter {}
end

-- Vulkan is supported for all desktop platforms.
function vulkan()
    defines {"MG_VULKAN"}

    files {"vulkan/**.h", "vulkan/**.cpp"}

    includedirs {"external/vulkan-headers/include", "external/volk", "external/vma/include",
        path.join(vulkan_sdk, "include")}

    filter {"system:macosx"}
    libdirs {path.join(vulkan_sdk, "lib/MoltenVK.xcframework/macos-arm64_x86_64")}
    links {"MoltenVK", "IOSurface.framework", "Foundation.framework", "QuartzCore.framework", "AppKit.framework"}
    filter {}
end

-- DirectX12 is supported on Xbox and Windows.
function directx12()
    defines {"MG_DIRECTX12"}

    files {"directx12/**.h", "directx12/**.cpp"}

    filter {"system:windows"}
    links {"dxguid", "dxgi", "d3d12"}
    filter {}
end

-- FAudio is supported for all desktop platforms.
function faudio()
    defines {"MG_FAUDIO"}

    files {"faudio/**.h", "faudio/**.cpp"}

    includedirs {"external/faudio/include"}
    
    filter {"system:windows"}
    libdirs {"external/faudio/build/%{cfg.platform}/Release"}
    links {"FAudio.lib"}
    
    filter {"system:macosx"}
    libdirs {"external/faudio/build"}
    linkoptions {
        "-Wl,-force_load,external/faudio/build/libFAudio.a",
        "-Wl,-ld_classic"
    }
    
    filter {"system:linux"}
    linkoptions {"external/faudio/build/libFAudio.a"}
    filter {}
end

-- Xaudio is supported on Windows and Xbox.
function xaudio()
    defines {"MG_XAUDIO"}

    files {"xaudio/**.h", "xaudio/**.cpp"}
end

function configs()
    filter "configurations:Debug"
    defines {"DEBUG"}
    symbols "On"

    filter "configurations:Release"
    defines {"NDEBUG"}
    optimize "On"

    filter {"system:windows"}
    staticruntime "On"
    filter {"system:windows", "configurations:Debug"}
    runtime "Debug"
    filter {"system:windows", "configurations:Release"}
    runtime "Release"

    filter "system:macosx"
    buildoptions {"-arch x86_64", "-arch arm64"}
    linkoptions {"-arch x86_64", "-arch arm64"}
    filter {}
end

workspace "monogame"
configurations {"Debug", "Release"}
if os.target() == "windows" then
    platforms { "x64", "arm64" }
end

project "desktopvk"
common("desktopvk")
sdl2()
vulkan()
faudio()
configs()

if os.target() == "windows" then
    project "windowsdx"
    common("windowsdx")
    sdl2()
    directx12()
    xaudio()
    configs()
end
