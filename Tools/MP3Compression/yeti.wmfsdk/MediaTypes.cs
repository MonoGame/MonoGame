//  Widows Media Format Utils classes
//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.
//
//  Email:  yetiicb@hotmail.com
//
//  Copyright (C) 2002-2004 Idael Cardoso. 
//

using System;

namespace Yeti.WMFSdk
{
	/// <summary>
	/// Media types used to define the format of media used with 
	/// the Windows Media Format SDK
	/// </summary>
  public sealed class MediaTypes
  {
    private MediaTypes(){}

    public static Guid MEDIASUBTYPE_I420 { get { return new Guid("30323449-0000-0010-8000-00AA00389B71"); } }
    public static Guid MEDIASUBTYPE_IYUV { get { return new Guid("56555949-0000-0010-8000-00AA00389B71"); } }
    public static Guid MEDIASUBTYPE_RGB1 { get { return new Guid("E436EB78-524F-11CE-9F53-0020AF0BA770"); } }
    public static Guid MEDIASUBTYPE_RGB24 { get { return new Guid("E436EB7D-524F-11CE-9F53-0020AF0BA770"); } }
    public static Guid MEDIASUBTYPE_RGB32 { get { return new Guid("E436EB7E-524F-11CE-9F53-0020AF0BA770"); } }
    public static Guid MEDIASUBTYPE_RGB4 { get { return new Guid("E436EB79-524F-11CE-9F53-0020AF0BA770"); } }  
    public static Guid MEDIASUBTYPE_RGB555 { get { return new Guid("E436EB7C-524F-11CE-9F53-0020AF0BA770"); } }    
    public static Guid MEDIASUBTYPE_RGB565 { get { return new Guid("E436EB7B-524F-11CE-9F53-0020AF0BA770"); } }

    public static Guid MEDIASUBTYPE_RGB8 { get { return new Guid("E436EB7A-524F-11CE-9F53-0020AF0BA770"); } }
    public static Guid MEDIASUBTYPE_UYVY { get { return new Guid("59565955-0000-0010-8000-00AA00389B71"); } }
    public static Guid MEDIASUBTYPE_VIDEOIMAGE { get { return new Guid("1D4A45F2-E5F6-4B44-8388-F0AE5C0E0C37"); } }
    public static Guid MEDIASUBTYPE_YUY2 { get { return new Guid("32595559-0000-0010-8000-00AA00389B71"); } }
    public static Guid MEDIASUBTYPE_YV12 { get { return new Guid("31313259-0000-0010-8000-00AA00389B71"); } }
    public static Guid MEDIASUBTYPE_YVU9 { get { return new Guid("39555659-0000-0010-8000-00AA00389B71"); } }
    public static Guid MEDIASUBTYPE_YVYU { get { return new Guid("55595659-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMFORMAT_MPEG2Video { get { return new Guid("E06D80E3-DB46-11CF-B4D1-00805F6CBBEA"); } }
    public static Guid WMFORMAT_Script { get { return new Guid("5C8510F2-DEBE-4CA7-BBA5-F07A104F8DFF"); } }
    public static Guid WMFORMAT_VideoInfo { get { return new Guid("05589F80-C356-11CE-BF01-00AA0055595A"); } }
    public static Guid WMFORMAT_WaveFormatEx { get { return new Guid("05589F81-C356-11CE-BF01-00AA0055595A"); } }
    public static Guid WMFORMAT_WebStream { get { return new Guid("DA1E6B13-8359-4050-B398-388E965BF00C"); } }
    public static Guid WMMEDIASUBTYPE_ACELPnet { get { return new Guid("00000130-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIASUBTYPE_Base { get { return new Guid("00000000-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIASUBTYPE_DRM { get { return new Guid("00000009-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIASUBTYPE_MP3 { get { return new Guid("00000050-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIASUBTYPE_MP43 { get { return new Guid("3334504D-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIASUBTYPE_MP4S { get { return new Guid("5334504D-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIASUBTYPE_MPEG2_VIDEO { get { return new Guid("E06D8026-DB46-11CF-B4D1-00805F6CBBEA"); } }
    public static Guid WMMEDIASUBTYPE_MSS1 { get { return new Guid("3153534D-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIASUBTYPE_MSS2 { get { return new Guid("3253534D-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIASUBTYPE_PCM { get { return new Guid("00000001-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIASUBTYPE_WebStream { get { return new Guid("776257D4-C627-41CB-8F81-7AC7FF1C40CC"); } }
    public static Guid WMMEDIASUBTYPE_WMAudio_Lossless { get { return new Guid("00000163-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIASUBTYPE_WMAudioV2 { get { return new Guid("00000161-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIASUBTYPE_WMAudioV7 { get { return new Guid("00000161-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIASUBTYPE_WMAudioV8 { get { return new Guid("00000161-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIASUBTYPE_WMAudioV9 { get { return new Guid("00000162-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIASUBTYPE_WMSP1 { get { return new Guid("0000000A-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIASUBTYPE_WMV1 { get { return new Guid("31564D57-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIASUBTYPE_WMV2 { get { return new Guid("32564D57-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIASUBTYPE_WMV3 { get { return new Guid("33564D57-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIASUBTYPE_WMVP { get { return new Guid("50564D57-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIATYPE_Audio { get { return new Guid("73647561-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIATYPE_FileTransfer { get { return new Guid("D9E47579-930E-4427-ADFC-AD80F290E470"); } }
    public static Guid WMMEDIATYPE_Image { get { return new Guid("34A50FD8-8AA5-4386-81FE-A0EFE0488E31"); } }
    public static Guid WMMEDIATYPE_Script { get { return new Guid("73636D64-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMMEDIATYPE_Text { get { return new Guid("9BBA1EA7-5AB2-4829-BA57-0940209BCF3E"); } }
    public static Guid WMMEDIATYPE_Video { get { return new Guid("73646976-0000-0010-8000-00AA00389B71"); } }
    public static Guid WMSCRIPTTYPE_TwoStrings { get { return new Guid("82F38A70-C29F-11D1-97AD-00A0C95EA850"); } }
	}
}
