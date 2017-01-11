﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public partial class BuildOutput
    {
        public static Point MouseLocation;
        public static int Count;
        public static int ReqWidth;

        private OutputParser _output;
        private List<BuildItem> _items;
        private CheckCommand _cmdFilterOutput, _cmdAutoScroll, _cmdShowSkipped, _cmdShowSuccessful, _cmdShowCleaned;
        private Image _iconInformation, _iconFail, _iconProcessing, _iconSkip, _iconSucceed, _iconSucceedWithWarnings, _iconStart, _iconEndSucceed, _iconEndFailed;
        private BuildItem _selectedItem;

        public BuildOutput()
        {
            InitializeComponent();
            scrollable1.Style = "BuildOutput";
            
            _output = new OutputParser();

            _iconInformation = Global.GetEtoIcon("Build.Information.png");
            _iconFail = Global.GetEtoIcon("Build.Fail.png");
            _iconProcessing = Global.GetEtoIcon("Build.Processing.png");
            _iconSkip = Global.GetEtoIcon("Build.Skip.png");
            _iconStart = Global.GetEtoIcon("Build.Start.png");
            _iconEndSucceed = Global.GetEtoIcon("Build.EndSucceed.png");
            _iconEndFailed = Global.GetEtoIcon("Build.EndFailed.png");
            _iconSucceed = Global.GetEtoIcon("Build.Succeed.png");
            _iconSucceedWithWarnings = Global.GetEtoIcon("Build.SucceedWithWarnings.png");

            _items = new List<BuildItem>();

            _cmdFilterOutput = new CheckCommand();
            _cmdFilterOutput.MenuText = "Filter Output";
            _cmdFilterOutput.CheckedChanged += CmdFilterOutput_CheckedChanged;
            AddCommand(_cmdFilterOutput);

            _cmdShowSkipped = new CheckCommand();
            _cmdShowSkipped.MenuText = "Show Skipped Files";
            _cmdShowSkipped.CheckedChanged += CmdShowSkipped_CheckedChanged;
            AddCommand(_cmdShowSkipped);

            _cmdShowSuccessful = new CheckCommand();
            _cmdShowSuccessful.MenuText = "Show Successfully Built Files";
            _cmdShowSuccessful.CheckedChanged += CmdShowSuccessful_CheckedChanged;
            AddCommand(_cmdShowSuccessful);

            _cmdShowCleaned = new CheckCommand();
            _cmdShowCleaned.MenuText = "Show Cleaned Files";
            _cmdShowCleaned.CheckedChanged += CmdShowCleaned_CheckedChanged;
            AddCommand(_cmdShowCleaned);

            _cmdAutoScroll = new CheckCommand();
            _cmdAutoScroll.MenuText = "Auto Scroll";
            _cmdAutoScroll.CheckedChanged += CmdAutoScroll_CheckedChanged;
            AddCommand(_cmdAutoScroll);

            MouseLocation = new Point(-1, -1);

#if LINUX
            var scrollView = scrollable1.ControlObject as Gtk.ScrolledWindow;
            scrollView.Vadjustment.ValueChanged += Scrollable1_Scroll;
            scrollView.Hadjustment.ValueChanged += Scrollable1_Scroll;
#endif
        }

        public override void LoadSettings()
        {
            _cmdFilterOutput.Checked = PipelineSettings.Default.FilterOutput;
            _cmdShowSkipped.Checked = PipelineSettings.Default.FilterShowSkipped;
            _cmdShowSuccessful.Checked = PipelineSettings.Default.FilterShowSuccessful;
            _cmdShowCleaned.Checked = PipelineSettings.Default.FilterShowCleaned;
            _cmdAutoScroll.Checked = PipelineSettings.Default.AutoScrollBuildOutput;
        }

        private void CmdFilterOutput_CheckedChanged(object sender, EventArgs e)
        {
            if (_cmdFilterOutput.Checked)
                drawable.Paint -= Drawable_Paint;

            panel.Content = _cmdFilterOutput.Checked ? (Control)scrollable1 : textArea;
            PipelineSettings.Default.FilterOutput = _cmdFilterOutput.Checked;
            
            if (_cmdFilterOutput.Checked)
                drawable.Paint += Drawable_Paint;

            drawable.Invalidate();
        }

        private void CmdShowSkipped_CheckedChanged(object sender, EventArgs e)
        {
            PipelineSettings.Default.FilterShowSkipped = _cmdShowSkipped.Checked;
            drawable.Invalidate();
        }

        private void CmdShowSuccessful_CheckedChanged(object sender, EventArgs e)
        {
            PipelineSettings.Default.FilterShowSuccessful = _cmdShowSuccessful.Checked;
            drawable.Invalidate();
        }

        private void CmdShowCleaned_CheckedChanged(object sender, EventArgs e)
        {
            PipelineSettings.Default.FilterShowCleaned = _cmdShowCleaned.Checked;
            drawable.Invalidate();
        }

        private void CmdAutoScroll_CheckedChanged(object sender, EventArgs e)
        {
            PipelineSettings.Default.AutoScrollBuildOutput = _cmdAutoScroll.Checked;
        }

        public void ClearOutput()
        {
            scrollable1.ScrollPosition = new Point(0, 0);
            textArea.Text = "";
            _items.Clear();
            drawable.Invalidate();
        }

        public void WriteLine(string line)
        {
            textArea.Append(line + Environment.NewLine, _cmdAutoScroll.Checked);

            if (string.IsNullOrEmpty(line))
                return;

            _output.Parse(line);
            line = line.Trim(new[] { ' ', '\n', '\r', '\t' });

            switch (_output.State)
            {
                case OutputState.BuildBegin:
                    _items.Add(new BuildItem { Text = line, Icon = _iconStart });
                    Count = -1;
                    ReqWidth = 0;
                    break;
                case OutputState.Cleaning:
                    _items.Add(new BuildItem
                    {
                        Text = "Cleaning " + PipelineController.Instance.GetRelativePath(_output.Filename),
                        Icon = _iconInformation,
                        Description = line
                    });
                    break;
                case OutputState.Skipping:
                    if (_items[_items.Count - 1].Icon == _iconProcessing)
                        _items[_items.Count - 1].Icon = _iconSucceed;

                    _items.Add(new BuildItem
                    {
                        Text = "Skipping " + PipelineController.Instance.GetRelativePath(_output.Filename),
                        Icon = _iconSkip,
                        Description = _output.Filename
                    });
                    break;
                case OutputState.BuildAsset:
                    if (_items[_items.Count - 1].Icon == _iconProcessing)
                        _items[_items.Count - 1].Icon = _iconSucceed;

                    _items.Add(new BuildItem
                    {
                        Text = "Building " + PipelineController.Instance.GetRelativePath(_output.Filename),
                        Icon = _iconProcessing,
                        Description = _output.Filename
                    });
                    break;
                case OutputState.BuildError:
                    _items[_items.Count - 1].Icon = _iconFail;
                    _items[_items.Count - 1].AddDescription(_output.ErrorMessage);
                    break;
                case OutputState.BuildErrorContinue:
                    _items[_items.Count - 1].AddDescription(_output.ErrorMessage);
                    break;
                case OutputState.BuildWarning:
                    if (_items[_items.Count - 1].Icon == _iconProcessing)
                        _items[_items.Count - 1].Icon = _iconSucceedWithWarnings;
                    _items[_items.Count - 1].AddDescription(_output.ErrorMessage);
                    break;
                case OutputState.BuildEnd:
                    if (_items[_items.Count - 1].Icon == _iconProcessing)
                        _items[_items.Count - 1].Icon = _iconSucceed;

                    _items.Add(new BuildItem
                    {
                        Text = line,
                        Icon = line.Contains("0 failed") ? _iconEndSucceed : _iconEndFailed
                    });
                    break;
                case OutputState.BuildTime:
                    var text = _items[_items.Count - 1].Text.TrimEnd(new[] { '.', ' ' }) + ", " + line;
                    _items[_items.Count - 1].Text = text;
                    Count = _items.Count * 35 - 3;
                    break;
            }

            drawable.Invalidate();
        }

        private void Drawable_MouseMove(object sender, MouseEventArgs e)
        {
            MouseLocation = new Point((int)e.Location.X, (int)e.Location.Y);
            drawable.Invalidate();
        }

        private void Drawable_MouseLeave(object sender, MouseEventArgs e)
        {
            _selectedItem = null;
            MouseLocation = new Point(-1, -1);
            drawable.Invalidate();
        }

        private void Drawable_MouseDown(object sender, MouseEventArgs e)
        {
            if (_selectedItem != null)
                _selectedItem.OnClick();

            ReqWidth = 0;
            foreach (var item in _items)
                if (item.RequestedWidth > ReqWidth)
                    ReqWidth = item.RequestedWidth;

            SetWidth();
            drawable.Invalidate();
        }

        private void Scrollable1_SizeChanged(object sender, EventArgs e)
        {
            SetWidth();
            drawable.Invalidate();
        }


        private void Scrollable1_Scroll(object sender, EventArgs e)
        {
            drawable.Invalidate();
        }

        private void SetWidth()
        {
#if WINDOWS
            var scrollsize = (drawable.Height >= scrollable1.Height) ? System.Windows.SystemParameters.VerticalScrollBarWidth : 0.0;
            var width = (int)(Width - scrollsize - System.Windows.SystemParameters.BorderWidth * 2);

            if (ReqWidth > width)
                width = ReqWidth;

            if (drawable.Width != width)
                drawable.Width = width;
#endif
        }

        private void Drawable_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var count = _items.Count;
            var y = 0;

            g.Clear(DrawInfo.BackColor);

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];

                // Skip Skipped items
                if (!PipelineSettings.Default.FilterShowSkipped && item.Icon == _iconSkip)
                    continue;

                // Skip Successful items
                if (!PipelineSettings.Default.FilterShowSuccessful && item.Icon == _iconSucceed)
                    continue;

                // Skip Cleaned items
                if (!PipelineSettings.Default.FilterShowCleaned && item.Icon == _iconInformation)
                    continue;

                // Check if the item is in the visible rectangle
                if (y + item.Height >= scrollable1.ScrollPosition.Y && y < scrollable1.ScrollPosition.Y + scrollable1.Height)
                {
                    // Check if the item is selected
                    if (MouseLocation.Y > y && MouseLocation.Y < y + item.Height)
                        _selectedItem = item;

                    // Draw item
                    item.Draw(g, y, drawable.Width);
                }

                // Add border
                y += item.Height + 3;
            }
            
            drawable.Height = Math.Max(y - 3, 1);
            SetWidth();

#if WINDOWS
            if (Count == -1 && PipelineSettings.Default.AutoScrollBuildOutput)
                scrollable1.ScrollPosition = new Point(0, y - scrollable1.Height);
#endif
        }
    }
}
