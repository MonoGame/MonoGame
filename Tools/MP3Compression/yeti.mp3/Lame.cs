//
//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.
//
//  Email:  yetiicb@hotmail.com
//
//  Copyright (C) 2002-2003 Idael Cardoso. 
//  
//  LAME ( LAME Ain't an Mp3 Encoder ) 
//  You must call the fucntion "beVersion" to obtain information  like version 
//  numbers (both of the DLL and encoding engine), release date and URL for 
//  lame_enc's homepage. All this information should be made available to the 
//  user of your product through a dialog box or something similar.
//  You must see all information about LAME project and legal license infos at 
//  http://www.mp3dev.org/  The official LAME site
//
//
//  About Thomson and/or Fraunhofer patents:
//  Any use of this product does not convey a license under the relevant 
//  intellectual property of Thomson and/or Fraunhofer Gesellschaft nor imply 
//  any right to use this product in any finished end user or ready-to-use final 
//  product. An independent license for such use is required. 
//  For details, please visit http://www.mp3licensing.com.
//

using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using WaveLib;

namespace Yeti.Lame
{
  public enum VBRMETHOD : int
  {
    VBR_METHOD_NONE			= -1,
    VBR_METHOD_DEFAULT	=  0,
    VBR_METHOD_OLD			=  1,
    VBR_METHOD_NEW			=  2,
    VBR_METHOD_MTRH			=  3,
    VBR_METHOD_ABR			=  4
  } 

  /* MPEG modes */
  public enum MpegMode : uint 
  {
    STEREO = 0,
    JOINT_STEREO,
    DUAL_CHANNEL,   /* LAME doesn't supports this! */
    MONO,
    NOT_SET,
    MAX_INDICATOR   /* Don't use this! It's used for sanity checks. */ 
  }

  public enum LAME_QUALITY_PRESET : int
  {
    LQP_NOPRESET			=-1,
    // QUALITY PRESETS
    LQP_NORMAL_QUALITY		= 0,
    LQP_LOW_QUALITY			= 1,
    LQP_HIGH_QUALITY		= 2,
    LQP_VOICE_QUALITY		= 3,
    LQP_R3MIX				= 4,
    LQP_VERYHIGH_QUALITY	= 5,
    LQP_STANDARD			= 6,
    LQP_FAST_STANDARD		= 7,
    LQP_EXTREME				= 8,
    LQP_FAST_EXTREME		= 9,
    LQP_INSANE				= 10,
    LQP_ABR					= 11,
    LQP_CBR					= 12,
    LQP_MEDIUM				= 13,
    LQP_FAST_MEDIUM			= 14,
    // NEW PRESET VALUES
    LQP_PHONE	=1000,
    LQP_SW		=2000,
    LQP_AM		=3000,
    LQP_FM		=4000,
    LQP_VOICE	=5000,
    LQP_RADIO	=6000,
    LQP_TAPE	=7000,
    LQP_HIFI	=8000,
    LQP_CD		=9000,
    LQP_STUDIO	=10000
  } 
    
  [StructLayout(LayoutKind.Sequential), Serializable]
  public struct	MP3 //BE_CONFIG_MP3
  {
    public uint   dwSampleRate;		// 48000, 44100 and 32000 allowed
    public byte	  byMode;			// BE_MP3_MODE_STEREO, BE_MP3_MODE_DUALCHANNEL, BE_MP3_MODE_MONO
    public ushort	wBitrate;		// 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256 and 320 allowed
    public int	  bPrivate;		
    public int	  bCRC;
    public int	  bCopyright;
    public int	  bOriginal;
  } 

  [StructLayout(LayoutKind.Sequential, Size=327), Serializable]
  public struct LHV1 // BE_CONFIG_LAME LAME header version 1
  {
    public const uint MPEG1	= 1;
    public const uint MPEG2	= 0;

    // STRUCTURE INFORMATION
    public uint			dwStructVersion;	
    public uint			dwStructSize;
    // BASIC ENCODER SETTINGS
    public uint			dwSampleRate;		// SAMPLERATE OF INPUT FILE
    public uint			dwReSampleRate;		// DOWNSAMPLERATE, 0=ENCODER DECIDES  
    public MpegMode			nMode;				// STEREO, MONO
    public uint			dwBitrate;			// CBR bitrate, VBR min bitrate
    public uint			dwMaxBitrate;		// CBR ignored, VBR Max bitrate
    public LAME_QUALITY_PRESET			nPreset;			// Quality preset
    public uint			dwMpegVersion;		// MPEG-1 OR MPEG-2
    public uint			dwPsyModel;			// FUTURE USE, SET TO 0
    public uint			dwEmphasis;			// FUTURE USE, SET TO 0
    // BIT STREAM SETTINGS
    public int			bPrivate;			// Set Private Bit (TRUE/FALSE)
    public int			bCRC;				// Insert CRC (TRUE/FALSE)
    public int			bCopyright;			// Set Copyright Bit (TRUE/FALSE)
    public int			bOriginal;			// Set Original Bit (TRUE/FALSE)
    // VBR STUFF
    public int			bWriteVBRHeader;	// WRITE XING VBR HEADER (TRUE/FALSE)
    public int			bEnableVBR;			// USE VBR ENCODING (TRUE/FALSE)
    public int			nVBRQuality;		// VBR QUALITY 0..9
    public uint			dwVbrAbr_bps;		// Use ABR in stead of nVBRQuality
    public VBRMETHOD		nVbrMethod;
    public int			bNoRes;				// Disable Bit resorvoir (TRUE/FALSE)
    // MISC SETTINGS
    public int			bStrictIso;			// Use strict ISO encoding rules (TRUE/FALSE)
    public ushort		nQuality;			// Quality Setting, HIGH BYTE should be NOT LOW byte, otherwhise quality=5
    // FUTURE USE, SET TO 0, align strucutre to 331 bytes
    //[ MarshalAs( UnmanagedType.ByValArray, SizeConst=255-4*4-2 )]
    //public byte[]   btReserved;//[255-4*sizeof(DWORD) - sizeof( WORD )];
    public LHV1(WaveFormat format, uint MpeBitRate)
    {
      if ( format.wFormatTag != (short)WaveFormats.Pcm )
      {
        throw new ArgumentOutOfRangeException("format", "Only PCM format supported");
      }
      if ( format.wBitsPerSample != 16)
      {
        throw new ArgumentOutOfRangeException("format", "Only 16 bits samples supported");
      }
      dwStructVersion	= 1;
      dwStructSize		= (uint)Marshal.SizeOf(typeof(BE_CONFIG));
      switch (format.nSamplesPerSec)
      {
        case 16000 :
        case 22050 :
        case 24000 :
          dwMpegVersion		= MPEG2;
          break;
        case 32000 :
        case 44100 :
        case 48000 :
          dwMpegVersion		= MPEG1;
          break;
        default :
          throw new ArgumentOutOfRangeException("format", "Unsupported sample rate");
      }
      dwSampleRate = (uint)format.nSamplesPerSec;				// INPUT FREQUENCY
      dwReSampleRate = 0;					// DON'T RESAMPLE
      switch (format.nChannels)
      {
        case 1 :
          nMode	=	MpegMode.MONO;
          break;
        case 2 :
          nMode = MpegMode.STEREO;
          break;
        default:
          throw new ArgumentOutOfRangeException("format", "Invalid number of channels");
      }
      switch (MpeBitRate)
      {
        case 32 :
        case 40 :
        case 48 :
        case 56 :
        case 64 :
        case 80 :
        case 96 :
        case 112 :
        case 128 :
        case 160 : //Allowed bit rates in MPEG1 and MPEG2
          break; 
        case 192 :
        case 224 :
        case 256 :
        case 320 : //Allowed only in MPEG1
          if (dwMpegVersion	!= MPEG1)
          {
            throw new ArgumentOutOfRangeException("MpsBitRate", "Bit rate not compatible with input format");
          }
          break;
        case 8 :
        case 16 :
        case 24 :
        case 144 : //Allowed only in MPEG2
          if (dwMpegVersion	!= MPEG2)
          {
            throw new ArgumentOutOfRangeException("MpsBitRate", "Bit rate not compatible with input format");
          }
          break;
        default :
          throw new ArgumentOutOfRangeException("MpsBitRate", "Unsupported bit rate");
      }
      dwBitrate	= MpeBitRate;					// MINIMUM BIT RATE
      nPreset = LAME_QUALITY_PRESET.LQP_NORMAL_QUALITY;		// QUALITY PRESET SETTING
      dwPsyModel = 0;					// USE DEFAULT PSYCHOACOUSTIC MODEL 
      dwEmphasis = 0;					// NO EMPHASIS TURNED ON
      bOriginal = 1;					// SET ORIGINAL FLAG
      bWriteVBRHeader	= 0;					
      bNoRes = 0;					// No Bit resorvoir
      bCopyright = 0;
      bCRC = 0;
      bEnableVBR = 0;
      bPrivate = 0;
      bStrictIso = 0;
      dwMaxBitrate = 0;
      dwVbrAbr_bps = 0;
      nQuality = 0;
      nVbrMethod = VBRMETHOD.VBR_METHOD_NONE;
      nVBRQuality = 0;
    }
  }					

  
  [StructLayout(LayoutKind.Sequential), Serializable]
  public struct	ACC
  {
    public uint	dwSampleRate;
    public byte	byMode;
    public ushort	wBitrate;
    public byte	byEncodingMethod;
  }

  [StructLayout(LayoutKind.Explicit), Serializable]
  public class Format
  {
    [FieldOffset(0)] 
    public MP3 mp3;
    [FieldOffset(0)]
    public LHV1 lhv1;
    [FieldOffset(0)]
    public ACC acc;

    public Format(WaveFormat format, uint MpeBitRate)
    {
      lhv1 = new LHV1(format, MpeBitRate);
    }
  }

  [StructLayout(LayoutKind.Sequential), Serializable]
  public class BE_CONFIG
  {
    // encoding formats
    public const uint BE_CONFIG_MP3	 = 0;
    public const uint BE_CONFIG_LAME = 256;

    public uint	dwConfig;	
    public Format format;

    public BE_CONFIG(WaveFormat format, uint MpeBitRate)
    {
      this.dwConfig = BE_CONFIG_LAME;
      this.format = new Format(format, MpeBitRate);
    }
    public BE_CONFIG(WaveFormat format)
      : this(format, 128)
    {
    }
  }
    
  [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
  public class BE_VERSION
  { 
    public const uint BE_MAX_HOMEPAGE	= 256;
    public byte	byDLLMajorVersion;
    public byte	byDLLMinorVersion;
    public byte	byMajorVersion;
    public byte	byMinorVersion;
    // DLL Release date
    public byte	byDay;
    public byte	byMonth;
    public ushort	wYear;
    //Homepage URL
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst=257/*BE_MAX_HOMEPAGE+1*/)]
    public string	zHomepage;	
    public byte	byAlphaLevel;
    public byte	byBetaLevel;
    public byte	byMMXEnabled;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst=125)]
    public byte[]	btReserved;
    public BE_VERSION()
    {
      btReserved = new byte[125];
    }
  }

  /// <summary>
	/// Lame_enc DLL functions
	/// </summary>
	public class Lame_encDll
	{
    //Error codes
    public const uint BE_ERR_SUCCESSFUL = 0;
    public const uint BE_ERR_INVALID_FORMAT = 1;
    public const uint BE_ERR_INVALID_FORMAT_PARAMETERS = 2;
    public const uint BE_ERR_NO_MORE_HANDLES = 3;
    public const uint BE_ERR_INVALID_HANDLE = 4;

    /// <summary>
    /// This function is the first to call before starting an encoding stream.
    /// </summary>
    /// <param name="pbeConfig">Encoder settings</param>
    /// <param name="dwSamples">Receives the number of samples (not bytes, each sample is a SHORT) to send to each beEncodeChunk() on return.</param>
    /// <param name="dwBufferSize">Receives the minimun number of bytes that must have the output(result) buffer</param>
    /// <param name="phbeStream">Receives the stream handle on return</param>
    /// <returns>On success: BE_ERR_SUCCESSFUL</returns>
    [DllImport("Lame_enc.dll")]
    public static extern uint beInitStream(BE_CONFIG pbeConfig, ref uint dwSamples, ref uint dwBufferSize, ref uint phbeStream);
    /// <summary>
    /// Encodes a chunk of samples. Please note that if you have set the output to 
    /// generate mono MP3 files you must feed beEncodeChunk() with mono samples
    /// </summary>
    /// <param name="hbeStream">Handle of the stream.</param>
    /// <param name="nSamples">Number of samples to be encoded for this call. 
    /// This should be identical to what is returned by beInitStream(), 
    /// unless you are encoding the last chunk, which might be smaller.</param>
    /// <param name="pInSamples">Array of 16-bit signed samples to be encoded. 
    /// These should be in stereo when encoding a stereo MP3 
    /// and mono when encoding a mono MP3</param>
    /// <param name="pOutput">Buffer where to write the encoded data. 
    /// This buffer should be at least of the minimum size returned by beInitStream().</param>
    /// <param name="pdwOutput">Returns the number of bytes of encoded data written. 
    /// The amount of data written might vary from chunk to chunk</param>
    /// <returns>On success: BE_ERR_SUCCESSFUL</returns>
    [DllImport("Lame_enc.dll")]
    public static extern uint beEncodeChunk(uint hbeStream, uint nSamples, short[] pInSamples, [In, Out] byte[] pOutput, ref uint pdwOutput);
    /// <summary>
    /// Encodes a chunk of samples. Please note that if you have set the output to 
    /// generate mono MP3 files you must feed beEncodeChunk() with mono samples
    /// </summary>
    /// <param name="hbeStream">Handle of the stream.</param>
    /// <param name="nSamples">Number of samples to be encoded for this call. 
    /// This should be identical to what is returned by beInitStream(), 
    /// unless you are encoding the last chunk, which might be smaller.</param>
    /// <param name="pSamples">Pointer at the 16-bit signed samples to be encoded. 
    /// InPtr is used to pass any type of array without need of make memory copy, 
    /// then gaining in performance. Note that nSamples is not the number of bytes,
    /// but samples (is sample is a SHORT)</param>
    /// <param name="pOutput">Buffer where to write the encoded data. 
    /// This buffer should be at least of the minimum size returned by beInitStream().</param>
    /// <param name="pdwOutput">Returns the number of bytes of encoded data written. 
    /// The amount of data written might vary from chunk to chunk</param>
    /// <returns>On success: BE_ERR_SUCCESSFUL</returns>
    [DllImport("Lame_enc.dll")]
    protected static extern uint beEncodeChunk(uint hbeStream, uint nSamples, IntPtr pSamples, [In, Out] byte[] pOutput, ref uint pdwOutput);
    /// <summary>
    /// Encodes a chunk of samples. Samples are contained in a byte array
    /// </summary>
    /// <param name="hbeStream">Handle of the stream.</param>
    /// <param name="buffer">Bytes to encode</param>
    /// <param name="index">Position of the first byte to encode</param>
    /// <param name="nBytes">Number of bytes to encode (not samples, samples are two byte lenght)</param>
    /// <param name="pOutput">Buffer where to write the encoded data.
    /// This buffer should be at least of the minimum size returned by beInitStream().</param>
    /// <param name="pdwOutput">Returns the number of bytes of encoded data written. 
    /// The amount of data written might vary from chunk to chunk</param>
    /// <returns>On success: BE_ERR_SUCCESSFUL</returns>
    public static uint EncodeChunk(uint hbeStream, byte[] buffer, int index, uint nBytes, byte[] pOutput, ref uint pdwOutput)
    {
      uint res;
      GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
      try
      {
        IntPtr ptr = (IntPtr)(handle.AddrOfPinnedObject().ToInt32()+index);
        res = beEncodeChunk(hbeStream, nBytes/2/*Samples*/, ptr, pOutput, ref pdwOutput);
      }
      finally
      {
        handle.Free();
      }
      return res;
    }
    /// <summary>
    /// Encodes a chunk of samples. Samples are contained in a byte array
    /// </summary>
    /// <param name="hbeStream">Handle of the stream.</param>
    /// <param name="buffer">Bytes to encode</param>
    /// <param name="pOutput">Buffer where to write the encoded data.
    /// This buffer should be at least of the minimum size returned by beInitStream().</param>
    /// <param name="pdwOutput">Returns the number of bytes of encoded data written. 
    /// The amount of data written might vary from chunk to chunk</param>
    /// <returns>On success: BE_ERR_SUCCESSFUL</returns>
    public static uint EncodeChunk(uint hbeStream, byte[] buffer, byte[] pOutput, ref uint pdwOutput)
    {
      return EncodeChunk(hbeStream, buffer, 0, (uint)buffer.Length, pOutput, ref pdwOutput);
    }
    /// <summary>
    /// This function should be called after encoding the last chunk in order to flush 
    /// the encoder. It writes any encoded data that still might be left inside the 
    /// encoder to the output buffer. This function should NOT be called unless 
    /// you have encoded all of the chunks in your stream.
    /// </summary>
    /// <param name="hbeStream">Handle of the stream.</param>
    /// <param name="pOutput">Where to write the encoded data. This buffer should be 
    /// at least of the minimum size returned by beInitStream().</param>
    /// <param name="pdwOutput">Returns number of bytes of encoded data written.</param>
    /// <returns>On success: BE_ERR_SUCCESSFUL</returns>
    [DllImport("Lame_enc.dll")]
    public static extern uint beDeinitStream(uint hbeStream, [In, Out] byte[] pOutput, ref uint pdwOutput);
    /// <summary>
    /// Last function to be called when finished encoding a stream. 
    /// Should unlike beDeinitStream() also be called if the encoding is canceled.
    /// </summary>
    /// <param name="hbeStream">Handle of the stream.</param>
    /// <returns>On success: BE_ERR_SUCCESSFUL</returns>
    [DllImport("Lame_enc.dll")]
    public static extern uint beCloseStream(uint hbeStream);
    /// <summary>
    /// Returns information like version numbers (both of the DLL and encoding engine), 
    /// release date and URL for lame_enc's homepage. 
    /// All this information should be made available to the user of your product 
    /// through a dialog box or something similar.
    /// </summary>
    /// <param name="pbeVersion"Where version number, release date and URL for homepage 
    /// is returned.</param>
    [DllImport("Lame_enc.dll")]
    public static extern void beVersion([Out] BE_VERSION pbeVersion);
    [DllImport("Lame_enc.dll", CharSet=CharSet.Ansi)]
    public static extern void beWriteVBRHeader(string pszMP3FileName);
    [DllImport("Lame_enc.dll")]
    public static extern uint	beEncodeChunkFloatS16NI(uint hbeStream, uint nSamples, [In]float[] buffer_l, [In]float[] buffer_r, [In, Out]byte[] pOutput, ref uint pdwOutput);
    [DllImport("Lame_enc.dll")]
    public static extern uint	beFlushNoGap(uint hbeStream, [In, Out]byte[] pOutput, ref uint pdwOutput);
    [DllImport("Lame_enc.dll", CharSet=CharSet.Ansi)]
    public static extern uint	beWriteInfoTag(uint hbeStream, string lpszFileName);
	}
}
