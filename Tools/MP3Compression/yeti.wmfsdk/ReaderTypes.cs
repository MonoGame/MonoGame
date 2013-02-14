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
  [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
  public struct WM_READER_CLIENTINFO
  {
    public uint cbSize;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string wszLang;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string wszBrowserUserAgent;  
    [MarshalAs(UnmanagedType.LPWStr)]
    public string wszBrowserWebPage;    
    ulong  qwReserved;            
    public IntPtr pReserved;            
    [MarshalAs(UnmanagedType.LPWStr)]
    public string wszHostExe;           
    public ulong  qwHostVersion;         
    [MarshalAs(UnmanagedType.LPWStr)]
    public string wszPlayerUserAgent;   
  };

  [StructLayout(LayoutKind.Sequential)]
  public struct WM_READER_STATISTICS
  {
    public uint cbSize;
    public uint dwBandwidth;
    public uint cPacketsReceived;
    public uint cPacketsRecovered;
    public uint cPacketsLost;
    public uint wQuality;
  };

  [ComImport]
  [Guid("9F762FA7-A22E-428d-93C9-AC82F3AAFE5A")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMReaderAllocatorEx
  {
    void AllocateForStreamEx( [In] ushort wStreamNum,
                              [In] uint cbBuffer,
                              [Out] out INSSBuffer ppBuffer,
                              [In] uint dwFlags,
                              [In] ulong cnsSampleTime,
                              [In] ulong cnsSampleDuration,
                              [In] IntPtr pvContext);

    void AllocateForOutputEx( [In] uint dwOutputNum,
                                 [In] uint cbBuffer,
                                 [Out] out INSSBuffer ppBuffer,
                                 [In] uint dwFlags,
                                 [In] ulong cnsSampleTime,
                                 [In] ulong cnsSampleDuration,
                                 [In] IntPtr pvContext );
  }

  [ComImport]
  [Guid("9397F121-7705-4dc9-B049-98B698188414")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMSyncReader 
  {
    void Open( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilename );
    void Close();
    void SetRange([In] ulong cnsStartTime, [In] long cnsDuration );
    void SetRangeByFrame([In] ushort wStreamNum, [In] ulong qwFrameNumber, [In]long cFramesToRead );
    void GetNextSample([In] ushort wStreamNum,
                       [Out] out INSSBuffer ppSample,
                       [Out] out ulong pcnsSampleTime,
                       [Out] out ulong pcnsDuration,
                       [Out] out uint pdwFlags,
                       [Out] out uint pdwOutputNum,
                       [Out] out ushort pwStreamNum );
    void SetStreamsSelected( [In] ushort cStreamCount,
                             [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] ushort[] pwStreamNumbers,
                             [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] WMT_STREAM_SELECTION[] pSelections  );
    void GetStreamSelected( [In]ushort wStreamNum,
                            [Out] out WMT_STREAM_SELECTION  pSelection );
    void SetReadStreamSamples( [In] ushort wStreamNum,
                               [In, MarshalAs(UnmanagedType.Bool)] bool fCompressed );
    void GetReadStreamSamples( [In] ushort wStreamNum,
                               [Out, MarshalAs(UnmanagedType.Bool)] out bool pfCompressed );
    void GetOutputSetting( [In] uint dwOutputNum,
                           [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
                           [Out] out WMT_ATTR_DATATYPE pType,
                           /*[out, size_is( *pcbLength )]*/ IntPtr pValue,
                           [In, Out] ref uint pcbLength );
    void SetOutputSetting( [In] uint dwOutputNum,
                           [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
                           [In] WMT_ATTR_DATATYPE  Type,
                           [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] byte[] pValue,
                           [In] uint cbLength );
    void GetOutputCount( [Out] out uint pcOutputs );
    void GetOutputProps( [In] uint dwOutputNum, [Out, MarshalAs(UnmanagedType.Interface)] out IWMOutputMediaProps ppOutput );
    void SetOutputProps( [In] uint dwOutputNum, [In, MarshalAs(UnmanagedType.Interface)] IWMOutputMediaProps pOutput );
    void GetOutputFormatCount( [In] uint dwOutputNum, [Out] out uint pcFormats );
    void GetOutputFormat( [In] uint dwOutputNum,
                          [In] uint dwFormatNum,
                          [Out, MarshalAs(UnmanagedType.Interface)] out IWMOutputMediaProps ppProps );
    void GetOutputNumberForStream( [In] ushort wStreamNum, [Out] out uint pdwOutputNum );
    void GetStreamNumberForOutput( [In] uint dwOutputNum, [Out] out ushort pwStreamNum );
    void GetMaxOutputSampleSize( [In] uint dwOutput, [Out] out uint pcbMax );
    void GetMaxStreamSampleSize( [In] ushort wStream, [Out] out uint pcbMax );
    void OpenStream( [In, MarshalAs(UnmanagedType.Interface)] UCOMIStream pStream );
  }

  [ComImport]
  [Guid("faed3d21-1b6b-4af7-8cb6-3e189bbc187b")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMSyncReader2 : IWMSyncReader
  {
    //IWMSyncReader
    new void Open( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilename );
    new void Close();
    new void SetRange([In] ulong cnsStartTime, [In] long cnsDuration );
    new void SetRangeByFrame([In] ushort wStreamNum, [In] ulong qwFrameNumber, [In]long cFramesToRead );
    new void GetNextSample([In] ushort wStreamNum,
      [Out] out INSSBuffer ppSample,
      [Out] out ulong pcnsSampleTime,
      [Out] out ulong pcnsDuration,
      [Out] out uint pdwFlags,
      [Out] out uint pdwOutputNum,
      [Out] out ushort pwStreamNum );
    new void SetStreamsSelected( [In] ushort cStreamCount,
      [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] ushort[] pwStreamNumbers,
      [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] WMT_STREAM_SELECTION[] pSelections  );
    new void GetStreamSelected( [In]ushort wStreamNum,
      [Out] out WMT_STREAM_SELECTION  pSelection );
    new void SetReadStreamSamples( [In] ushort wStreamNum,
      [In, MarshalAs(UnmanagedType.Bool)] bool fCompressed );
    new void GetReadStreamSamples( [In] ushort wStreamNum,
      [Out, MarshalAs(UnmanagedType.Bool)] out bool pfCompressed );
    new void GetOutputSetting( [In] uint dwOutputNum,
      [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
      [Out] out WMT_ATTR_DATATYPE pType,
      /*[out, size_is( *pcbLength )]*/ IntPtr pValue,
      [In, Out] ref uint pcbLength );
    new void SetOutputSetting( [In] uint dwOutputNum,
      [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
      [In] WMT_ATTR_DATATYPE  Type,
      [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] byte[] pValue,
      [In] uint cbLength );
    new void GetOutputCount( [Out] out uint pcOutputs );
    new void GetOutputProps( [In] uint dwOutputNum, [Out, MarshalAs(UnmanagedType.Interface)] out IWMOutputMediaProps ppOutput );
    new void SetOutputProps( [In] uint dwOutputNum, [In, MarshalAs(UnmanagedType.Interface)] IWMOutputMediaProps pOutput );
    new void GetOutputFormatCount( [In] uint dwOutputNum, [Out] out uint pcFormats );
    new void GetOutputFormat( [In] uint dwOutputNum,
      [In] uint dwFormatNum,
      [Out, MarshalAs(UnmanagedType.Interface)] out IWMOutputMediaProps ppProps );
    new void GetOutputNumberForStream( [In] ushort wStreamNum, [Out] out uint pdwOutputNum );
    new void GetStreamNumberForOutput( [In] uint dwOutputNum, [Out] out ushort pwStreamNum );
    new void GetMaxOutputSampleSize( [In] uint dwOutput, [Out] out uint pcbMax );
    new void GetMaxStreamSampleSize( [In] ushort wStream, [Out] out uint pcbMax );
    new void OpenStream( [In, MarshalAs(UnmanagedType.Interface)] UCOMIStream pStream );
    //IWMSyncReader2
    void SetRangeByTimecode( [In] ushort wStreamNum,
                             [In] ref WMT_TIMECODE_EXTENSION_DATA pStart,
                             [In] ref WMT_TIMECODE_EXTENSION_DATA pEnd );

    void SetRangeByFrameEx( [In] ushort wStreamNum,
                            [In] ulong qwFrameNumber,
                            [In] long cFramesToRead,
                            [Out] out ulong pcnsStartTime );
    void SetAllocateForOutput( [In] uint dwOutputNum, [In, MarshalAs(UnmanagedType.Interface)] IWMReaderAllocatorEx pAllocator );
    void GetAllocateForOutput( [In] uint dwOutputNum, [Out, MarshalAs(UnmanagedType.Interface)] out IWMReaderAllocatorEx ppAllocator );
    void SetAllocateForStream( [In] ushort wStreamNum, [In, MarshalAs(UnmanagedType.Interface)] IWMReaderAllocatorEx pAllocator );
    void GetAllocateForStream( [In] ushort dwSreamNum, [Out, MarshalAs(UnmanagedType.Interface)] out IWMReaderAllocatorEx ppAllocator );
  }
  
  [ComImport]
  [Guid("96406BD8-2B2B-11d3-B36B-00C04F6108FF")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMReaderCallback : IWMStatusCallback
  {
    //IWMStatusCallback
    new void OnStatus( [In] WMT_STATUS Status,
      [In] IntPtr hr,
      [In] WMT_ATTR_DATATYPE dwType,
      [In] IntPtr pValue,
      [In] IntPtr pvContext );
    //IWMReaderCallback
    void OnSample( [In] uint dwOutputNum,
                   [In] ulong cnsSampleTime,
                   [In] ulong cnsSampleDuration,
                   [In] uint dwFlags,
                   [In, MarshalAs(UnmanagedType.Interface)] INSSBuffer pSample,
                   [In] IntPtr pvContext );
  }

  [ComImport]
  [Guid("96406BD6-2B2B-11d3-B36B-00C04F6108FF")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMReader
  {
    void Open( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszURL,
               [In, MarshalAs(UnmanagedType.Interface)] IWMReaderCallback pCallback,
               [In] IntPtr pvContext );
    void Close();
    void GetOutputCount( [Out] out uint pcOutputs );
    void GetOutputProps( [In] uint dwOutputNum,
                         [Out, MarshalAs(UnmanagedType.Interface)] out IWMOutputMediaProps ppOutput );
    void SetOutputProps( [In] uint dwOutputNum,
                         [In, MarshalAs(UnmanagedType.Interface)] IWMOutputMediaProps pOutput );
    void GetOutputFormatCount( [In] uint dwOutputNumber, [Out] out uint pcFormats );
    void GetOutputFormat( [In] uint dwOutputNumber,
                          [In] uint dwFormatNumber,
                          [Out, MarshalAs(UnmanagedType.Interface)] out IWMOutputMediaProps ppProps );
    void Start( [In] ulong cnsStart,
                [In] ulong cnsDuration,
                [In] float fRate,
                [In] IntPtr pvContext );
    void Stop();
    void Pause();
    void Resume();
  }

  [ComImport]
  [Guid("96406BEA-2B2B-11d3-B36B-00C04F6108FF")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMReaderAdvanced 
  {
    void SetUserProvidedClock( [In, MarshalAs(UnmanagedType.Bool)] bool fUserClock );
    void GetUserProvidedClock( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfUserClock );
    void DeliverTime( [In] ulong cnsTime );
    void SetManualStreamSelection( [In, MarshalAs(UnmanagedType.Bool)] bool fSelection );
    void GetManualStreamSelection( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfSelection );
    void SetStreamsSelected( [In] ushort cStreamCount,
                             [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] ushort[] pwStreamNumbers,
                             [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] WMT_STREAM_SELECTION[] pSelections );
    void GetStreamSelected( [In] ushort wStreamNum, [Out] out WMT_STREAM_SELECTION pSelection );
    void SetReceiveSelectionCallbacks( [In, MarshalAs(UnmanagedType.Bool)] bool fGetCallbacks );
    void GetReceiveSelectionCallbacks( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfGetCallbacks );
    void SetReceiveStreamSamples( [In] ushort wStreamNum, [In, MarshalAs(UnmanagedType.Bool)] bool fReceiveStreamSamples );
    void GetReceiveStreamSamples( [In] ushort wStreamNum,[Out, MarshalAs(UnmanagedType.Bool)] out bool pfReceiveStreamSamples );
    void SetAllocateForOutput( [In] uint dwOutputNum, [In, MarshalAs(UnmanagedType.Bool)] bool fAllocate );
    void GetAllocateForOutput( [In] uint dwOutputNum, [Out, MarshalAs(UnmanagedType.Bool)] out bool pfAllocate );
    void SetAllocateForStream( [In] ushort wStreamNum, [In, MarshalAs(UnmanagedType.Bool)] bool fAllocate );
    void GetAllocateForStream( [In] ushort dwSreamNum, [Out, MarshalAs(UnmanagedType.Bool)] out bool pfAllocate );
    void GetStatistics( [In, Out] ref WM_READER_STATISTICS pStatistics );
    void SetClientInfo( [In] ref WM_READER_CLIENTINFO pClientInfo );
    void GetMaxOutputSampleSize( [In] uint dwOutput, [Out] out uint pcbMax );
    void GetMaxStreamSampleSize( [In] ushort wStream, [Out] out uint pcbMax );
    void NotifyLateDelivery( ulong cnsLateness );
  }

  [ComImport]
  [Guid("ae14a945-b90c-4d0d-9127-80d665f7d73e")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMReaderAdvanced2 : IWMReaderAdvanced
  {
    //IWMReaderAdvanced
    new void SetUserProvidedClock( [In, MarshalAs(UnmanagedType.Bool)] bool fUserClock );
    new void GetUserProvidedClock( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfUserClock );
    new void DeliverTime( [In] ulong cnsTime );
    new void SetManualStreamSelection( [In, MarshalAs(UnmanagedType.Bool)] bool fSelection );
    new void GetManualStreamSelection( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfSelection );
    new void SetStreamsSelected( [In] ushort cStreamCount,
      [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] ushort[] pwStreamNumbers,
      [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] WMT_STREAM_SELECTION[] pSelections );
    new void GetStreamSelected( [In] ushort wStreamNum, [Out] out WMT_STREAM_SELECTION pSelection );
    new void SetReceiveSelectionCallbacks( [In, MarshalAs(UnmanagedType.Bool)] bool fGetCallbacks );
    new void GetReceiveSelectionCallbacks( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfGetCallbacks );
    new void SetReceiveStreamSamples( [In] ushort wStreamNum, [In, MarshalAs(UnmanagedType.Bool)] bool fReceiveStreamSamples );
    new void GetReceiveStreamSamples( [In] ushort wStreamNum,[Out, MarshalAs(UnmanagedType.Bool)] out bool pfReceiveStreamSamples );
    new void SetAllocateForOutput( [In] uint dwOutputNum, [In, MarshalAs(UnmanagedType.Bool)] bool fAllocate );
    new void GetAllocateForOutput( [In] uint dwOutputNum, [Out, MarshalAs(UnmanagedType.Bool)] out bool pfAllocate );
    new void SetAllocateForStream( [In] ushort wStreamNum, [In, MarshalAs(UnmanagedType.Bool)] bool fAllocate );
    new void GetAllocateForStream( [In] ushort dwSreamNum, [Out, MarshalAs(UnmanagedType.Bool)] out bool pfAllocate );
    new void GetStatistics( [In, Out] ref WM_READER_STATISTICS pStatistics );
    new void SetClientInfo( [In] ref WM_READER_CLIENTINFO pClientInfo );
    new void GetMaxOutputSampleSize( [In] uint dwOutput, [Out] out uint pcbMax );
    new void GetMaxStreamSampleSize( [In] ushort wStream, [Out] out uint pcbMax );
    new void NotifyLateDelivery( ulong cnsLateness );
    //IWMReaderAdvanced2
    void SetPlayMode( [In] WMT_PLAY_MODE Mode );
    void GetPlayMode( [Out] out WMT_PLAY_MODE pMode );
    void GetBufferProgress( [Out] out uint pdwPercent, [Out] out ulong pcnsBuffering );
    void GetDownloadProgress( [Out] out uint pdwPercent,
                              [Out] out ulong pqwBytesDownloaded,
                              [Out] out ulong pcnsDownload );
    void GetSaveAsProgress( [Out] out uint pdwPercent );
    void SaveFileAs( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilename );
    void GetProtocolName( [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszProtocol,
                          [In, Out] ref uint pcchProtocol );
    void StartAtMarker( [In] ushort wMarkerIndex,
                        [In] ulong cnsDuration,
                        [In] float fRate,
                        [In] IntPtr pvContext );
    void GetOutputSetting( [In] uint dwOutputNum,
                           [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
                           [Out] out WMT_ATTR_DATATYPE pType,
                           [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pValue,
                           [In, Out] ref ushort pcbLength );

    void SetOutputSetting( [In] uint dwOutputNum,
                           [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
                           [In] WMT_ATTR_DATATYPE Type,
                           [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] byte[] pValue,
                           [In] ushort cbLength );
    void Preroll( [In] ulong cnsStart,
                  [In] ulong cnsDuration,
                  [In] float fRate );
    void SetLogClientID( [In, MarshalAs(UnmanagedType.Bool)] bool fLogClientID );
    void GetLogClientID( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfLogClientID );
    void StopBuffering( );
    void OpenStream( [In, MarshalAs(UnmanagedType.Interface)] UCOMIStream pStream,
                     [In, MarshalAs(UnmanagedType.Interface)] IWMReaderCallback pCallback,
                     [In] IntPtr pvContext );
  }

  [ComImport]
  [Guid("5DC0674B-F04B-4a4e-9F2A-B1AFDE2C8100")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMReaderAdvanced3 : IWMReaderAdvanced2
  {
    //IWMReaderAdvanced
    new void SetUserProvidedClock( [In, MarshalAs(UnmanagedType.Bool)] bool fUserClock );
    new void GetUserProvidedClock( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfUserClock );
    new void DeliverTime( [In] ulong cnsTime );
    new void SetManualStreamSelection( [In, MarshalAs(UnmanagedType.Bool)] bool fSelection );
    new void GetManualStreamSelection( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfSelection );
    new void SetStreamsSelected( [In] ushort cStreamCount,
      [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] ushort[] pwStreamNumbers,
      [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] WMT_STREAM_SELECTION[] pSelections );
    new void GetStreamSelected( [In] ushort wStreamNum, [Out] out WMT_STREAM_SELECTION pSelection );
    new void SetReceiveSelectionCallbacks( [In, MarshalAs(UnmanagedType.Bool)] bool fGetCallbacks );
    new void GetReceiveSelectionCallbacks( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfGetCallbacks );
    new void SetReceiveStreamSamples( [In] ushort wStreamNum, [In, MarshalAs(UnmanagedType.Bool)] bool fReceiveStreamSamples );
    new void GetReceiveStreamSamples( [In] ushort wStreamNum,[Out, MarshalAs(UnmanagedType.Bool)] out bool pfReceiveStreamSamples );
    new void SetAllocateForOutput( [In] uint dwOutputNum, [In, MarshalAs(UnmanagedType.Bool)] bool fAllocate );
    new void GetAllocateForOutput( [In] uint dwOutputNum, [Out, MarshalAs(UnmanagedType.Bool)] out bool pfAllocate );
    new void SetAllocateForStream( [In] ushort wStreamNum, [In, MarshalAs(UnmanagedType.Bool)] bool fAllocate );
    new void GetAllocateForStream( [In] ushort dwSreamNum, [Out, MarshalAs(UnmanagedType.Bool)] out bool pfAllocate );
    new void GetStatistics( [In, Out] ref WM_READER_STATISTICS pStatistics );
    new void SetClientInfo( [In] ref WM_READER_CLIENTINFO pClientInfo );
    new void GetMaxOutputSampleSize( [In] uint dwOutput, [Out] out uint pcbMax );
    new void GetMaxStreamSampleSize( [In] ushort wStream, [Out] out uint pcbMax );
    new void NotifyLateDelivery( ulong cnsLateness );
    //IWMReaderAdvanced2
    new void SetPlayMode( [In] WMT_PLAY_MODE Mode );
    new void GetPlayMode( [Out] out WMT_PLAY_MODE pMode );
    new void GetBufferProgress( [Out] out uint pdwPercent, [Out] out ulong pcnsBuffering );
    new void GetDownloadProgress( [Out] out uint pdwPercent,
      [Out] out ulong pqwBytesDownloaded,
      [Out] out ulong pcnsDownload );
    new void GetSaveAsProgress( [Out] out uint pdwPercent );
    new void SaveFileAs( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilename );
    new void GetProtocolName( [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszProtocol,
      [In, Out] ref uint pcchProtocol );
    new void StartAtMarker( [In] ushort wMarkerIndex,
      [In] ulong cnsDuration,
      [In] float fRate,
      [In] IntPtr pvContext );
    new void GetOutputSetting( [In] uint dwOutputNum,
      [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
      [Out] out WMT_ATTR_DATATYPE pType,
      [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pValue,
      [In, Out] ref ushort pcbLength );
    new void SetOutputSetting( [In] uint dwOutputNum,
      [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
      [In] WMT_ATTR_DATATYPE Type,
      [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] byte[] pValue,
      [In] ushort cbLength );
    new void Preroll( [In] ulong cnsStart,
      [In] ulong cnsDuration,
      [In] float fRate );
    new void SetLogClientID( [In, MarshalAs(UnmanagedType.Bool)] bool fLogClientID );
    new void GetLogClientID( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfLogClientID );
    new void StopBuffering( );
    new void OpenStream( [In, MarshalAs(UnmanagedType.Interface)] UCOMIStream pStream,
      [In, MarshalAs(UnmanagedType.Interface)] IWMReaderCallback pCallback,
      [In] IntPtr pvContext );
    //IWMReaderAdvanced3
    void StopNetStreaming( );
    void StartAtPosition(  [In] ushort wStreamNum,
                           [In] IntPtr pvOffsetStart,
                           [In] IntPtr pvDuration,
                           [In] WMT_OFFSET_FORMAT dwOffsetFormat,
                           [In] float fRate,
                           [In] IntPtr pvContext );
  }

  [ComImport]
  [Guid("945A76A2-12AE-4d48-BD3C-CD1D90399B85")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMReaderAdvanced4 : IWMReaderAdvanced3
  {
    //IWMReaderAdvanced
    new void SetUserProvidedClock( [In, MarshalAs(UnmanagedType.Bool)] bool fUserClock );
    new void GetUserProvidedClock( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfUserClock );
    new void DeliverTime( [In] ulong cnsTime );
    new void SetManualStreamSelection( [In, MarshalAs(UnmanagedType.Bool)] bool fSelection );
    new void GetManualStreamSelection( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfSelection );
    new void SetStreamsSelected( [In] ushort cStreamCount,
      [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] ushort[] pwStreamNumbers,
      [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] WMT_STREAM_SELECTION[] pSelections );
    new void GetStreamSelected( [In] ushort wStreamNum, [Out] out WMT_STREAM_SELECTION pSelection );
    new void SetReceiveSelectionCallbacks( [In, MarshalAs(UnmanagedType.Bool)] bool fGetCallbacks );
    new void GetReceiveSelectionCallbacks( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfGetCallbacks );
    new void SetReceiveStreamSamples( [In] ushort wStreamNum, [In, MarshalAs(UnmanagedType.Bool)] bool fReceiveStreamSamples );
    new void GetReceiveStreamSamples( [In] ushort wStreamNum,[Out, MarshalAs(UnmanagedType.Bool)] out bool pfReceiveStreamSamples );
    new void SetAllocateForOutput( [In] uint dwOutputNum, [In, MarshalAs(UnmanagedType.Bool)] bool fAllocate );
    new void GetAllocateForOutput( [In] uint dwOutputNum, [Out, MarshalAs(UnmanagedType.Bool)] out bool pfAllocate );
    new void SetAllocateForStream( [In] ushort wStreamNum, [In, MarshalAs(UnmanagedType.Bool)] bool fAllocate );
    new void GetAllocateForStream( [In] ushort dwSreamNum, [Out, MarshalAs(UnmanagedType.Bool)] out bool pfAllocate );
    new void GetStatistics( [In, Out] ref WM_READER_STATISTICS pStatistics );
    new void SetClientInfo( [In] ref WM_READER_CLIENTINFO pClientInfo );
    new void GetMaxOutputSampleSize( [In] uint dwOutput, [Out] out uint pcbMax );
    new void GetMaxStreamSampleSize( [In] ushort wStream, [Out] out uint pcbMax );
    new void NotifyLateDelivery( ulong cnsLateness );
    //IWMReaderAdvanced2
    new void SetPlayMode( [In] WMT_PLAY_MODE Mode );
    new void GetPlayMode( [Out] out WMT_PLAY_MODE pMode );
    new void GetBufferProgress( [Out] out uint pdwPercent, [Out] out ulong pcnsBuffering );
    new void GetDownloadProgress( [Out] out uint pdwPercent,
      [Out] out ulong pqwBytesDownloaded,
      [Out] out ulong pcnsDownload );
    new void GetSaveAsProgress( [Out] out uint pdwPercent );
    new void SaveFileAs( [In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilename );
    new void GetProtocolName( [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszProtocol,
      [In, Out] ref uint pcchProtocol );
    new void StartAtMarker( [In] ushort wMarkerIndex,
      [In] ulong cnsDuration,
      [In] float fRate,
      [In] IntPtr pvContext );
    new void GetOutputSetting( [In] uint dwOutputNum,
      [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
      [Out] out WMT_ATTR_DATATYPE pType,
      [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pValue,
      [In, Out] ref ushort pcbLength );
    new void SetOutputSetting( [In] uint dwOutputNum,
      [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
      [In] WMT_ATTR_DATATYPE Type,
      [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)] byte[] pValue,
      [In] ushort cbLength );
    new void Preroll( [In] ulong cnsStart,
      [In] ulong cnsDuration,
      [In] float fRate );
    new void SetLogClientID( [In, MarshalAs(UnmanagedType.Bool)] bool fLogClientID );
    new void GetLogClientID( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfLogClientID );
    new void StopBuffering( );
    new void OpenStream( [In, MarshalAs(UnmanagedType.Interface)] UCOMIStream pStream,
      [In, MarshalAs(UnmanagedType.Interface)] IWMReaderCallback pCallback,
      [In] IntPtr pvContext );
    //IWMReaderAdvanced3
    new void StopNetStreaming( );
    new void StartAtPosition(  [In] ushort wStreamNum,
      [In] IntPtr pvOffsetStart,
      [In] IntPtr pvDuration,
      [In] WMT_OFFSET_FORMAT dwOffsetFormat,
      [In] float fRate,
      [In] IntPtr pvContext );
    //IWMReaderAdvanced4
    void GetLanguageCount( [In] uint dwOutputNum,
                           [Out] out ushort pwLanguageCount );
    void GetLanguage( [In] uint dwOutputNum,
                      [In] ushort wLanguage,
                      [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszLanguageString,
                      [In, Out] ref ushort pcchLanguageStringLength );
    void GetMaxSpeedFactor( [Out] out double pdblFactor );
    void IsUsingFastCache( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfUsingFastCache );
    void AddLogParam( [In, MarshalAs(UnmanagedType.LPWStr)] string wszNameSpace,
                      [In, MarshalAs(UnmanagedType.LPWStr)] string wszName,
                      [In, MarshalAs(UnmanagedType.LPWStr)] string wszValue );
    void SendLogParams( );
    void CanSaveFileAs( [Out, MarshalAs(UnmanagedType.Bool)] out bool pfCanSave );
    void CancelSaveFileAs( );
    void GetURL( [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszURL,
                 [In, Out] ref uint pcchURL );
  }

}