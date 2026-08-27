set(_monogameDotNetRootCandidates)

if(DEFINED ENV{DOTNET_ROOT} AND NOT "$ENV{DOTNET_ROOT}" STREQUAL "")
    list(APPEND _monogameDotNetRootCandidates "$ENV{DOTNET_ROOT}")
endif()

if(CMAKE_HOST_WIN32)
    list(APPEND _monogameDotNetRootCandidates "C:/Program Files/dotnet")
elseif(CMAKE_HOST_APPLE)
    list(APPEND _monogameDotNetRootCandidates "/usr/local/share/dotnet" "/usr/local/share/dotnet/x64")
else()
    list(APPEND _monogameDotNetRootCandidates "/usr/share/dotnet")
endif()

function(monogame_find_latest_path outputVariable globPattern description)
    set(matches)

    foreach(dotnetRoot IN LISTS _monogameDotNetRootCandidates)
        if(EXISTS "${dotnetRoot}")
            file(GLOB currentMatches LIST_DIRECTORIES TRUE "${dotnetRoot}/${globPattern}")
            list(APPEND matches ${currentMatches})
        endif()
    endforeach()

    list(REMOVE_DUPLICATES matches)

    if(NOT matches)
        message(FATAL_ERROR "Could not locate ${description} in the installed .NET workload. Set an explicit override before configuring CMake.")
    endif()

    list(SORT matches COMPARE NATURAL ORDER DESCENDING)
    list(GET matches 0 latestMatch)
    set(${outputVariable} "${latestMatch}" PARENT_SCOPE)
endfunction()

function(monogame_write_emscripten_wrapper wrapperPath toolPath)
    if(CMAKE_HOST_WIN32)
        file(TO_NATIVE_PATH "${toolPath}" nativeToolPath)
        file(TO_NATIVE_PATH "${_monogameEmscriptenToolsRoot}/bin" nativeLlvmRoot)
        file(TO_NATIVE_PATH "${_monogameEmscriptenToolsRoot}" nativeBinaryenRoot)
        file(TO_NATIVE_PATH "${DOTNET_EMSCRIPTEN_NODE_JS}" nativeNodePath)
        file(TO_NATIVE_PATH "${DOTNET_EMSCRIPTEN_PYTHON}" nativePythonPath)
        file(TO_NATIVE_PATH "${_monogamePythonRoot}" nativePythonRoot)
        file(TO_NATIVE_PATH "${DOTNET_EMSCRIPTEN_CONFIG_PATH}" nativeEmConfigPath)
        file(TO_NATIVE_PATH "${DOTNET_EMSCRIPTEN_CACHE_ROOT}" nativeEmCachePath)
        file(TO_NATIVE_PATH "${DOTNET_EMSCRIPTEN_TEMP_ROOT}" nativeEmTempPath)

        file(WRITE "${wrapperPath}" "@echo off\n")
        file(APPEND "${wrapperPath}" "setlocal\n")
        file(APPEND "${wrapperPath}" "set DOTNET_EMSCRIPTEN_LLVM_ROOT=${nativeLlvmRoot}\n")
        file(APPEND "${wrapperPath}" "set DOTNET_EMSCRIPTEN_BINARYEN_ROOT=${nativeBinaryenRoot}\n")
        file(APPEND "${wrapperPath}" "set DOTNET_EMSCRIPTEN_NODE_JS=${nativeNodePath}\n")
        file(APPEND "${wrapperPath}" "set EMSDK_PYTHON=${nativePythonPath}\n")
        file(APPEND "${wrapperPath}" "set PYTHONPATH=${nativePythonRoot}\n")
        file(APPEND "${wrapperPath}" "set PYTHONHOME=\n")
        file(APPEND "${wrapperPath}" "set EM_CONFIG=${nativeEmConfigPath}\n")
        file(APPEND "${wrapperPath}" "set EM_CACHE=${nativeEmCachePath}\n")
        file(APPEND "${wrapperPath}" "set EMCC_TEMP_DIR=${nativeEmTempPath}\n")
        file(APPEND "${wrapperPath}" "\"${nativeToolPath}\" %*\n")
    else()
        file(WRITE "${wrapperPath}" "#!/bin/sh\n")
        file(APPEND "${wrapperPath}" "export DOTNET_EMSCRIPTEN_LLVM_ROOT='${_monogameEmscriptenToolsRoot}/bin'\n")
        file(APPEND "${wrapperPath}" "export DOTNET_EMSCRIPTEN_BINARYEN_ROOT='${_monogameEmscriptenToolsRoot}'\n")
        file(APPEND "${wrapperPath}" "export DOTNET_EMSCRIPTEN_NODE_JS='${DOTNET_EMSCRIPTEN_NODE_JS}'\n")
        file(APPEND "${wrapperPath}" "export EMSDK_PYTHON='${DOTNET_EMSCRIPTEN_PYTHON}'\n")
        file(APPEND "${wrapperPath}" "export PYTHONPATH='${_monogamePythonRoot}'\n")
        file(APPEND "${wrapperPath}" "export PYTHONHOME=''\n")
        file(APPEND "${wrapperPath}" "export EM_CONFIG='${DOTNET_EMSCRIPTEN_CONFIG_PATH}'\n")
        file(APPEND "${wrapperPath}" "export EM_CACHE='${DOTNET_EMSCRIPTEN_CACHE_ROOT}'\n")
        file(APPEND "${wrapperPath}" "export EMCC_TEMP_DIR='${DOTNET_EMSCRIPTEN_TEMP_ROOT}'\n")
        file(APPEND "${wrapperPath}" "exec \"${toolPath}\" \"$@\"\n")
        file(CHMOD "${wrapperPath}" FILE_PERMISSIONS OWNER_READ OWNER_WRITE OWNER_EXECUTE GROUP_READ GROUP_EXECUTE WORLD_READ WORLD_EXECUTE)
    endif()
endfunction()

if(NOT DEFINED DOTNET_EMSCRIPTEN_ROOT_PATH)
    monogame_find_latest_path(
        DOTNET_EMSCRIPTEN_ROOT_PATH
        "packs/Microsoft.NET.Runtime.Emscripten.*.Sdk.*/*/tools/emscripten"
        "the .NET Emscripten SDK")
endif()

if(NOT DEFINED DOTNET_EMSCRIPTEN_NODE_JS)
    monogame_find_latest_path(
        DOTNET_EMSCRIPTEN_NODE_JS
        "packs/Microsoft.NET.Runtime.Emscripten.*.Node.*/*/tools/bin/node*"
        "the .NET Emscripten Node runtime")
endif()

if(NOT DEFINED DOTNET_EMSCRIPTEN_PYTHON)
    monogame_find_latest_path(
        DOTNET_EMSCRIPTEN_PYTHON
        "packs/Microsoft.NET.Runtime.Emscripten.*.Python.*/*/tools/python*"
        "the .NET Emscripten Python runtime")
endif()

if(NOT DEFINED DOTNET_EMSCRIPTEN_SEED_CACHE_ROOT)
    monogame_find_latest_path(
        DOTNET_EMSCRIPTEN_SEED_CACHE_ROOT
        "packs/Microsoft.NET.Runtime.Emscripten.*.Cache.*/*/tools/emscripten/cache"
        "the .NET Emscripten cache pack")
endif()

get_filename_component(DOTNET_EMSCRIPTEN_ROOT_PATH "${DOTNET_EMSCRIPTEN_ROOT_PATH}" ABSOLUTE)
get_filename_component(DOTNET_EMSCRIPTEN_SEED_CACHE_ROOT "${DOTNET_EMSCRIPTEN_SEED_CACHE_ROOT}" ABSOLUTE)
get_filename_component(_monogameEmscriptenToolsRoot "${DOTNET_EMSCRIPTEN_ROOT_PATH}/.." ABSOLUTE)
get_filename_component(_monogamePythonRoot "${DOTNET_EMSCRIPTEN_PYTHON}" DIRECTORY)

set(DOTNET_EMSCRIPTEN_CACHE_ROOT "${CMAKE_BINARY_DIR}/emscripten-cache" CACHE PATH "Writable Emscripten cache root")
set(DOTNET_EMSCRIPTEN_TEMP_ROOT "${CMAKE_BINARY_DIR}/emscripten-tmp" CACHE PATH "Writable Emscripten temp root")
set(DOTNET_EMSCRIPTEN_CONFIG_PATH "${CMAKE_BINARY_DIR}/.emscripten" CACHE FILEPATH "Generated Emscripten config")
set(DOTNET_EMSCRIPTEN_WRAPPER_ROOT "${CMAKE_BINARY_DIR}/emscripten-tools" CACHE PATH "Generated Emscripten wrapper root")

file(MAKE_DIRECTORY "${DOTNET_EMSCRIPTEN_CACHE_ROOT}")
file(MAKE_DIRECTORY "${DOTNET_EMSCRIPTEN_TEMP_ROOT}")
file(MAKE_DIRECTORY "${DOTNET_EMSCRIPTEN_WRAPPER_ROOT}")

if(NOT EXISTS "${DOTNET_EMSCRIPTEN_CACHE_ROOT}/sysroot_install.stamp")
    file(COPY "${DOTNET_EMSCRIPTEN_SEED_CACHE_ROOT}/" DESTINATION "${DOTNET_EMSCRIPTEN_CACHE_ROOT}")
endif()

file(READ "${DOTNET_EMSCRIPTEN_ROOT_PATH}/.emscripten" _monogameEmscriptenConfig)
string(REGEX REPLACE "FROZEN_CACHE = .+" "FROZEN_CACHE = False" _monogameEmscriptenConfig "${_monogameEmscriptenConfig}")
file(WRITE "${DOTNET_EMSCRIPTEN_CONFIG_PATH}" "${_monogameEmscriptenConfig}")

set(ENV{DOTNET_EMSCRIPTEN_LLVM_ROOT} "${_monogameEmscriptenToolsRoot}/bin")
set(ENV{DOTNET_EMSCRIPTEN_BINARYEN_ROOT} "${_monogameEmscriptenToolsRoot}")
set(ENV{DOTNET_EMSCRIPTEN_NODE_JS} "${DOTNET_EMSCRIPTEN_NODE_JS}")
set(ENV{EMSDK_PYTHON} "${DOTNET_EMSCRIPTEN_PYTHON}")
set(ENV{PYTHONPATH} "${_monogamePythonRoot}")
set(ENV{PYTHONHOME} "")
set(ENV{EM_CONFIG} "${DOTNET_EMSCRIPTEN_CONFIG_PATH}")
set(ENV{EM_CACHE} "${DOTNET_EMSCRIPTEN_CACHE_ROOT}")
set(ENV{EMCC_TEMP_DIR} "${DOTNET_EMSCRIPTEN_TEMP_ROOT}")

set(CMAKE_TRY_COMPILE_TARGET_TYPE STATIC_LIBRARY)
set(EMSCRIPTEN_FORCE_COMPILERS OFF CACHE BOOL "" FORCE)
set(EMSCRIPTEN_ROOT_PATH "${DOTNET_EMSCRIPTEN_ROOT_PATH}")
include("${DOTNET_EMSCRIPTEN_ROOT_PATH}/cmake/Modules/Platform/Emscripten.cmake")

if(CMAKE_HOST_WIN32)
    set(_monogameWrapperSuffix ".cmd")
    set(_monogameToolSuffix ".bat")
else()
    set(_monogameWrapperSuffix "")
    set(_monogameToolSuffix "")
endif()

set(_monogameEmccWrapper "${DOTNET_EMSCRIPTEN_WRAPPER_ROOT}/emcc${_monogameWrapperSuffix}")
set(_monogameEmxxWrapper "${DOTNET_EMSCRIPTEN_WRAPPER_ROOT}/emxx${_monogameWrapperSuffix}")
set(_monogameEmarWrapper "${DOTNET_EMSCRIPTEN_WRAPPER_ROOT}/emar${_monogameWrapperSuffix}")
set(_monogameEmranlibWrapper "${DOTNET_EMSCRIPTEN_WRAPPER_ROOT}/emranlib${_monogameWrapperSuffix}")
set(_monogameEmnmWrapper "${DOTNET_EMSCRIPTEN_WRAPPER_ROOT}/emnm${_monogameWrapperSuffix}")

monogame_write_emscripten_wrapper("${_monogameEmccWrapper}" "${DOTNET_EMSCRIPTEN_ROOT_PATH}/emcc${_monogameToolSuffix}")
monogame_write_emscripten_wrapper("${_monogameEmxxWrapper}" "${DOTNET_EMSCRIPTEN_ROOT_PATH}/em++${_monogameToolSuffix}")
monogame_write_emscripten_wrapper("${_monogameEmarWrapper}" "${DOTNET_EMSCRIPTEN_ROOT_PATH}/emar${_monogameToolSuffix}")
monogame_write_emscripten_wrapper("${_monogameEmranlibWrapper}" "${DOTNET_EMSCRIPTEN_ROOT_PATH}/emranlib${_monogameToolSuffix}")
monogame_write_emscripten_wrapper("${_monogameEmnmWrapper}" "${DOTNET_EMSCRIPTEN_ROOT_PATH}/emnm${_monogameToolSuffix}")

set(CMAKE_C_COMPILER "${_monogameEmccWrapper}" CACHE FILEPATH "" FORCE)
set(CMAKE_CXX_COMPILER "${_monogameEmxxWrapper}" CACHE FILEPATH "" FORCE)
set(CMAKE_AR "${_monogameEmarWrapper}" CACHE FILEPATH "" FORCE)
set(CMAKE_RANLIB "${_monogameEmranlibWrapper}" CACHE FILEPATH "" FORCE)
set(CMAKE_NM "${_monogameEmnmWrapper}" CACHE FILEPATH "" FORCE)
set(CMAKE_C_COMPILER_AR "${CMAKE_AR}" CACHE FILEPATH "" FORCE)
set(CMAKE_CXX_COMPILER_AR "${CMAKE_AR}" CACHE FILEPATH "" FORCE)
set(CMAKE_C_COMPILER_RANLIB "${CMAKE_RANLIB}" CACHE FILEPATH "" FORCE)
set(CMAKE_CXX_COMPILER_RANLIB "${CMAKE_RANLIB}" CACHE FILEPATH "" FORCE)
