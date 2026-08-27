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

set(SDL_SHARED OFF CACHE BOOL "" FORCE)
set(SDL_STATIC ON CACHE BOOL "" FORCE)
set(SDL_TEST OFF CACHE BOOL "" FORCE)
set(SDL_TESTS OFF CACHE BOOL "" FORCE)
set(SDL_INSTALL_TESTS OFF CACHE BOOL "" FORCE)
set(SDL_ASSEMBLY OFF CACHE BOOL "" FORCE)
set(SDL_PTHREADS OFF CACHE BOOL "" FORCE)
set(SDL_CMAKE_DEBUG_POSTFIX "" CACHE STRING "" FORCE)

add_subdirectory("${MONOGAME_NATIVE_ROOT}/external/sdl2/sdl" "${CMAKE_CURRENT_BINARY_DIR}/sdl")

target_compile_options(SDL2-static PRIVATE "-sUSE_SDL=0" "-Wno-warn-absolute-paths")
set_target_properties(SDL2-static PROPERTIES
    DEBUG_POSTFIX ""
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
    "${MONOGAME_NATIVE_ROOT}/external/sdl2/sdl/include"
    "${MONOGAME_SOURCE_ROOT}/external/stb")

set_target_properties(mgruntime PROPERTIES
    PREFIX ""
    OUTPUT_NAME "mgruntime"
    ARCHIVE_OUTPUT_DIRECTORY "${MONOGAME_BROWSER_OUTPUT_ROOT}/$<CONFIG>")
