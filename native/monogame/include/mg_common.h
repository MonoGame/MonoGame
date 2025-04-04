// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include "api_common.h"

#include <stdio.h>
#include <assert.h>

#include <vector>
#include <string>
#include <queue>
#include <map>
#include <functional>


#if defined(_WIN32)
#define MG_GENERATE_TRAP() __debugbreak()
#else
#define MG_GENERATE_TRAP() __builtin_trap()
#endif

#if defined(_WIN32)
#define MG_PATH_SEPARATOR "\\"
#else
#define MG_PATH_SEPARATOR "/"
#endif

inline void MG_Print_StdError(const char* file, int line, const char* message)
{
    fflush(stdout);
    fprintf(stderr, "%s(%d): %s\n", file, line, message);
    fflush(stderr);
}

inline void MG_Print_StdOut(const char* file, int line, const char* message)
{
    fprintf(stdout, "%s(%d): %s\n", file, line, message);
    fflush(stdout);
}

#define MG_PRINT(msg) \
    MG_Print_StdOut(__FILE__, __LINE__, msg)

#define MG_ERROR_PRINT(msg) \
    MG_Print_StdError(__FILE__, __LINE__, msg)

#define MG_NOT_IMPLEMEMTED	MG_ERROR_PRINT("NOT IMPLEMENTED!"); MG_GENERATE_TRAP()


mguint MG_ComputeHash(const mgbyte* value, mgint length);
mguint MG_ComputeHash(mguint value, mguint result = 0x811c9dc5);

template <class T>
void mg_remove(std::vector<T>& vector, const T& element)
{
    auto new_end = std::remove(vector.begin(), vector.end(), element);
    assert(new_end != vector.end());
    vector.erase(new_end, vector.end());
}

template <class T>
bool mg_contains(std::vector<T>& vector, const T& element)
{
    auto found = std::find(vector.begin(), vector.end(), element);
    return found != vector.end();
}



