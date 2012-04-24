/******************************************************************************

 @File         PVRTGlobal.h

 @Title        PVRTGlobal

 @Version      

 @Copyright    Copyright (c) Imagination Technologies Limited. All Rights Reserved. Strictly Confidential.

 @Platform     ANSI compatible

 @Description  Global defines and typedefs for PVRTools

******************************************************************************/
#ifndef _PVRTGLOBAL_H_
#define _PVRTGLOBAL_H_

/*!***************************************************************************
 Macros
*****************************************************************************/
#define PVRT_MIN(a,b)            (((a) < (b)) ? (a) : (b))
#define PVRT_MAX(a,b)            (((a) > (b)) ? (a) : (b))
#define PVRT_CLAMP(x, l, h)      (PVRT_MIN((h), PVRT_MAX((x), (l))))

// avoid warning about unused parameter
#define PVRT_UNREFERENCED_PARAMETER(x) ((void) x)

#if defined(_WIN32) && !defined(UNDER_CE) && !defined(__SYMBIAN32__) && !defined(__BADA__) && !defined(__QT__)	/* Windows desktop */
	#define _CRTDBG_MAP_ALLOC
	#include <windows.h>
	#include <crtdbg.h>
	#include <tchar.h>
#endif

#if defined(UNDER_CE)
	#include <windows.h>

#ifndef _ASSERT
	#ifdef _DEBUG
		#define _ASSERT(X) { (X) ? 0 : DebugBreak(); }
	#else
		#define _ASSERT(X)
	#endif
#endif

#ifndef _ASSERTE
	#ifdef _DEBUG
		#define _ASSERTE _ASSERT
	#else
		#define _ASSERTE(X)
	#endif
#endif
	#define _RPT0(a,b)
	#define _RPT1(a,b,c)
	#define _RPT2(a,b,c,d)
	#define _RPT3(a,b,c,d,e)
	#define _RPT4(a,b,c,d,e,f)
#else

#if defined(_WIN32) && !defined(__WINSCW__) && !defined(__BADA__) && !defined(__QT__)

#else
#if defined(__linux__) || defined(__APPLE__)
	#define _ASSERT(a)((void)0)
	#define _ASSERTE(a)((void)0)
	#ifdef _DEBUG
		#ifndef _RPT0
		#define _RPT0(a,b) printf(b)
		#endif
		#ifndef _RPT1
		#define _RPT1(a,b,c) printf(b,c)
		#endif
	#else
		#ifndef _RPT0
	    #define _RPT0(a,b)((void)0)
		#endif
		#ifndef _RPT1
	    #define _RPT1(a,b,c)((void)0)
		#endif
	#endif
	#define _RPT2(a,b,c,d)((void)0)
	#define _RPT3(a,b,c,d,e)((void)0)
	#define _RPT4(a,b,c,d,e,f)((void)0)
	#include <stdlib.h>
	#include <string.h>
	#define BYTE unsigned char
	#define WORD unsigned short
	#define DWORD unsigned int
	typedef struct tagRGBQUAD {
	BYTE    rgbBlue;
	BYTE    rgbGreen;
	BYTE    rgbRed;
	BYTE    rgbReserved;
	} RGBQUAD;
	#define BOOL int
	#define TRUE 1
	#define FALSE 0
#else
	#define _CRT_WARN 0
	#define _RPT0(a,b)
	#define _RPT1(a,b,c)
	#define _RPT2(a,b,c,d)
	#define _RPT3(a,b,c,d,e)
	#define _RPT4(a,b,c,d,e,f)
	#define _ASSERT(X)
	#define _ASSERTE(X)
#endif
#endif
#endif

#include <stdio.h>

#define FREE(X)		{ if(X) { free(X); (X) = 0; } }

// This macro is used to check at compile time that types are of a certain size
// If the size does not equal the expected size, this typedefs an array of size 0
// which causes a compile error
#define PVRTSIZEASSERT(T, size) typedef int (sizeof_##T)[sizeof(T) == (size)]


/****************************************************************************
** Integer types
****************************************************************************/

typedef char				PVRTchar8;
typedef signed char			PVRTint8;
typedef signed short		PVRTint16;
typedef signed int			PVRTint32;
typedef unsigned char		PVRTuint8;
typedef unsigned short		PVRTuint16;
typedef unsigned int		PVRTuint32;

typedef float				PVRTfloat32;

#if !defined(__BADA__) && (defined(__int64) || defined(_WIN32))
typedef signed __int64     PVRTint64;
typedef unsigned __int64   PVRTuint64;
#elif defined(TInt64)
typedef TInt64             PVRTint64;
typedef TUInt64            PVRTuint64;
#else
typedef signed long long   PVRTint64;
typedef unsigned long long PVRTuint64;
#endif

#if __SIZEOF_WCHAR_T__  == 4 || __WCHAR_MAX__ > 0x10000
	#define PVRTSIZEOFWCHAR 4
#else
	#define PVRTSIZEOFWCHAR 2
#endif

PVRTSIZEASSERT(PVRTchar8,   1);
PVRTSIZEASSERT(PVRTint8,   1);
PVRTSIZEASSERT(PVRTuint8,  1);
PVRTSIZEASSERT(PVRTint16,  2);
PVRTSIZEASSERT(PVRTuint16, 2);
PVRTSIZEASSERT(PVRTint32,  4);
PVRTSIZEASSERT(PVRTuint32, 4);
PVRTSIZEASSERT(PVRTint64,  8);
PVRTSIZEASSERT(PVRTuint64, 8);
PVRTSIZEASSERT(PVRTfloat32, 4);

/*!**************************************************************************
@Enum   ETextureFilter
@Brief  Enum values for defining texture filtering
****************************************************************************/
enum ETextureFilter
{
	eFilter_Nearest,
	eFilter_Linear,
	eFilter_None,

	eFilter_Size,
	eFilter_Default		= eFilter_Nearest,
	eFilter_MipDefault	= eFilter_None
};

/*!**************************************************************************
@Enum   ETextureWrap
@Brief  Enum values for defining texture wrapping
****************************************************************************/
enum ETextureWrap
{
	eWrap_Clamp,
	eWrap_Repeat,

	eWrap_Size,
	eWrap_Default = eWrap_Repeat
};

/****************************************************************************
** swap template function
****************************************************************************/
/*!***************************************************************************
 @Function		PVRTswap
 @Input			a Type a
 @Input			b Type b
 @Description	A swap template function that swaps a and b
*****************************************************************************/

template <typename T>
inline void PVRTswap(T& a, T& b)
{
	T temp = a;
	a = b;
	b = temp;
}

#ifdef _UITRON_
template void PVRTswap<unsigned char>(unsigned char& a, unsigned char& b);
#endif

/*!***************************************************************************
 @Function		PVRTClamp
 @Input			val		Value to clamp
 @Input			min		Minimum legal value
 @Input			max		Maximum legal value
 @Description	A clamp template function that clamps val between min and max.
*****************************************************************************/
template <typename T>
inline T PVRTClamp(const T& val, const T& min, const T& max)
{
	if(val > max)
		return max;
	if(val < min)
		return min;
	return val;
}

/*!***************************************************************************
 @Function		PVRTByteSwap
 @Input			pBytes A number
 @Input			i32ByteNo Number of bytes in pBytes
 @Description	Swaps the endianness of pBytes in place
*****************************************************************************/
inline void PVRTByteSwap(unsigned char* pBytes, int i32ByteNo)
{
	int i = 0, j = i32ByteNo - 1;

	while(i < j)
		PVRTswap<unsigned char>(pBytes[i++], pBytes[j--]);
}

/*!***************************************************************************
 @Function		PVRTByteSwap32
 @Input			ui32Long A number
 @Returns		ui32Long with its endianness changed
 @Description	Converts the endianness of an unsigned int
*****************************************************************************/
inline unsigned int PVRTByteSwap32(unsigned int ui32Long)
{
	return ((ui32Long&0x000000FF)<<24) + ((ui32Long&0x0000FF00)<<8) + ((ui32Long&0x00FF0000)>>8) + ((ui32Long&0xFF000000) >> 24);
}

/*!***************************************************************************
 @Function		PVRTByteSwap16
 @Input			ui16Short A number
 @Returns		ui16Short with its endianness changed
 @Description	Converts the endianness of a unsigned short
*****************************************************************************/
inline unsigned short PVRTByteSwap16(unsigned short ui16Short)
{
	return (ui16Short>>8) | (ui16Short<<8);
}

/*!***************************************************************************
 @Function		PVRTIsLittleEndian
 @Returns		True if the platform the code is ran on is little endian
 @Description	Returns true if the platform the code is ran on is little endian
*****************************************************************************/
inline bool PVRTIsLittleEndian()
{
	static bool bLittleEndian;
	static bool bIsInit = false;

	if(!bIsInit)
	{
		short int word = 0x0001;
		char *byte = (char*) &word;
		bLittleEndian = byte[0] ? true : false;
		bIsInit = true;
	}

	return bLittleEndian;
}

#endif // _PVRTGLOBAL_H_

/*****************************************************************************
 End of file (Tools.h)
*****************************************************************************/

