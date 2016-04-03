// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace MonoGame.Tools.Pipeline.Windows.Controls
{
    public partial class TabControlEx : TabControl
    {
        const int TCM_ADJUSTRECT = 0x1328;

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool HideTabHeader { get; set; }

        public TabControlEx():base()
        {
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case TCM_ADJUSTRECT:
                    if (HideTabHeader && !DesignMode)
                    {
                        m.Result = (IntPtr)1;
                        return;
                    }
                    break;
            }
            
            base.WndProc(ref m);
        }        
    }
}
