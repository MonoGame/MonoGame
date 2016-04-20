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
            AbortButton.Visible &= !Global.UseHeaderBar;
            AbortButton.Text = "Close";

            dynamic1 = new DynamicLayout();
            dynamic1.DefaultSpacing = new Size(6, 6);
            dynamic1.Padding = 6;
            dynamic1.BeginVertical();

            image1 = new ImageView();
            image1.Image = Bitmap.FromResource("Icons.monogame.png");
            if (Global.Unix)
                image1.Height = 128;
            dynamic1.Add(image1, true, true);

            labelTitle = new Label();
            labelTitle.Font = new Font(labelTitle.Font.Family, labelTitle.Font.Size, FontStyle.Bold);
            labelTitle.Text = GetAttribute(typeof(AssemblyTitleAttribute));
            dynamic1.AddAutoSized(labelTitle, centered: true);

            labelVersion = new Label();
            labelVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            dynamic1.AddAutoSized(labelVersion, centered: true);

            labelDescription = new Label();
            labelDescription.Text = GetAttribute(typeof(AssemblyDescriptionAttribute));
            dynamic1.AddAutoSized(labelDescription, centered: true);

            linkWebsite = new LinkButton();
            linkWebsite.Text = "MonoGame Website";
            dynamic1.AddAutoSized(linkWebsite, centered: true);

            labelCopyright = new Label();
            labelCopyright.Text = GetAttribute(typeof(AssemblyCopyrightAttribute));
            dynamic1.AddAutoSized(labelCopyright, centered: true);

            dynamic1.EndVertical();
            CreateContent(dynamic1);

            linkWebsite.Click += LinkWebsite_Click;
        }
    }
}
