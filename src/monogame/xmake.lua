-- MonoGame - Copyright (C) MonoGame Foundation, Inc
-- This file is subject to the terms and conditions defined in
-- file 'LICENSE.txt', which is part of this source code package.

-- Bulding a dynamic link library.
set_kind("shared")
set_basename("monogame.native")
add_defines("DLL_EXPORT")
add_rules("mode.release", "mode.debug")

-- Always include the common bits.
add_files("common/*.cpp")
add_includedirs("include")

-- Required dependencies.
add_requires("libsdl 2.30.*")
add_requires("vulkansdk")

target("desktopvk")

    set_targetdir("../../Artifacts/monogame.native/$(os)/desktopvk/$(mode)")

    -- SDL is supported on all desktop platforms.
    add_defines("MG_SDL2")
    add_files("sdl/*.cpp")
    add_packages("libsdl")

    -- Vulkan is supported for all desktop platforms.
    add_defines("MG_VULKAN");
    add_files("vulkan/*.cpp")
    add_packages("vulkansdk")
    if is_plat("windows") then
        add_files("vulkan/vulkan.rc")
    end
    add_links("volk")

 target_end()

 -- 
 target("windowsdx")

    -- TODO!

 target_end()

