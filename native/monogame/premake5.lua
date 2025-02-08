-- MonoGame - Copyright (C) MonoGame Foundation, Inc
-- This file is subject to the terms and conditions defined in
-- file 'LICENSE.txt', which is part of this source code package.
require "external/premake-cmake/cmake"
require "external/premake-androidmk/androidmk"

function common(project_name)

   platform_target_path = "../../Artifacts/monogame.native/%{cfg.system}/" .. project_name .. "/%{cfg.buildcfg}"

   kind "SharedLib"
   language "C++"
   cppdialect "C++17"
   defines { "DLL_EXPORT" }
   targetdir( platform_target_path )
   targetname "monogame.native"

   files 
   { 
      "include/**.h",
      "common/**.h",
      "common/**.cpp",
   }
   includedirs 
   {
      "include",      
   }

end

function android()
   defines { "MG_ANDROID" }

   files 
   { 
      "android/**.h",
      "android/**.cpp",
   }
   
   -- Android-specific configurations
   filter "system:android"
      defines { "ANDROID" }
      links { "log", "android" }

end

-- SDL is supported on all desktop platforms.
function sdl2()

   defines { "MG_SDL2" }

   files 
   { 
      "sdl/**.h",
      "sdl/**.cpp",
   }

   includedirs 
   {
      "external/sdl2/sdl/include",      
   }

   filter { "system:windows" }
      libdirs
      {
         "external/sdl2/sdl/build/Release"
      }
      links 
      { 
         "SDL2-static",
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
   filter { "system:macosx" }
      buildoptions { "-fobjc-arc", "-arch x86_64", "-arch arm64" } 
      libdirs
      {
         "external/sdl2/sdl/build"
      }
      links
      {
         "SDL2-2.0.0",
         "Cocoa.framework",
         "CoreAudio.framework",
      }

end

-- Vulkan is supported for all desktop platforms.
function vulkan()

   defines { "MG_VULKAN" }

   filter "system:macosx"
      prebuildcommands { 'find ../vulkan -type f -name "*.vk.mgfxo" -print0 | xargs -0 -I {} xxd -i {} {}.h ' }

   files 
   { 
      "vulkan/**.h",
      "vulkan/**.cpp",
   }

   includedirs 
   {
      "external/vulkan-headers/include",      
      "external/volk",      
      "external/vma/include",      
   }

   filter "system:windows"
      files { "vulkan/**.rc", }
end


-- FAudio is supported for all desktop platforms.
function faudio()

   defines { "MG_FAUDIO" }

   files 
   { 
      "faudio/**.h",
      "faudio/**.cpp",
   }

end

function configs()

   filter "configurations:Debug"
      defines { "DEBUG" }
      symbols "On"

   filter "configurations:Release"
      defines { "NDEBUG" }
      optimize "On"

end

workspace "monogame"
   location "generated"
   configurations { "Debug", "Release" }
   platforms { "x86", "x86_64", "arm", "arm64", "universal" }
   -- Architecture-specific global settings
   filter "platforms:x86"
      architecture "x86"

   filter "platforms:x86_64"
      architecture "x86_64"

   filter "platforms:arm"
      architecture "arm"

   filter "platforms:arm64"
      architecture "arm64"

   filter "platforms:universal"
      architecture "universal"

project "desktopvk"
   removeplatforms { "x86", "arm", "arm64", "x86_64" }
   common("desktopvk")
   sdl2()
   vulkan()
   faudio()
   configs()

if os.target() == "windows" then
   project "windowsdx"
      removeplatforms { "x86", "arm" }
      defines { "MG_WINDOWSDX" }
      common("windowsdx")
else
   print("Skipping windowsdx: Not building on Windows")
end

-- project "androidvk"
--    removeplatforms { "universal" }
--    common("androidvk")
--    android()
--    vulkan()
--    faudio()
--    configs()
