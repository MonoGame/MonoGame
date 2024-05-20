// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

#include <stdio.h>


#ifdef _MSC_VER
#define MG_GENERATE_TRAP() __debugbreak()
#else
#define MG_GENERATE_TRAP() __builtin_trap()
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







