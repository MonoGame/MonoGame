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

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Forms;
using WaveLib;

namespace Yeti.MMedia
{
	
  /// <summary>
  /// 
  /// </summary>
  [Serializable]
  public class AudioWriterConfig : ISerializable
  {
    protected WaveFormat m_Format;

    /// <summary>
    /// A constructor with this signature must be implemented by descendants. 
    /// <see cref="System.Runtime.Serialization.ISerializable"/> for more information
    /// </summary>
    /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> where is the serialized data.</param>
    /// <param name="context">The source (see <see cref="System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
    protected AudioWriterConfig(SerializationInfo info, StreamingContext context)
    {
      int rate = info.GetInt32("Format.Rate");
      int bits = info.GetInt32("Format.Bits");
      int channels = info.GetInt32("Format.Channels");
      m_Format = new WaveFormat(rate, bits, channels);
    }

    public AudioWriterConfig(WaveFormat f)
    {
      m_Format = new WaveFormat(f.nSamplesPerSec, f.wBitsPerSample, f.nChannels);
    }

    public AudioWriterConfig()
      :this(new WaveFormat(44100, 16, 2))
    {
    }

    public WaveFormat Format
    {
      get
      {
        return m_Format;
      }
      set
      {
        m_Format = value;
      }
    }

    #region ISerializable Members

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Format.Rate", m_Format.nSamplesPerSec);
      info.AddValue("Format.Bits", m_Format.wBitsPerSample);
      info.AddValue("Format.Channels", m_Format.nChannels);
    }

    #endregion
  }
  
  public interface IConfigControl
  {
    void DoApply();
    void DoSetInitialValues();
    Control ConfigControl { get; }
    string ControlName { get; }
    event EventHandler ConfigChange;
  }

  public interface IEditAudioWriterConfig : IConfigControl
	{
    AudioWriterConfig Config { get; set; }
	}

  public interface IEditFormat : IConfigControl
  {
    WaveFormat Format { get; set; }
  }

}
