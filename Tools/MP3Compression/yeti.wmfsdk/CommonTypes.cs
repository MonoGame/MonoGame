//Widows Media Format Interfaces
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
using System.Text;
using System.Runtime.InteropServices;

namespace Yeti.WMFSdk
{

  public enum WMT_STREAM_SELECTION
  {
    WMT_OFF               = 0,
    WMT_CLEANPOINT_ONLY   = 1,
    WMT_ON                = 2,
  };

  
  public enum WMT_ATTR_DATATYPE
  {
    WMT_TYPE_DWORD      = 0,
    
    WMT_TYPE_STRING     = 1,
    
    WMT_TYPE_BINARY     = 2,
    
    WMT_TYPE_BOOL       = 3,
    
    WMT_TYPE_QWORD      = 4,
    
    WMT_TYPE_WORD       = 5,
    
    WMT_TYPE_GUID       = 6,
  };

  
  public enum WMT_VERSION
  {
    
    WMT_VER_4_0 = 0x00040000,
    
    WMT_VER_7_0 = 0x00070000,
    
    WMT_VER_8_0 = 0x00080000,
    
    WMT_VER_9_0 = 0x00090000,
  };

  
  public enum WMT_STORAGE_FORMAT
  {
    
    WMT_Storage_Format_MP3,
    
    WMT_Storage_Format_V1
  }; 

  
  public enum WMT_TRANSPORT_TYPE
  {
    
    WMT_Transport_Type_Unreliable,
    
    WMT_Transport_Type_Reliable
  };

  
  public enum WMT_STATUS
  {
    
    WMT_ERROR                       =  0,
    
    WMT_OPENED                      =  1,
    
    WMT_BUFFERING_START             =  2,
    
    WMT_BUFFERING_STOP              =  3,
    
    WMT_EOF                         =  4,
    
    WMT_END_OF_FILE                 =  4,
    
    WMT_END_OF_SEGMENT              =  5,
    
    WMT_END_OF_STREAMING            =  6,
    
    WMT_LOCATING                    =  7,
    
    WMT_CONNECTING                  =  8,
    
    WMT_NO_RIGHTS                   =  9,
    
    WMT_MISSING_CODEC               = 10,
    
    WMT_STARTED                     = 11,
    
    WMT_STOPPED                     = 12,
    
    WMT_CLOSED                      = 13,
    
    WMT_STRIDING                    = 14,
    
    WMT_TIMER                       = 15,
    
    WMT_INDEX_PROGRESS              = 16,
    
    WMT_SAVEAS_START                = 17,
    
    WMT_SAVEAS_STOP                 = 18,
    
    WMT_NEW_SOURCEFLAGS             = 19,
    
    WMT_NEW_METADATA                = 20,
    
    WMT_BACKUPRESTORE_BEGIN         = 21,
    
    WMT_SOURCE_SWITCH               = 22,
    
    WMT_ACQUIRE_LICENSE             = 23,
    
    WMT_INDIVIDUALIZE               = 24,
    
    WMT_NEEDS_INDIVIDUALIZATION     = 25,
    
    WMT_NO_RIGHTS_EX                = 26,
    
    WMT_BACKUPRESTORE_END           = 27,
    
    WMT_BACKUPRESTORE_CONNECTING    = 28,
    
    WMT_BACKUPRESTORE_DISCONNECTING = 29,
    
    WMT_ERROR_WITHURL               = 30,
    
    WMT_RESTRICTED_LICENSE          = 31,
    
    WMT_CLIENT_CONNECT              = 32,
    
    WMT_CLIENT_DISCONNECT           = 33,
    
    WMT_NATIVE_OUTPUT_PROPS_CHANGED = 34,
    
    WMT_RECONNECT_START             = 35,
    
    WMT_RECONNECT_END               = 36,
    
    WMT_CLIENT_CONNECT_EX           = 37,
    
    WMT_CLIENT_DISCONNECT_EX        = 38,
    
    WMT_SET_FEC_SPAN                = 39,
    
    WMT_PREROLL_READY               = 40,
    
    WMT_PREROLL_COMPLETE            = 41,
    
    WMT_CLIENT_PROPERTIES           = 42,
    
    WMT_LICENSEURL_SIGNATURE_STATE  = 43
  };

  
  public enum WMT_PLAY_MODE
  {
    
    WMT_PLAY_MODE_AUTOSELECT    = 0,
    
    WMT_PLAY_MODE_LOCAL         = 1,
    
    WMT_PLAY_MODE_DOWNLOAD      = 2,
    
    WMT_PLAY_MODE_STREAMING     = 3,
  };

  
  public enum WMT_OFFSET_FORMAT
  {
    
    WMT_OFFSET_FORMAT_100NS,
    
    WMT_OFFSET_FORMAT_FRAME_NUMBERS,
    
    WMT_OFFSET_FORMAT_PLAYLIST_OFFSET,
    
    WMT_OFFSET_FORMAT_TIMECODE
  };

  
  public enum WMT_CODEC_INFO_TYPE : uint
  {
    
    WMT_CODECINFO_AUDIO         = 0,            
    
    WMT_CODECINFO_VIDEO         = 1,            
    
    WMT_CODECINFO_UNKNOWN       = 0xFFFFFFFF,   
  };

  
  [Flags]
  public enum WMT_RIGHTS : uint
  {
    /// <summary>
    /// This rigth is not defined in the WMF SDK, I added it to
    /// play files with no DRM
    /// </summary>
    WMT_RIGHT_NO_DRM                    = 0x00000000,
    
    WMT_RIGHT_PLAYBACK                  = 0x00000001,
    
    WMT_RIGHT_COPY_TO_NON_SDMI_DEVICE   = 0x00000002,
    
    WMT_RIGHT_COPY_TO_CD                = 0x00000008,
    
    WMT_RIGHT_COPY_TO_SDMI_DEVICE       = 0x00000010,
    
    WMT_RIGHT_ONE_TIME                  = 0x00000020,
    
    WMT_RIGHT_SAVE_STREAM_PROTECTED     = 0x00000040,
    
    WMT_RIGHT_SDMI_TRIGGER              = 0x00010000,
    
    WMT_RIGHT_SDMI_NOMORECOPIES         = 0x00020000
  }

  
  public enum WMT_INDEXER_TYPE
  {
    
    WMT_IT_PRESENTATION_TIME,
    
    WMT_IT_FRAME_NUMBERS,
    
    WMT_IT_TIMECODE
  };

  
  public enum WMT_INDEX_TYPE
  {
    
    WMT_IT_NEAREST_DATA_UNIT = 1,
    
    WMT_IT_NEAREST_OBJECT,
    
    WMT_IT_NEAREST_CLEAN_POINT
  };

  
  public enum WMT_NET_PROTOCOL
  {
    
    WMT_PROTOCOL_HTTP    = 0,
  };

  
  [StructLayout(LayoutKind.Sequential)]
  public struct WMT_TIMECODE_EXTENSION_DATA
  {
    
    public ushort wRange;
    
    public uint dwTimecode;
    
    public uint dwUserbits;
    
    public uint dwAmFlags;
  };
  
  
  [StructLayout(LayoutKind.Sequential)]
  public struct WM_STREAM_PRIORITY_RECORD
  {
    
    public ushort wStreamNumber;
    
    [MarshalAs(UnmanagedType.Bool)]
    public bool fMandatory;
  }; 

  
  [StructLayout(LayoutKind.Sequential)]
  public struct WM_MEDIA_TYPE
  {
    
    public Guid majortype;
    
    public Guid subtype;
    
    [MarshalAs(UnmanagedType.Bool)]
    public bool bFixedSizeSamples;
    
    [MarshalAs(UnmanagedType.Bool)]
    public bool bTemporalCompression;
    
    public uint lSampleSize;
    
    public Guid formattype;
    
    public IntPtr pUnk;
    
    public uint cbFormat;
    
    public IntPtr pbFormat;
  };
  
  
  [ComImport]
  [Guid("96406BCE-2B2B-11d3-B36B-00C04F6108FF")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMMediaProps
  {
    
    void GetType( [Out] out Guid pguidType );
    
    void GetMediaType( /*[out] WM_MEDIA_TYPE* */ IntPtr pType,
                       [In, Out] ref uint pcbType );
    
    void SetMediaType( [In] ref WM_MEDIA_TYPE pType );
  }

  
  [ComImport]
  [Guid("96406BCF-2B2B-11d3-B36B-00C04F6108FF")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMVideoMediaProps : IWMMediaProps
  {
    //IWMMediaProps
    new void GetType( [Out] out Guid pguidType );
    new void GetMediaType( /*[out] WM_MEDIA_TYPE* */ IntPtr pType,
      [In, Out] ref uint pcbType );
    new void SetMediaType( [In] ref WM_MEDIA_TYPE pType );
    //IWMVideoMediaProps
    void GetMaxKeyFrameSpacing( [Out] out long pllTime );
    
    void SetMaxKeyFrameSpacing( [In] long llTime );
    
    void GetQuality( [Out] out uint pdwQuality );
    
    void SetQuality( [In] uint dwQuality );
  }

  
  [ComImport]
  [Guid("96406BD5-2B2B-11d3-B36B-00C04F6108FF")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMInputMediaProps : IWMMediaProps
  {
    //IWMMediaProps
    new void GetType( [Out] out Guid pguidType );
    new void GetMediaType( /*[out] WM_MEDIA_TYPE* */ IntPtr pType,
      [In, Out] ref uint pcbType );
    new void SetMediaType( [In] ref WM_MEDIA_TYPE pType );
    //IWMInputMediaProps  
    void GetConnectionName( [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName, 
                            [In, Out] ref ushort pcchName );
    
    void GetGroupName( [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName,
                       [In, Out] ref ushort pcchName );
  }

  
  [ComImport]
  [Guid("96406BD7-2B2B-11d3-B36B-00C04F6108FF")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMOutputMediaProps : IWMMediaProps
  {
    //IWMMediaProps
    new void GetType( [Out] out Guid pguidType );
    new void GetMediaType( /*[out] WM_MEDIA_TYPE* */ IntPtr pType,
      [In, Out] ref uint pcbType );
    new void SetMediaType( [In] ref WM_MEDIA_TYPE pType );
    //IWMOutputMediaProps    
    void GetStreamGroupName( [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName, 
                             [In, Out] ref ushort pcchName );
    
    void GetConnectionName( [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName,
                            [In, Out] ref ushort pcchName );
  }

  
  [ComImport]
  [Guid("96406BDD-2B2B-11d3-B36B-00C04F6108FF")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMStreamList
  {
    
    void GetStreams( [Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwStreamNumArray,
                     [In, Out] ref ushort pcStreams );
    
    void AddStream( [In] ushort wStreamNum );
    
    void RemoveStream( [In] ushort wStreamNum );
  };

  
  [ComImport]
  [Guid("96406BDE-2B2B-11d3-B36B-00C04F6108FF")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMMutualExclusion : IWMStreamList
  {
    //IWMStreamList
    new void GetStreams( [Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwStreamNumArray,
      [In, Out] ref ushort pcStreams );
    new void AddStream( [In] ushort wStreamNum );
    new void RemoveStream( [In] ushort wStreamNum );
    //IWMMutualExclusion
    void GetType( [Out] out Guid pguidType );
    
    void SetType( [In] ref Guid guidType );
  };

  
  [ComImport]
  [Guid("0302B57D-89D1-4ba2-85C9-166F2C53EB91")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMMutualExclusion2 : IWMMutualExclusion
  {
    //IWMStreamList
    new void GetStreams( [Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwStreamNumArray,
      [In, Out] ref ushort pcStreams );
    new void AddStream( [In] ushort wStreamNum );
    new void RemoveStream( [In] ushort wStreamNum );
    //IWMMutualExclusion
    new void GetType( [Out] out Guid pguidType );
    new void SetType( [In] ref Guid guidType );
    //IWMMutualExclusion2
    void GetName( [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName,
                  [In, Out] ref ushort pcchName );
    
    void SetName( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszName );
    
    void GetRecordCount( [Out] out ushort pwRecordCount );
    
    void AddRecord();
    
    void RemoveRecord( [In] ushort wRecordNumber );
    
    void GetRecordName( [In] ushort wRecordNumber,
                        [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszRecordName,
                        [In, Out] ref ushort pcchRecordName );
    
    void SetRecordName( [In] ushort wRecordNumber,
                        [In, MarshalAs(UnmanagedType.LPWStr)] string pwszRecordName );
    
    void GetStreamsForRecord( [In] ushort wRecordNumber,
                              [Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwStreamNumArray,
                              [In, Out] ref ushort pcStreams );
    
    void AddStreamForRecord( [In] ushort wRecordNumber, [In] ushort wStreamNumber );
    
    void RemoveStreamForRecord( [In] ushort wRecordNumber, [In] ushort wStreamNumber );
  }

  
  [ComImport]
  [Guid("AD694AF1-F8D9-42F8-BC47-70311B0C4F9E")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMBandwidthSharing : IWMStreamList
  {
    //IWMStreamList
    new void GetStreams( [Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwStreamNumArray,
      [In, Out] ref ushort pcStreams );
    new void AddStream( [In] ushort wStreamNum );
    new void RemoveStream( [In] ushort wStreamNum );
    //IWMBandwidthSharing    
    void GetType( [Out] out Guid pguidType );
    
    void SetType( [In] ref Guid guidType );
    
    void GetBandwidth( [Out] out uint pdwBitrate, [Out] out uint pmsBufferWindow );
    
    void SetBandwidth( [In] uint dwBitrate, [In] uint msBufferWindow );
  }

  
  [ComImport]
  [Guid("8C1C6090-F9A8-4748-8EC3-DD1108BA1E77")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMStreamPrioritization 
  {
    
    void GetPriorityRecords( [Out, MarshalAs(UnmanagedType.LPArray)] WM_STREAM_PRIORITY_RECORD[] pRecordArray,
                             [In, Out] ref ushort pcRecords );
    
    void SetPriorityRecords( [In, MarshalAs(UnmanagedType.LPArray)] WM_STREAM_PRIORITY_RECORD[] pRecordArray,
                             [In] ushort cRecords );
  }

  
  [ComImport]
  [Guid("6d7cdc70-9888-11d3-8edc-00c04f6109cf")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMStatusCallback
  {
    
    void OnStatus( [In] WMT_STATUS Status,
                   [In] IntPtr hr,
                   [In] WMT_ATTR_DATATYPE dwType,
                   [In] IntPtr pValue,
                   [In] IntPtr pvContext );
  }
  
  
  [ComImport]
  [Guid("96406BDA-2B2B-11d3-B36B-00C04F6108FF")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMHeaderInfo 
  {
    
    void GetAttributeCount( [In] ushort wStreamNum, [Out] out ushort pcAttributes );
    
    void GetAttributeByIndex( [In] ushort wIndex,
                              [In, Out] ref ushort pwStreamNum,
                              [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName,
                              [In, Out] ref ushort pcchNameLen,
                              [Out] out WMT_ATTR_DATATYPE pType,
                              IntPtr pValue,
                              [In, Out] ref ushort pcbLength );
    
    void GetAttributeByName( [In, Out] ref ushort pwStreamNum,
                             [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
                             [Out] out WMT_ATTR_DATATYPE pType,
                             IntPtr pValue,
                             [In, Out] ref ushort pcbLength );
    
    void SetAttribute( [In] ushort wStreamNum,
                       [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
                       [In] WMT_ATTR_DATATYPE Type,
                       IntPtr pValue,
                       [In] ushort cbLength );
    
    void GetMarkerCount( [Out] out ushort pcMarkers );
    
    void GetMarker( [In] ushort wIndex,
                    [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszMarkerName,
                    [In, Out] ref ushort pcchMarkerNameLen,
                    [Out] out ulong pcnsMarkerTime );
    
    void AddMarker( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszMarkerName,
                    [In] ulong cnsMarkerTime );
    
    void RemoveMarker( [In] ushort wIndex );
    
    void GetScriptCount( [Out] out ushort pcScripts );
    
    void GetScript( [In] ushort wIndex,
                    [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszType,
                    [In, Out] ref ushort pcchTypeLen,
                    [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszCommand,
                    [In, Out] ref ushort pcchCommandLen,
                    [Out] out ulong pcnsScriptTime );
    
    void AddScript( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszType,
                    [In, MarshalAs(UnmanagedType.LPWStr)] string pwszCommand,
                    [In] ulong cnsScriptTime );
    
    void RemoveScript( [In] ushort wIndex );
  }

  
  [ComImport]
  [Guid("15CF9781-454E-482e-B393-85FAE487A810")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMHeaderInfo2 : IWMHeaderInfo
  {
    //IWMHeaderInfo
    new void GetAttributeCount( [In] ushort wStreamNum, [Out] out ushort pcAttributes );
    new void GetAttributeByIndex( [In] ushort wIndex,
      [In, Out] ref ushort pwStreamNum,
      [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName,
      [In, Out] ref ushort pcchNameLen,
      [Out] out WMT_ATTR_DATATYPE pType,
      IntPtr pValue,
      [In, Out] ref ushort pcbLength );
    new void GetAttributeByName( [In, Out] ref ushort pwStreamNum,
      [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
      [Out] out WMT_ATTR_DATATYPE pType,
      IntPtr pValue,
      [In, Out] ref ushort pcbLength );
    new void SetAttribute( [In] ushort wStreamNum,
      [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
      [In] WMT_ATTR_DATATYPE Type,
      IntPtr pValue,
      [In] ushort cbLength );
    new void GetMarkerCount( [Out] out ushort pcMarkers );
    new void GetMarker( [In] ushort wIndex,
      [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszMarkerName,
      [In, Out] ref ushort pcchMarkerNameLen,
      [Out] out ulong pcnsMarkerTime );
    new void AddMarker( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszMarkerName,
      [In] ulong cnsMarkerTime );
    new void RemoveMarker( [In] ushort wIndex );
    new void GetScriptCount( [Out] out ushort pcScripts );
    new void GetScript( [In] ushort wIndex,
      [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszType,
      [In, Out] ref ushort pcchTypeLen,
      [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszCommand,
      [In, Out] ref ushort pcchCommandLen,
      [Out] out ulong pcnsScriptTime );
    new void AddScript( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszType,
      [In, MarshalAs(UnmanagedType.LPWStr)] string pwszCommand,
      [In] ulong cnsScriptTime );
    new void RemoveScript( [In] ushort wIndex );
    //IWMHeaderInfo2
    void GetCodecInfoCount(  [Out] out uint pcCodecInfos );
    
    void GetCodecInfo( [In] uint wIndex,
                       [In, Out] ref ushort pcchName,
                       [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName,
                       [In, Out] ref ushort pcchDescription,
                       [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszDescription,
                       [Out] out WMT_CODEC_INFO_TYPE pCodecType,
                       [In, Out] ref ushort pcbCodecInfo,
                       [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbCodecInfo );
  }

  
  [ComImport]
  [Guid("15CC68E3-27CC-4ecd-B222-3F5D02D80BD5")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMHeaderInfo3 : IWMHeaderInfo2
  {
    //IWMHeaderInfo
    new void GetAttributeCount( [In] ushort wStreamNum, [Out] out ushort pcAttributes );
    new void GetAttributeByIndex( [In] ushort wIndex,
      [In, Out] ref ushort pwStreamNum,
      [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName,
      [In, Out] ref ushort pcchNameLen,
      [Out] out WMT_ATTR_DATATYPE pType,
      IntPtr pValue,
      [In, Out] ref ushort pcbLength );
    new void GetAttributeByName( [In, Out] ref ushort pwStreamNum,
      [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
      [Out] out WMT_ATTR_DATATYPE pType,
      IntPtr pValue,
      [In, Out] ref ushort pcbLength );
    new void SetAttribute( [In] ushort wStreamNum,
      [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
      [In] WMT_ATTR_DATATYPE Type,
      IntPtr pValue,
      [In] ushort cbLength );
    new void GetMarkerCount( [Out] out ushort pcMarkers );
    new void GetMarker( [In] ushort wIndex,
      [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszMarkerName,
      [In, Out] ref ushort pcchMarkerNameLen,
      [Out] out ulong pcnsMarkerTime );
    new void AddMarker( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszMarkerName,
      [In] ulong cnsMarkerTime );
    new void RemoveMarker( [In] ushort wIndex );
    new void GetScriptCount( [Out] out ushort pcScripts );
    new void GetScript( [In] ushort wIndex,
      [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszType,
      [In, Out] ref ushort pcchTypeLen,
      [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszCommand,
      [In, Out] ref ushort pcchCommandLen,
      [Out] out ulong pcnsScriptTime );
    new void AddScript( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszType,
      [In, MarshalAs(UnmanagedType.LPWStr)] string pwszCommand,
      [In] ulong cnsScriptTime );
    new void RemoveScript( [In] ushort wIndex );
    //IWMHeaderInfo2
    new void GetCodecInfoCount(  [Out] out uint pcCodecInfos );
    new void GetCodecInfo( [In] uint wIndex,
      [In, Out] ref ushort pcchName,
      [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName,
      [In, Out] ref ushort pcchDescription,
      [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszDescription,
      [Out] out WMT_CODEC_INFO_TYPE pCodecType,
      [In, Out] ref ushort pcbCodecInfo,
      [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbCodecInfo );
    //IWMHeaderInfo3
    void GetAttributeCountEx( [In] ushort wStreamNum, [Out] out ushort pcAttributes );
    
    void GetAttributeIndices( [In] ushort wStreamNum,
                              [In, MarshalAs(UnmanagedType.LPWStr)] string pwszName,
                              /* DWORD* */IntPtr pwLangIndex,
                              [Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwIndices,
                              [In, Out] ref ushort pwCount );
    
    void GetAttributeByIndexEx( [In] ushort wStreamNum,
                                [In] ushort wIndex,
                                [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName,
                                [In, Out] ref ushort pwNameLen,
                                [Out] out WMT_ATTR_DATATYPE pType,
                                [Out] out ushort pwLangIndex,
                                IntPtr pValue,
                                [In, Out] ref uint pdwDataLength );
    
    void ModifyAttribute( [In] ushort wStreamNum,
                          [In] ushort wIndex,
                          [In] WMT_ATTR_DATATYPE Type,
                          [In] ushort wLangIndex,
                          IntPtr pValue,
                          [In] uint dwLength );
    
    void AddAttribute( [In] ushort wStreamNum,
                       [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
                       [Out] out ushort pwIndex,
                       [In] WMT_ATTR_DATATYPE Type,
                       [In] ushort wLangIndex,
                       IntPtr pValue,
                       [In] uint dwLength );
    
    void DeleteAttribute( [In] ushort wStreamNum, [In] ushort wIndex );
    
    void AddCodecInfo( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszName,
                       [In, MarshalAs(UnmanagedType.LPWStr)] string pwszDescription,
                       [In] WMT_CODEC_INFO_TYPE codecType,
                       [In] ushort cbCodecInfo,
                       [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=3)] byte[] pbCodecInfo );
  }
  
  
  [ComImport]
  [Guid("96406BD9-2B2B-11d3-B36B-00C04F6108FF")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMMetadataEditor
  {
    
    void Open( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilename );
    
    void Close();
    
    void Flush();
  }

  
  [Guid("203CFFE3-2E18-4fdf-B59D-6E71530534CF")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMMetadataEditor2 : IWMMetadataEditor
  {
    //IWMMetadataEditor
    new void Open( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilename );
    new void Close();
    new void Flush();
    //IWMMetadataEditor2
    void OpenEx( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilename,
                 [In] uint dwDesiredAccess,
                 [In] uint dwShareMode );
  }

  
  [ComImport]
  [Guid("6d7cdc71-9888-11d3-8edc-00c04f6109cf")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMIndexer 
  {
    
    void StartIndexing( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
                        [In, MarshalAs(UnmanagedType.Interface)] IWMStatusCallback pCallback,
                        [In] IntPtr pvContext );
    
    void Cancel();
  }

  
  [ComImport]
  [Guid("B70F1E42-6255-4df0-A6B9-02B212D9E2BB")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMIndexer2 : IWMIndexer
  {
    //IWMIndexer
    new void StartIndexing( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
      [In, MarshalAs(UnmanagedType.Interface)] IWMStatusCallback pCallback,
      [In] IntPtr pvContext );
    new void Cancel();
    //IWMIndexer2
    void Configure( [In] ushort wStreamNum,
                       [In] WMT_INDEXER_TYPE nIndexerType,
                       [In] IntPtr pvInterval,
                       [In] IntPtr pvIndexType );
  }

}