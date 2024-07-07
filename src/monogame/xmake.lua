-- MonoGame - Copyright (C) MonoGame Foundation, Inc
-- This file is subject to the terms and conditions defined in
-- file 'LICENSE.txt', which is part of this source code package.

-- Bulding a dynamic link library.
set_kind("shared")
set_basename("monogame.native")
add_defines("DLL_EXPORT")

-- Always include the common bits.
add_files("common/*.cpp")
add_includedirs("include")

target("desktopvk")

    set_targetdir("../../Artifacts/monogame.native/$(os)/desktopvk/$(mode)")

    -- SDL is supported on all desktop platforms.
    add_defines("MG_SDL2")
    add_files("sdl/*.cpp")
    add_includedirs("../../ThirdParty/Dependencies/SDL/include")
    add_links("SDL2-static")
    if is_plat("windows") then
        add_linkdirs("../../ThirdParty/Dependencies/SDL/Windows/x64")
        add_links("winmm", "imm32", "user32", "gdi32", "advapi32", "setupapi", "ole32", "oleaut32", "version", "shell32")
    end

    -- Vulkan is supported for all desktop platforms.
    add_defines("MG_VULKAN");
    add_files("vulkan/*.cpp")
    add_includedirs("../../ThirdParty/Dependencies/VulkanSDK/Include")
    if is_plat("windows") then
        add_files("vulkan/vulkan.rc")
        add_linkdirs("../../ThirdParty/Dependencies/VulkanSDK/Lib/Windows")
    end
    add_links("volk")

 target_end()

 -- 
 target("windowsdx")

    -- TODO!

 target_end()

