// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#pragma once

typedef unsigned short       mgchar;
typedef unsigned char        mgbyte;
typedef short                mgshort;
typedef unsigned short       mgushort;
typedef int                  mgint;
typedef unsigned int         mguint;
typedef long long            mglong;
typedef unsigned long long   mgulong;
typedef bool                 mgbool;
typedef float                mgfloat;
typedef double               mgdouble;


#define MG_FIELD_OFFSET(off, type, name) struct { char _pad_##name[off]; type name; }

#if defined(_WIN32) || defined(__CYGWIN__)
#ifdef DLL_EXPORT
#ifdef __GNUC__
#define MG_API __attribute__((dllexport))
#else
#define MG_API __declspec(dllexport)
#endif
#else
#ifdef __GNUC__
#define MG_API __attribute__((dllimport))
#else
#define MG_API __declspec(dllimport)
#endif
#endif
#else
#if __GNUC__ >= 4
#define MG_API __attribute__((visibility("default")))
#else
#define MG_API
#endif
#endif

#ifdef __cplusplus
#define MG_EXPORT extern "C" MG_API
#else
#define MG_EXPORT MG_API
#endif
