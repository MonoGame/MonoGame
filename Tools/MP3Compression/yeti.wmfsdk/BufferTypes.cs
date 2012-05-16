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
using System.Runtime.InteropServices;

namespace Yeti.WMFSdk
{
  
  [ComImport]
  [Guid("E1CD3524-03D7-11d2-9EED-006097D2D7CF")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface INSSBuffer
  {
    
    void GetLength( [Out] out uint pdwLength );
    
    void SetLength( [In] uint dwLength );
    
    void GetMaxLength( [Out] out uint pdwLength );
    
    void GetBuffer( [Out] out IntPtr ppdwBuffer );
    
    void GetBufferAndLength( [Out] out IntPtr ppdwBuffer, [Out] out uint pdwLength );
  }

  
  [ComImport]
  [Guid("4F528693-1035-43fe-B428-757561AD3A68")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface INSSBuffer2 : INSSBuffer
  {
    //INSSBuffer
    new void GetLength( [Out] out uint pdwLength );
    new void SetLength( [In] uint dwLength );
    new void GetMaxLength( [Out] out uint pdwLength );
    new void GetBuffer( [Out] out IntPtr ppdwBuffer );
    new void GetBufferAndLength( [Out] out IntPtr ppdwBuffer, [Out] out uint pdwLength );
    //INSSBuffer2
    void GetSampleProperties( [In] uint cbProperties, [Out] out byte pbProperties );
    
    void SetSampleProperties( [In] uint cbProperties, [In] ref byte pbProperties );
  };

  
  [ComImport]
  [Guid("C87CEAAF-75BE-4bc4-84EB-AC2798507672")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface INSSBuffer3 : INSSBuffer2
  {
    //INSSBuffer
    new void GetLength( [Out] out uint pdwLength );
    new void SetLength( [In] uint dwLength );
    new void GetMaxLength( [Out] out uint pdwLength );
    new void GetBuffer( [Out] out IntPtr ppdwBuffer );
    new void GetBufferAndLength( [Out] out IntPtr ppdwBuffer, [Out] out uint pdwLength );
    //INSSBuffer2
    new void GetSampleProperties( [In] uint cbProperties, [Out] out byte pbProperties );
    new void SetSampleProperties( [In] uint cbProperties, [In] ref byte pbProperties );
    //INSSBuffer3
    void SetProperty([In] Guid guidBufferProperty, 
                     [In] IntPtr pvBufferProperty,
                     [In] uint dwBufferPropertySize );
    
    void GetProperty([In] Guid guidBufferProperty,
                     /*out]*/ IntPtr pvBufferProperty,
                     [In,Out] ref uint pdwBufferPropertySize );
  }

  
  [ComImport]
  [Guid("B6B8FD5A-32E2-49d4-A910-C26CC85465ED")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface INSSBuffer4 : INSSBuffer3
  {
    //INSSBuffer
    new void GetLength( [Out] out uint pdwLength );
    new void SetLength( [In] uint dwLength );
    new void GetMaxLength( [Out] out uint pdwLength );
    new void GetBuffer( [Out] out IntPtr ppdwBuffer );
    new void GetBufferAndLength( [Out] out IntPtr ppdwBuffer, [Out] out uint pdwLength );
    //INSSBuffer2
    new void GetSampleProperties( [In] uint cbProperties, [Out] out byte pbProperties );
    new void SetSampleProperties( [In] uint cbProperties, [In] ref byte pbProperties );
    //INSSBuffer3
    new void SetProperty([In] Guid guidBufferProperty, 
      [In] IntPtr pvBufferProperty,
      [In] uint dwBufferPropertySize );
    new void GetProperty([In] Guid guidBufferProperty,
      /*out]*/ IntPtr pvBufferProperty,
      [In,Out] ref uint pdwBufferPropertySize );
    //INSSBuffer4
    void GetPropertyCount([Out] out uint pcBufferProperties );
    
    void GetPropertyByIndex([In] uint dwBufferPropertyIndex,
                            [Out] out Guid pguidBufferProperty,
                            /*[out]*/   IntPtr pvBufferProperty,
                            [In,Out] ref uint pdwBufferPropertySize );
  }

  
  [ComImport]
  [Guid("61103CA4-2033-11d2-9EF1-006097D2D7CF")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IWMSBufferAllocator 
  {
    
    void AllocateBuffer( [In] uint dwMaxBufferSize,
                         [Out, MarshalAs(UnmanagedType.Interface)] out INSSBuffer ppBuffer);
    
    void AllocatePageSizeBuffer([In] uint dwMaxBufferSize,
                                [Out, MarshalAs(UnmanagedType.Interface)] out INSSBuffer ppBuffer);
  };

}