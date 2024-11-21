-- MonoGame - Copyright (C) MonoGame Foundation, Inc
-- This file is subject to the terms and conditions defined in
-- file 'LICENSE.txt', which is part of this source code package.

function common(project_name)

   platform_target_path = "../../Artifacts/monogame.native/%{cfg.system}/" .. project_name .. "/%{cfg.buildcfg}"

   kind "SharedLib"
   language "C++"
   architecture "x64"
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
      links 
      { 
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

end

-- Vulkan is supported for all desktop platforms.
function vulkan()

   defines { "MG_VULKAN" }

   files 
   { 
      "vulkan/**.h",
      "vulkan/**.cpp",
   }

   filter "system:windows"
      files { "vulkan/**.rc", }

   includedirs 
   {
      "external/vulkan-headers/include",      
      "external/volk",      
      "external/vma/include",      
   }

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
   configurations { "Debug", "Release" }

project "desktopvk"
   common("desktopvk")
   sdl2()
   vulkan()
   faudio()
   configs()


project "windowsdx"
   common("windowsdx")
