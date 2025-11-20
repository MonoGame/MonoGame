-- MonoGame - Copyright (C) MonoGame Foundation, Inc
-- This file is subject to the terms and conditions defined in
-- file 'LICENSE.txt', which is part of this source code package.

local vulkan_sdk = os.getenv("VULKAN_SDK")

if vulkan_sdk == nil and os.target() == "macosx" then
    error("Error: VULKAN_SDK environment variable is not set. Please set it to your Vulkan SDK installation path.")
end

function common(project_name)
    platform_target_path = "../../Artifacts/native/mgruntime/" .. project_name .. "/%{cfg.system}/%{cfg.buildcfg}"

    kind "SharedLib"
    language "C++"
    filter "system:windows"
    architecture "x64"
    filter "system:linux"
    pic "On"
    filter {}
    if os.target() == "emscripten" then
        kind "StaticLib"
        targetprefix "" -- remove lib prefix
    end
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

    if os.target() ~= "emscripten" then
        filter {"system:windows"}
        links {"external/sdl2/sdl/build/Release/SDL2-static.lib", "winmm", "imm32", "user32", "gdi32", "advapi32",
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

    if os.target() == "emscripten" then
        linkoptions {"external/sdl2/sdl/build_emscripten/libSDL2.a"}
    end
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

-- Add Emscripten/WASM support
function opengl()
    defines {"MG_OPENGL"}
    if os.target() == "emscripten" then
        defines {"MG_EMSCRIPTEN"}
        defines {"MG_WEBGL"}
    end

    -- Add your Emscripten-specific files
    files {"opengl/**.h", "opengl/**.cpp"}

    if os.target() ~= "emscripten" then
        filter {"system:macosx"}
        links {"OpenGL.framework"}
        filter {"system:windows"}
        links {"opengl32"}
        filter {}
    end
end

-- FAudio is supported for all desktop/web platforms.
function faudio()
    defines {"MG_FAUDIO"}

    files {"faudio/**.h", "faudio/**.cpp"}

    includedirs {"external/faudio/include"}

    if os.target() ~= "emscripten" then
    
        filter {"system:windows"}
        libdirs {"external/faudio/build/Release"}
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

    if os.target() == "emscripten" then
        linkoptions {"external/faudio/build_emscripten/libFAudio.a"}
    end
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

    filter {"system:macosx", "configurations:Debug"}
    buildoptions {"-g", "-O0", "-fno-omit-frame-pointer"}
    linkoptions {"-g"}

    if os.target() == "emscripten" then
        -- Change to StaticLib for Emscripten builds to output .a files
        kind "StaticLib"
    end
    
    filter {}
end

workspace "monogame"
configurations {"Debug", "Release"}

if os.target() ~= "emscripten" then
    project "desktopvk"
    common("desktopvk")
    sdl2()
    vulkan()
    faudio()
    configs()

    project "desktopgl"
    common("desktopgl")
    sdl2()
    opengl()
    faudio()
    configs()
end

if os.target() == "windows" then
    project "windowsdx"
    common("windowsdx")
    sdl2()
    directx12()
    xaudio()
    configs()
end

-- Add this to your project section
if os.target() == "emscripten" then
    project "wasm"
    common("wasm")
    sdl2()
    opengl()
    faudio()
    configs()
end
