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
  /// Represent a marker in ASF streams
  /// </summary>
  public struct Marker
  {
    /// <summary>
    /// Marker name
    /// </summary>
    public string Name;
    /// <summary>
    /// Marker time in 100-nanosecond units
    /// </summary>
    public ulong Time;
    /// <summary>
    /// Marker constructor
    /// </summary>
    /// <param name="name">Name of the marker</param>
    /// <param name="time">Time in 100-nanosecond units</param>
    public Marker(string name, ulong time)
    {
      this.Name = name;
      this.Time = time;
    }
  }

  /// <summary>
  /// Represent a script in ASF streams
  /// </summary>
  public struct Script
  {
    /// <summary>
    /// Type of the script. Its length must be less than 1024 characters.
    /// </summary>
    public string Type;
    /// <summary>
    /// The script command. 
    /// </summary>
    public string Command;
    /// <summary>
    /// Script time in 100-nanosecond units
    /// </summary>
    public ulong  Time;

    /// <summary>
    /// Scrip constructor
    /// </summary>
    /// <param name="type">Script type</param>
    /// <param name="command">Scrip Command</param>
    /// <param name="time">Time in 100-nanosecond units</param>
    public Script(string type, string command, ulong time)
    {
      this.Type = type;
      this.Command = command;
      this.Time = time; 
    }
  }

  /// <summary>
  /// Represent the WMF attributes that can be present in a header of an ASF stream.
  /// There are defined explicit convertion operator for every possible data type 
  /// represented with this structure. 
  /// </summary>
  public struct WM_Attr
  {
    private WMT_ATTR_DATATYPE m_DataType;
    private object m_Value;
    private string m_Name;
    
    /// <summary>
    /// Initialize the WM_Attr streucture with proper values.
    /// </summary>
    /// <param name="name">Name of the attribute</param>
    /// <param name="type">WMT_ATTR_DATATYPE enum describing the type of the attribute. </param>
    /// <param name="val">The atrtibute value. This param is an obcjet and must match the 
    /// param type, ex. If type is WMT_ATTR_DATATYPE.WMT_TYPE_BOOL val param must be a valid <code>bool</code> and so on. </param>
    public WM_Attr(string name, WMT_ATTR_DATATYPE type, object val)
    {
     m_Name = name;
      this.m_DataType = type;
      switch(type)
      {
        case WMT_ATTR_DATATYPE.WMT_TYPE_BINARY:
          m_Value = (byte[])val;
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_BOOL:
          m_Value = (bool)val;
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_DWORD:
          m_Value = (uint)val;
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_GUID:
          m_Value = (Guid)val;
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_QWORD:
          m_Value = (ulong)val;
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_STRING:
          m_Value = (string)val;
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_WORD:
          m_Value = (ushort)val;
          break;
        default:
          throw new ArgumentException("Invalid data type", "type");
      }
    }

    /// <summary>
    /// ToString is overrided to provide the string representation in "name=value" format.
    /// </summary>
    /// <returns>String represantation of this struct in "name=value" format.</returns>
    public override string ToString()
    {
      return string.Format("{0} = {1}", m_Name, m_Value);
    }

    /// <summary>
    /// Name of the attribute
    /// </summary>
    public string Name
    {
      get { return m_Name;}
    }

    /// <summary>
    /// Data type of the attribute
    /// </summary>
    public WMT_ATTR_DATATYPE DataType
    {
      get { return m_DataType; }
    }
    
    /// <summary>
    /// Read/Write object representing the value of the attribute. 
    /// When writing this property the object must match to the DataType
    /// </summary>
    public object Value
    {
      get
      {
        return m_Value;
      }
      set
      {
        switch ( m_DataType )
        {
          case WMT_ATTR_DATATYPE.WMT_TYPE_BOOL:
            m_Value = (bool)value;
            break;
          case WMT_ATTR_DATATYPE.WMT_TYPE_DWORD:
            m_Value = (uint)value;
            break;
          case WMT_ATTR_DATATYPE.WMT_TYPE_WORD:
            m_Value = (ushort)value;
            break;
          case WMT_ATTR_DATATYPE.WMT_TYPE_QWORD:
            m_Value = (ulong)value;
            break;
          case WMT_ATTR_DATATYPE.WMT_TYPE_GUID:
            m_Value = (Guid)value;
            break;
          case WMT_ATTR_DATATYPE.WMT_TYPE_STRING:
            m_Value = (string)value;
            break;
          case WMT_ATTR_DATATYPE.WMT_TYPE_BINARY:
            // Try Binary values as byte array only
            m_Value = (byte[])value;
            break;
        };
      }
    }
    
    #region Type convertion operators

    public static explicit operator string(WM_Attr attr)
    {
      if (attr.m_DataType == WMT_ATTR_DATATYPE.WMT_TYPE_STRING)
      {
        return (string)attr.m_Value;
      }
      else
      {
        throw new InvalidCastException();
      }
    }

    public static explicit operator bool (WM_Attr attr)
    {
      if (attr.m_DataType == WMT_ATTR_DATATYPE.WMT_TYPE_BOOL)
      {
        return (bool)attr.m_Value;
      }
      else
      {
        throw new InvalidCastException();
      }
    }

    public static explicit operator Guid (WM_Attr attr)
    {
      if (attr.m_DataType == WMT_ATTR_DATATYPE.WMT_TYPE_GUID)
      {
        return (Guid)attr.m_Value;
      }
      else
      {
        throw new InvalidCastException();
      }
    }

    public static explicit operator byte[] (WM_Attr attr)
    {
      if (attr.m_DataType == WMT_ATTR_DATATYPE.WMT_TYPE_BINARY)
      {
        return (byte[])attr.m_Value;
      }
      else
      {
        throw new InvalidCastException();
      }
    }

    public static explicit operator ulong (WM_Attr attr)
    {
      switch(attr.m_DataType)
      {
        case WMT_ATTR_DATATYPE.WMT_TYPE_DWORD:
        case WMT_ATTR_DATATYPE.WMT_TYPE_QWORD:
        case WMT_ATTR_DATATYPE.WMT_TYPE_WORD:
          return (ulong)attr.m_Value;
        default:
          throw new InvalidCastException();
      }
    }

    public static explicit operator long (WM_Attr attr)
    {
      return (long)(ulong)attr;
    }

    public static explicit operator int (WM_Attr attr)
    {
      return (int)(ulong)attr;
    }

    public static explicit operator uint (WM_Attr attr)
    {
      return (uint)(ulong)attr;
    }

    public static explicit operator ushort (WM_Attr attr)
    {
      return (ushort)(ulong)attr;
    }
    #endregion
  }
  
  /// <summary>
	/// Helper class to encapsulate IWMHeaderInfo interface
	/// </summary>
  public class WMHeaderInfo
  {
    private IWMHeaderInfo m_HeaderInfo;

    /// <summary>
    /// WMHeaderInfo constructor
    /// </summary>
    /// <param name="headinfo">IWMHeaderInfo to wrap</param>
    public WMHeaderInfo(IWMHeaderInfo headinfo)
    {
      m_HeaderInfo = headinfo;
    }

    /// <summary>
    /// Internal IWMHeaderInfo
    /// </summary>
    public IWMHeaderInfo HeaderInfo
    {
      get { return m_HeaderInfo; }
    }

    /// <summary>
    /// Add a marher to the header info. Wraps IWMHeaderInfo.AddMarker
    /// </summary>
    /// <param name="m">Marker to add. <see cref="Yeti.WMFSdk.Marker"/></param>
    public void AddMarker(Marker m)
    {
      m_HeaderInfo.AddMarker(m.Name, m.Time);
    }

    /// <summary>
    /// Get a marker. Wraps IWMHeaderInfo.GetMarker
    /// </summary>
    /// <param name="index">Index of the desired marker</param>
    /// <returns>The desired marker. <see cref="Yeti.WMFSdk.Marker"/></returns>
    public Marker GetMarker(int index)
    {
      ulong time;
      ushort namelen = 0;
      StringBuilder name;
      m_HeaderInfo.GetMarker((ushort)index, null, ref namelen, out time);
      name = new StringBuilder(namelen);
      m_HeaderInfo.GetMarker((ushort)index, name, ref namelen, out time);
      return new Marker(name.ToString(), time);
    }

    /// <summary>
    /// Remove a marker. Wraps IWMHeaderInfo.RemoveMarker
    /// </summary>
    /// <param name="index">Index of the marker to remove</param>
    public void RemoveMarker(int index)
    {
      m_HeaderInfo.RemoveMarker((ushort)index);
    }
    
    /// <summary>
    /// Add a scrip. Wraps IWMHeaderInfo.AddScript
    /// </summary>
    /// <param name="s">Scrip to add. <see cref="Yeti.WMFSdk.Script"/></param>
    public void AddScript(Script s)
    {
      m_HeaderInfo.AddScript(s.Type, s.Command, s.Time);
    }

    /// <summary>
    /// Get a script from the header info. Wraps IWMHeaderInfo.GetScript
    /// </summary>
    /// <param name="index">Index of desired script</param>
    /// <returns>Desired script. <see cref="Yeti.WMFSdk.Script"/></returns>
    public Script GetScript(int index)
    {
      ushort commandlen=0, typelen=0;
      ulong time;
      StringBuilder command, type;
      m_HeaderInfo.GetScript((ushort)index, null, ref typelen, null, ref commandlen, out time);
      command = new StringBuilder(commandlen);
      type = new StringBuilder(typelen);
      m_HeaderInfo.GetScript((ushort)index, type, ref typelen, command, ref commandlen, out time);
      return new Script(type.ToString(), command.ToString(), time);
    }
    
    /// <summary>
    /// Remove a script. Wraps IWMHeaderInfo.RemoveScript
    /// </summary>
    /// <param name="index">Index of script to remove</param>
    public void RemoveScript(int index)
    {
      m_HeaderInfo.RemoveScript((ushort)index);
    }

    /// <summary>
    /// Number of scripts in header info object. Wraps IWMHeaderInfo.GetScriptCount
    /// </summary>
    public int ScriptCount
    {
      get 
      { 
        ushort res;
        m_HeaderInfo.GetScriptCount(out res);
        return res;
      }
    }

    /// <summary>
    /// Number of markers in the header info object. Wraps IWMHeaderInfo.GetMarkerCount
    /// </summary>
    public int MarkerCount
    {
      get
      {
        ushort res;
        m_HeaderInfo.GetMarkerCount(out res);
        return res;
      }
    }
    
    /// <summary>
    /// Number of markers in the header info object for the specified stream. Wraps IWMHeaderInfo.GetAttributeCount
    /// </summary>
    /// <param name="StreamNumber">Stream number. Zero means file level attributes</param>
    /// <returns>Number of attributes</returns>
    public int AttributeCount(int StreamNumber)
    {
      ushort res;
      m_HeaderInfo.GetAttributeCount((ushort)StreamNumber, out res);
      return res;
    }

    /// <summary>
    /// File level attribute count.
    /// </summary>
    /// <returns>File level attribute count</returns>
    public int AttributeCount() 
    {
      return AttributeCount(0);
    }

    /// <summary>
    /// Get the attribute by index of specific stream. Wraps Number of markers in the header info object. Wraps IWMHeaderInfo.GetAttibuteByIndex
    /// </summary>
    /// <param name="StreamNumber">Stream number. Zero return a file level atrtibute</param>
    /// <param name="index">Attribute index</param>
    /// <returns>Desired attribute <see cref="Yeti.WMFSdk.WM_Attr"/></returns>
    public WM_Attr GetAttribute(int StreamNumber, int index)
    {
      WMT_ATTR_DATATYPE type;
      StringBuilder name;
      object obj;
      ushort stream = (ushort)StreamNumber;
      ushort namelen = 0;
      ushort datalen = 0;
      m_HeaderInfo.GetAttributeByIndex((ushort)index, ref stream, null, ref namelen, out type, IntPtr.Zero, ref datalen);
      name = new StringBuilder(namelen);
      switch (type)
      {
        case WMT_ATTR_DATATYPE.WMT_TYPE_BOOL:
        case WMT_ATTR_DATATYPE.WMT_TYPE_DWORD:
          obj = (uint)0;
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_GUID:
          obj = Guid.NewGuid();
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_QWORD:
          obj = (ulong)0;
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_WORD:
          obj = (ushort)0;
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_STRING:
        case WMT_ATTR_DATATYPE.WMT_TYPE_BINARY:
          obj = new byte[datalen];
          break;
        default:
          throw new InvalidOperationException(string.Format("Not supported data type: {0}", type.ToString()));
      }
      GCHandle h = GCHandle.Alloc(obj, GCHandleType.Pinned);
      try
      {
        IntPtr ptr = h.AddrOfPinnedObject();
        m_HeaderInfo.GetAttributeByIndex((ushort)index, ref stream, name, ref namelen, out type, ptr, ref datalen);
        switch (type)
        {
          case WMT_ATTR_DATATYPE.WMT_TYPE_STRING:
            obj = Marshal.PtrToStringUni(ptr);
            break;
          case WMT_ATTR_DATATYPE.WMT_TYPE_BOOL:
            obj = ((uint)obj != 0);
            break;
        }
      }
      finally
      {
        h.Free();
      }
      return new WM_Attr(name.ToString(), type, obj);
    }

    /// <summary>
    /// Retrun the file level attribute by index.
    /// </summary>
    /// <param name="index">Index of desired attribute</param>
    /// <returns><see cref="Yeti.WMFSdk.WM_Attr"/></returns>
    public WM_Attr GetAttribute(int index)
    {
      return GetAttribute(0, index);
    }
    
    /// <summary>
    /// Get the header attribute by name and by stream number. Wraps IWMHeaderInfo.GetAttributeByName
    /// </summary>
    /// <param name="StreamNumber">Stream numer. Zero means file level attributes</param>
    /// <param name="name">Nma of the desired attribute</param>
    /// <returns>Desired attribute <see cref="Yeti.WMFSdk.WM_Attr"/></returns>
    public WM_Attr GetAttribute(int StreamNumber, string name)
    {
      ushort stream = (ushort)StreamNumber;
      ushort datalen = 0;
      object obj;
      WMT_ATTR_DATATYPE type;
      
      m_HeaderInfo.GetAttributeByName(ref stream, name, out type, IntPtr.Zero, ref datalen);
      switch (type)
      {
        case WMT_ATTR_DATATYPE.WMT_TYPE_BOOL:
        case WMT_ATTR_DATATYPE.WMT_TYPE_DWORD:
          obj = (uint)0;
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_GUID:
          obj = Guid.NewGuid();
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_QWORD:
          obj = (ulong)0;
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_WORD:
          obj = (ushort)0;
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_STRING:
        case WMT_ATTR_DATATYPE.WMT_TYPE_BINARY:
          obj = new byte[datalen];
          break;
        default:
          throw new InvalidOperationException(string.Format("Not supported data type: {0}", type.ToString()));
      }
      GCHandle h = GCHandle.Alloc(obj, GCHandleType.Pinned);
      try
      {
        IntPtr ptr = h.AddrOfPinnedObject();
        m_HeaderInfo.GetAttributeByName(ref stream, name, out type, ptr, ref datalen);
        switch( type )
        {
          case WMT_ATTR_DATATYPE.WMT_TYPE_STRING:
            obj = Marshal.PtrToStringUni(ptr);
            break;
          case WMT_ATTR_DATATYPE.WMT_TYPE_BOOL:
            obj = ((uint)obj != 0);
            break;
        }
      }
      finally
      {
        h.Free();
      }
      return new WM_Attr(name, type, obj);
    }

    /// <summary>
    /// Return a file level attribute by name.
    /// </summary>
    /// <param name="name">Name of desired attribute</param>
    /// <returns>Desired attribute <see cref="Yeti.WMFSdk.WM_Attr"/></returns>
    public WM_Attr GetAttribute(string name) 
    {
      return GetAttribute(0, name);
    }

    /// <summary>
    /// Set an attribute specifying a stream number. Wraps IWMHeaderInfo.SetAttribute
    /// </summary>
    /// <param name="StreamNumber">Stream number. Zero for file level attributes.</param>
    /// <param name="attr">Attribute to set. <see cref="Yeti.WMFSdk.WM_Attr"/></param>
    public void SetAttribute(int StreamNumber, WM_Attr attr)
    {
      object obj;
      ushort size;
      switch (attr.DataType) 
      {
        case WMT_ATTR_DATATYPE.WMT_TYPE_STRING:
          byte[] arr = Encoding.Unicode.GetBytes((string)attr.Value+(char)0);
          obj = arr;
          size = (ushort)arr.Length;
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_BOOL:
          obj = (uint)((bool)attr?1:0);
          size = 4;
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_BINARY:
          obj = (byte[])attr.Value;
          size = (ushort)((byte[])obj).Length;
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_DWORD:
          obj = (uint)attr;
          size = 4;
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_QWORD:
          obj = (ulong)attr;
          size = 8;
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_WORD:
          obj = (ushort)attr;
          size = 2;
          break;
        case WMT_ATTR_DATATYPE.WMT_TYPE_GUID:
          obj = (Guid)attr;
          size = (ushort)Marshal.SizeOf(typeof(Guid));
          break;
        default:
          throw new ArgumentException("Invalid data type", "attr");          
      }
      GCHandle h = GCHandle.Alloc(obj, GCHandleType.Pinned);
      try
      {
        m_HeaderInfo.SetAttribute((ushort)StreamNumber, attr.Name, attr.DataType, h.AddrOfPinnedObject(), size);
      }
      finally
      {
        h.Free();
      }
    }

    /// <summary>
    /// Set file level attributes
    /// </summary>
    /// <param name="attr">Attribute to set <see cref="Yeti.WMFSdk.WM_Attr"/></param>
    public void SetAttribute(WM_Attr attr)
    {
      SetAttribute(0, attr);
    }

    /// <summary>
    /// Read only. File level attributes indexed by integer index. <see cref="Yeti.WMFSdk.WM_Attr"/>
    /// </summary>
    [System.Runtime.CompilerServices.IndexerName("Attributes")]
    public WM_Attr this[int index]
    {
      get 
      {
        return GetAttribute(index);
      }
    }

    /// <summary>
    /// Read/Write. File level attributes indexed by name. <see cref="Yeti.WMFSdk.WM_Attr"/>
    /// </summary>
    [System.Runtime.CompilerServices.IndexerName("Attributes")]
    public WM_Attr this[string name]
    {
      get
      {
        return GetAttribute(name);
      }
      set
      {
        SetAttribute(value);
      }
    }
  }
}
