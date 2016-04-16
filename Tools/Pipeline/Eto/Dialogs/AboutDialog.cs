// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Reflection;

namespace MonoGame.Tools.Pipeline
{
    partial class AboutDialog : DialogBase
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        private string GetAttribute(Type type)
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(type, false);

            if (attributes.Length == 0)
                return "";

            return type.GetProperties()[0].GetValue(attributes[0], null).ToString();
        }

        private void LinkWebsite_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.monogame.net/");
        }
    }
}
