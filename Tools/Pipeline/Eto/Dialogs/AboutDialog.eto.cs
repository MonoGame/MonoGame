// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Reflection;
using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class AboutDialog : DialogBase
    {
        DynamicLayout dynamic1;
        ImageView image1;
        Label labelTitle, labelVersion, labelDescription, labelCopyright;
        LinkButton linkWebsite;
        
        private void InitializeComponent()
        {
            Title = "About";
            Width = 300;
            Height = 300;

            DefaultButton.Visible = false;
            AbortButton.Text = "Close";

            dynamic1 = new DynamicLayout();
            dynamic1.DefaultSpacing = new Size(6, 6);
            dynamic1.Padding = 6;
            dynamic1.BeginVertical();

            image1 = new ImageView();
            image1.Image = Bitmap.FromResource("Icons.monogame.png");
            image1.Height = 128;
            dynamic1.Add(image1, true, true);

            labelTitle = new Label();
            labelTitle.TextAlignment = TextAlignment.Center;
            labelTitle.Font = new Font(labelTitle.Font.Family, labelTitle.Font.Size, FontStyle.Bold);
            labelTitle.Text = GetAttribute(typeof(AssemblyTitleAttribute));
            dynamic1.Add(labelTitle, true);

            labelVersion = new Label();
            labelVersion.TextAlignment = TextAlignment.Center;
            labelVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            dynamic1.Add(labelVersion, true);

            labelDescription = new Label();
            labelDescription.TextAlignment = TextAlignment.Center;
            labelDescription.Text = GetAttribute(typeof(AssemblyDescriptionAttribute));
            dynamic1.Add(labelDescription, true);

            linkWebsite = new LinkButton();
            linkWebsite.Style = "Center";
            linkWebsite.Text = "MonoGame Website";
            dynamic1.Add(linkWebsite, true);

            labelCopyright = new Label();
            labelCopyright.TextAlignment = TextAlignment.Center;
            labelCopyright.Text = GetAttribute(typeof(AssemblyCopyrightAttribute));
            dynamic1.Add(labelCopyright, true);

            dynamic1.EndVertical();
            CreateContent(dynamic1);

            linkWebsite.Click += LinkWebsite_Click;
        }
    }
}
