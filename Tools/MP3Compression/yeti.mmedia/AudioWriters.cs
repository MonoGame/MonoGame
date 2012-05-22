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
using System.Windows.Forms;
using WaveLib;

namespace Yeti.MMedia
{
  public abstract class AudioWriter : BinaryWriter
  {
    protected WaveFormat m_InputDataFormat;

    public AudioWriter(Stream Output, WaveFormat InputDataFormat)
      :base(Output, System.Text.Encoding.ASCII)
    {
      m_InputDataFormat = InputDataFormat;
    }

    
    public AudioWriter(Stream Output, AudioWriterConfig Config)
      :this(Output, Config.Format)
    {
    }

    
    protected abstract int GetOptimalBufferSize();

    private static int m_ConfigWidth = 368;
    private static int m_ConfigHeight = 264;

    protected virtual AudioWriterConfig GetWriterConfig()
    {
      return new AudioWriterConfig(m_InputDataFormat);
    }

    public AudioWriterConfig WriterConfig
    {
      get
      {
        return GetWriterConfig();
      }
    }

    /// <summary>
    /// Width of the config control
    /// </summary>
    public static int ConfigWidth
    {
      get
      {
        return m_ConfigWidth;
      }
      set
      {
        m_ConfigWidth = value;
      }
    }

    /// <summary>
    /// Height of the config control
    /// </summary>
    public static int ConfigHeight
    {
      get 
      {
        return m_ConfigHeight;
      }
      set
      {
        m_ConfigHeight = value;
      }
    }

    /// <summary>
    /// Optimal size of the buffer used in each write operation to obtain best performance.
    /// This value must be greater than 0 
    /// </summary>
    public int OptimalBufferSize
    {
      get
      {
        return GetOptimalBufferSize();
      }
    }
    
    public override void Write(string value)
    {
      throw new NotSupportedException("Write(string value) is not supported");
    }
  
    public override void Write(float value)
    {
      throw new NotSupportedException("Write(float value) is not supported");
    }
  
    public override void Write(ulong value)
    {
      throw new NotSupportedException("Write(ulong value) is not supported");
    }
  
    public override void Write(long value)
    {
      throw new NotSupportedException("Write(long value) is not supported");
    }
  
    public override void Write(uint value)
    {
      throw new NotSupportedException("Write(uint value) is not supported");
    }
  
    public override void Write(int value)
    {
      throw new NotSupportedException("Write(int value) is not supported");
    }
  
    public override void Write(ushort value)
    {
      throw new NotSupportedException("Write(ushort value) is not supported");
    }
  
    public override void Write(short value)
    {
      throw new NotSupportedException("Write(short value) is not supported");
    }
  
    public override void Write(decimal value)
    {
      throw new NotSupportedException("Write(decimal value) is not supported");
    }
  
    public override void Write(double value)
    {
      throw new NotSupportedException("Write(double value) is not supported");
    }
  
    public override void Write(char[] chars, int index, int count)
    {
      throw new NotSupportedException("Write(char[] chars, int index, int count) is not supported");
    }
  
    public override void Write(char[] chars)
    {
      throw new NotSupportedException("Write(char[] chars) is not supported");
    }
  
    public override void Write(char ch)
    {
      throw new NotSupportedException("Write(char ch) is not supported");
    }
  
    public override void Write(sbyte value)
    {
      throw new NotSupportedException("Write(sbyte value) is not supported");
    }
  
    public override void Write(byte value)
    {
      throw new NotSupportedException("Write(byte value) is not supported");
    }
  
    public override void Write(bool value)
    {
      throw new NotSupportedException("Write(bool value) is not supported");
    }
  }
}