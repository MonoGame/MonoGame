-- MonoGame - Copyright (C) MonoGame Foundation, Inc
-- This file is subject to the terms and conditions defined in
-- file 'LICENSE.txt', which is part of this source code package.

function pipeline_native()
    platform_target_path = "../../Artifacts/mgpipeline/%{cfg.buildcfg}"

    kind "SharedLib"
    language "C++"

    defines { 
        "DLL_EXPORT", 
        "STB_IMAGE_IMPLEMENTATION",
        "STB_IMAGE_WRITE_IMPLEMENTATION",
        "STB_IMAGE_RESIZE_IMPLEMENTATION",
    }
    
    filter "system:windows"
        architecture "x64"
        defines { 
            "STBI_WINDOWS_UTF8",
            "STBIW_WINDOWS_UTF8",
        }

    filter {}
    targetdir(platform_target_path)
    targetname "mgpipeline"
    cppdialect "C++17"

    files {
        "include/**.h",
        "*.cpp",
    }
    includedirs {
        "include",
        "../monogame/include",
        "../../external/stb",
    }
end

workspace "pipeline"
    configurations { "Debug", "Release" }

project "monogame_native_pipeline"
    pipeline_native()
    filter "configurations:Debug"
        defines { "DEBUG" }
        symbols "On"
    filter "configurations:Release"
        defines { "NDEBUG" }
        filter "system:windows"
            buildoptions { "/MT" }
        optimize "On"

    filter "system:macosx"
      buildoptions { "-arch x86_64", "-arch arm64" }
      linkoptions { "-arch x86_64", "-arch arm64" }
