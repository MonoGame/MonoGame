-- MonoGame - Copyright (C) MonoGame Foundation, Inc
-- This file is subject to the terms and conditions defined in
-- file 'LICENSE.txt', which is part of this source code package.

function pipeline_native()
    platform_target_path = "../../Artifacts/monogame.native.pipeline/%{cfg.system}/%{cfg.buildcfg}"

    kind "SharedLib"
    language "C++"
    architecture "x64"
    defines { "DLL_EXPORT" }
    targetdir(platform_target_path)
    targetname "monogame.native.pipeline"
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
        optimize "On"
