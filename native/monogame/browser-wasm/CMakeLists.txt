cmake_minimum_required(VERSION 3.20)

project(MonoGameBrowserWasm LANGUAGES C CXX)

if(NOT CMAKE_SYSTEM_NAME STREQUAL "Emscripten")
    message(FATAL_ERROR "MonoGame browser-wasm native archives must be configured with the Emscripten toolchain.")
endif()

if(NOT CMAKE_CONFIGURATION_TYPES AND NOT CMAKE_BUILD_TYPE)
    set(CMAKE_BUILD_TYPE Release CACHE STRING "Build type" FORCE)
endif()

set(CMAKE_C_STANDARD 11)
set(CMAKE_C_STANDARD_REQUIRED ON)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

get_filename_component(MONOGAME_NATIVE_ROOT "${CMAKE_CURRENT_LIST_DIR}/.." ABSOLUTE)
get_filename_component(MONOGAME_SOURCE_ROOT "${MONOGAME_NATIVE_ROOT}/../.." ABSOLUTE)
set(MONOGAME_BROWSER_OUTPUT_ROOT "${MONOGAME_SOURCE_ROOT}/Artifacts/native/mgruntime/webgl2/browser-wasm")
set(_monogameSdl2Root "${MONOGAME_NATIVE_ROOT}/external/sdl2/sdl")

# Keep this aligned with SDL's Emscripten source selection when the vendored
# SDL version changes. Add missing source folders here if a later SDL update
# fails to link because the curated browser list is incomplete.
file(GLOB MONOGAME_BROWSER_SDL2_SOURCES
    "${_monogameSdl2Root}/src/*.c"
    "${_monogameSdl2Root}/src/atomic/*.c"
    "${_monogameSdl2Root}/src/audio/*.c"
    "${_monogameSdl2Root}/src/audio/dummy/*.c"
    "${_monogameSdl2Root}/src/audio/disk/*.c"
    "${_monogameSdl2Root}/src/audio/emscripten/*.c"
    "${_monogameSdl2Root}/src/cpuinfo/*.c"
    "${_monogameSdl2Root}/src/dynapi/*.c"
    "${_monogameSdl2Root}/src/events/*.c"
    "${_monogameSdl2Root}/src/file/*.c"
    "${_monogameSdl2Root}/src/filesystem/emscripten/*.c"
    "${_monogameSdl2Root}/src/haptic/*.c"
    "${_monogameSdl2Root}/src/haptic/dummy/*.c"
    "${_monogameSdl2Root}/src/joystick/*.c"
    "${_monogameSdl2Root}/src/joystick/emscripten/*.c"
    "${_monogameSdl2Root}/src/libm/*.c"
    "${_monogameSdl2Root}/src/loadso/dlopen/*.c"
    "${_monogameSdl2Root}/src/locale/*.c"
    "${_monogameSdl2Root}/src/locale/emscripten/*.c"
    "${_monogameSdl2Root}/src/misc/*.c"
    "${_monogameSdl2Root}/src/misc/emscripten/*.c"
    "${_monogameSdl2Root}/src/power/*.c"
    "${_monogameSdl2Root}/src/power/emscripten/*.c"
    "${_monogameSdl2Root}/src/render/*.c"
    "${_monogameSdl2Root}/src/render/opengles2/*.c"
    "${_monogameSdl2Root}/src/render/software/*.c"
    "${_monogameSdl2Root}/src/sensor/*.c"
    "${_monogameSdl2Root}/src/sensor/dummy/*.c"
    "${_monogameSdl2Root}/src/stdlib/*.c"
    "${_monogameSdl2Root}/src/thread/*.c"
    "${_monogameSdl2Root}/src/thread/generic/*.c"
    "${_monogameSdl2Root}/src/timer/*.c"
    "${_monogameSdl2Root}/src/timer/unix/*.c"
    "${_monogameSdl2Root}/src/video/*.c"
    "${_monogameSdl2Root}/src/video/emscripten/*.c"
    "${_monogameSdl2Root}/src/video/yuv2rgb/*.c")

add_library(SDL2-static STATIC ${MONOGAME_BROWSER_SDL2_SOURCES})

target_compile_definitions(SDL2-static PRIVATE SDL_STATIC_LIB)
target_compile_options(SDL2-static PRIVATE
    "-sUSE_SDL=0"
    "-Wno-warn-absolute-paths"
    "-idirafter"
    "${_monogameSdl2Root}/src/video/khronos")
target_include_directories(SDL2-static PUBLIC "${_monogameSdl2Root}/include")
set_target_properties(SDL2-static PROPERTIES
    DEBUG_POSTFIX ""
    OUTPUT_NAME "SDL2"
    ARCHIVE_OUTPUT_DIRECTORY "${MONOGAME_BROWSER_OUTPUT_ROOT}/$<CONFIG>")

add_library(mgruntime STATIC
    "${MONOGAME_NATIVE_ROOT}/common/MGI.cpp"
    "${MONOGAME_NATIVE_ROOT}/common/MG_Asset.cpp"
    "${MONOGAME_NATIVE_ROOT}/common/mg_audio.cpp"
    "${MONOGAME_NATIVE_ROOT}/common/mg_hash.cpp"
    "${MONOGAME_NATIVE_ROOT}/common/mg_library.cpp"
    "${MONOGAME_NATIVE_ROOT}/opengl/OpenGLContext.cpp"
    "${MONOGAME_NATIVE_ROOT}/opengl/MGG_OpenGL.cpp"
    "${MONOGAME_NATIVE_ROOT}/sdl/MGP_sdl.cpp"
    "${MONOGAME_NATIVE_ROOT}/web/MGA_web.cpp"
    "${MONOGAME_NATIVE_ROOT}/web/MGM_web.cpp"
    "${MONOGAME_NATIVE_ROOT}/web/MGP_web.cpp")

target_compile_definitions(mgruntime PRIVATE DLL_EXPORT MG_SDL2 MG_OPENGL)
target_compile_options(mgruntime PRIVATE "-Wno-warn-absolute-paths")
target_include_directories(mgruntime PRIVATE
    "${MONOGAME_NATIVE_ROOT}/include"
    "${MONOGAME_NATIVE_ROOT}/common"
    "${MONOGAME_NATIVE_ROOT}/sdl"
    "${MONOGAME_NATIVE_ROOT}/opengl"
    "${_monogameSdl2Root}/include"
    "${MONOGAME_SOURCE_ROOT}/external/stb")

set_target_properties(mgruntime PROPERTIES
    PREFIX ""
    OUTPUT_NAME "mgruntime"
    ARCHIVE_OUTPUT_DIRECTORY "${MONOGAME_BROWSER_OUTPUT_ROOT}/$<CONFIG>")
