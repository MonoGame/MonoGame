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

#ifdef DLL_EXPORT
#define MG_EXPORT extern "C" __declspec(dllexport)
#else
#define MG_EXPORT extern "C"
#endif
