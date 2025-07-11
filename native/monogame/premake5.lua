-- MonoGame - Copyright (C) MonoGame Foundation, Inc
-- This file is subject to the terms and conditions defined in
-- file 'LICENSE.txt', which is part of this source code package.

function common(project_name)
    platform_target_path = "../../Artifacts/monogame.native/%{cfg.system}/" .. project_name .. "/%{cfg.buildcfg}"

    kind "SharedLib"
    language "C++"
    filter "system:windows"
        architecture "x64"
    filter "system:linux"
        pic "On"
    filter {}
    defines {"DLL_EXPORT"}
    targetdir(platform_target_path)
    targetname "monogame.native"
    cppdialect "C++17"

    files {
        "include/**.h",
        "common/**.h",
        "common/**.cpp"
    }
    includedirs {
        "include",
        "../../external/stb"
    }
end

-- SDL is supported on all desktop platforms.
function sdl2()
    defines {"MG_SDL2"}

    files {
        "sdl/**.h",
        "sdl/**.cpp"
    }

    includedirs {
        "external/sdl2/sdl/include"
    }

    filter {"system:windows"}
        links {
            "external/sdl2/sdl/build/Release/SDL2-static.lib",
            "winmm",
            "imm32",
            "user32",
            "gdi32",
            "advapi32",
            "setupapi",
            "ole32",
            "oleaut32",
            "version",
            "shell32"
        }
    filter {"system:macosx"}
        libdirs { "external/sdl2/sdl/build" }
        linkoptions { "-Wl,-force_load,external/sdl2/sdl/build/libSDL2.a" }
		links { "SDL2" }
		links { "Cocoa", "IOKit", "ForceFeedback", "CoreAudio", "AudioToolbox", "CoreGraphics", "CoreFoundation", "Metal" }

	filter {"system:linux"}
        libdirs { "external/sdl2/sdl/build" }
        linkoptions { "-Wl,-Bstatic" }
        links { "SDL2" }
        linkoptions { "-Wl,-Bdynamic" }
		links { "dl", "pthread", "m", "rt" }
    filter {}
end

-- Vulkan is supported for all desktop platforms.
function vulkan()
    defines {"MG_VULKAN"}

    files {
        "vulkan/**.h",
        "vulkan/**.cpp"
    }

    includedirs {
        "external/vulkan-headers/include",
        "external/volk",
        "external/vma/include"
    }
end

-- DirectX12 is supported on Xbox and Windows.
function directx12()
    defines {"MG_DIRECTX12"}

    files {
        "directx12/**.h",
        "directx12/**.cpp"
    }

    filter {"system:windows"}
    links {
        "dxguid",
        "dxgi",
        "d3d12"
    }
    filter {}
end

-- FAudio is supported for all desktop platforms.
function faudio()
    defines {"MG_FAUDIO"}

    files {
        "faudio/**.h",
        "faudio/**.cpp"
    }
end

-- Xaudio is supported on Windows and Xbox.
function xaudio()
    defines {"MG_XAUDIO"}

    files {
        "xaudio/**.h",
        "xaudio/**.cpp"
    }
end

function configs()
    filter "configurations:Debug"
        defines {"DEBUG"}
        symbols "On"

    filter "configurations:Release"
        defines {"NDEBUG"}
        optimize "On"
    
    filter { "system:windows", "configurations:Release" }
        buildoptions { "/MT" } 

    filter "system:macosx"
        buildoptions {"-arch x86_64", "-arch arm64"}
        linkoptions {"-arch x86_64", "-arch arm64"}
    filter {}
end

workspace "monogame"
    configurations {"Debug", "Release"}

project "desktopvk"
    common("desktopvk")
    sdl2()
    vulkan()
    faudio()
    configs()
    
filter "system:windows"
    project "windowsdx"
        common("windowsdx")
        sdl2()
        directx12()
        xaudio()
        configs()
filter {}
