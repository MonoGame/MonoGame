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
using System.Text;
using System.Runtime.InteropServices;

namespace Yeti.WMFSdk
{
	/// <summary>
	/// Helper to encapsulate IWMStreamConfig.
	/// </summary>
	public class WMStreamConfig
	{
    private IWMStreamConfig m_StreamConfig;

		/// <summary>
		/// WMStreamConfig constructor
		/// </summary>
		/// <param name="config">IWMStreamConfig to wrap</param>
    public WMStreamConfig(IWMStreamConfig config)
		{
			m_StreamConfig = config;
		}

    /// <summary>
    /// Wrapped IWMStreamConfig object
    /// </summary>
    public IWMStreamConfig StreamConfig
    {
      get { return m_StreamConfig; }
    }
    
    /// <summary>
    /// Read/Write. Bitrate of the stream. Wraps IWMStreamConfig.GetBitrate and WMStreamConfig.SetBitrate
    /// </summary>
    public uint Bitrate
    {
      get
      {
        uint res;
        m_StreamConfig.GetBitrate(out res);
        return res;
      }
      set
      {
        m_StreamConfig.SetBitrate(value);
      }
    }

    /// <summary>
    /// Get/Set the buffer window of the stream. Wraps IWMStreamConfig.GetBufferWindow and IWMStreamConfig.SetBufferWindow
    /// </summary>
    public uint BufferWindow
    {
      get
      {
        uint res;
        m_StreamConfig.GetBufferWindow(out res);
        return res;
      }
      set
      {
        m_StreamConfig.SetBufferWindow(value);
      }
    }

    /// <summary>
    /// Get/Set commention name. Wraps IWMStreamConfig.GetConnectionName and IWMStreamConfig.SetConnectionName
    /// </summary>
    public string ConnectionName
    {
      get
      {
        StringBuilder name;
        ushort namelen = 0;
        m_StreamConfig.GetConnectionName(null, ref namelen);
        name = new StringBuilder(namelen);
        m_StreamConfig.GetConnectionName(name, ref namelen);
        return name.ToString();
      }
      set
      {
        m_StreamConfig.SetConnectionName(value);
      }
    }

    /// <summary>
    /// Get/Set stream name. Wraps IWMStreamConfig.GetStreamName and IWMStreamConfig.SetStreamName
    /// </summary>
    public string StreamName
    {
      get
      {
        StringBuilder name;
        ushort namelen = 0;
        m_StreamConfig.GetStreamName(null, ref namelen);
        name = new StringBuilder(namelen);
        m_StreamConfig.GetStreamName(name, ref namelen);
        return name.ToString();
      }
      set
      {
        m_StreamConfig.SetStreamName(value);
      }
    }

    /// <summary>
    /// Get/Set stream number. Wraps IWMStreamConfig.GetStreamNumber and IWMStreamConfig.SetStreamNumber
    /// </summary>
    public ushort StreamNumber
    {
      get
      {
        ushort res;
        m_StreamConfig.GetStreamNumber(out res);
        return res;
      }
      set
      {
        m_StreamConfig.SetStreamNumber(value);
      }
    }

    /// <summary>
    /// Get the stream type (GUID that correspons of WM_MEDIA_TYPE.majortype
    /// Wraps IWMStreamConfig.GetStreamType 
    /// </summary>
    public Guid StreamType
    {
      get
      {
        Guid res;
        m_StreamConfig.GetStreamType(out res);
        return res;
      }
    }
	}
}
