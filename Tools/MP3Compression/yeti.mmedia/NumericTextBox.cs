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
using System.Windows.Forms;
using System.Globalization;

namespace Yeti.Controls
{
	/// <summary>
	/// Define a TextBox that allow only integer numbers.
	/// </summary>
	public class NumericTextBox : TextBox
	{
		public NumericTextBox()
      :base()
		{
		}

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
      base.OnKeyPress(e);
      if ( (!e.Handled) && ( "1234567890\b".IndexOf(e.KeyChar) < 0) )
      {
        Yeti.Sys.Win32.MessageBeep(Yeti.Sys.BeepType.SimpleBeep);
        e.Handled = true;
      }
    }

	  public event EventHandler FormatError;
    public event EventHandler FormatValid;

    protected virtual void OnFormatError(EventArgs e)
    {
      if (FormatError != null)
      {
        FormatError(this, e);
      }
    }

    protected virtual void OnFormatValid(EventArgs e)
    {
      if (FormatValid != null)
      {
        FormatValid(this, e);
      }
    }

    protected override void OnTextChanged(EventArgs e)
    {
      try
      {
        int.Parse(this.Text, NumberStyles.Integer);
        OnFormatValid(e);
      }
      catch
      {
        OnFormatError(e);
      }
      base.OnTextChanged (e);
    }

    public int Value
    {
      get
      {
        return int.Parse(this.Text, NumberStyles.Integer);
      }
      set
      {
        this.Text = value.ToString();
      }
    }
  }
}
