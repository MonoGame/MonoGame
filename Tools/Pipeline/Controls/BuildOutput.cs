// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Forms;
using Eto.Drawing;

namespace MonoGame.Tools.Pipeline
{
    public partial class BuildOutput
    {
        public bool Filtered
        {
            set
            {
                this.Content = value ? (Control)treeView : textArea;
            }
        }

        private OutputParser _output;
        private TreeItem _root, _last;
        private Icon _iconClean, _iconFail, _iconProcessing, _iconSkip, _iconStartEnd, _iconSucceed;

        public BuildOutput()
        {
            InitializeComponent();

            _output = new OutputParser();

            _iconClean = Icon.FromResource("Build.Clean.png");
            _iconFail = Icon.FromResource("Build.Fail.png");
            _iconProcessing = Icon.FromResource("Build.Processing.png");
            _iconSkip = Icon.FromResource("Build.Skip.png");
            _iconStartEnd = Icon.FromResource("Build.StartEnd.png");
            _iconSucceed = Icon.FromResource("Build.Succeed.png");

            treeView.DataStore = _root = new TreeItem();
        }

        public void ClearOutput()
        {
            textArea.Text = "";
            treeView.DataStore = _root = new TreeItem();
        }

        public void WriteLine(string line)
        {
            textArea.Append(line + Environment.NewLine, true);

            if (string.IsNullOrEmpty(line))
                return;

            _output.Parse(line);
            line = line.Trim(new[] { ' ', '\n', '\r', '\t' });

            switch (_output.State)
            {
                case OutputState.BuildBegin:
                    AddItem(_iconStartEnd, line);
                    break;
                case OutputState.Cleaning:
                    AddItem(_iconClean, "Cleaning " + PipelineController.Instance.GetRelativePath(_output.Filename));
                    AddItem(line);
                    break;
                case OutputState.Skipping:
                    AddItem(_iconSkip, "Skipping " + PipelineController.Instance.GetRelativePath(_output.Filename));
                    AddItem(line);
                    break;
                case OutputState.BuildAsset:
                    AddItem(_iconProcessing, "Building " + PipelineController.Instance.GetRelativePath(_output.Filename));
                    AddItem(line);
                    break;
                case OutputState.BuildError:
                    _last.Image = _iconFail;
                    AddItem(_output.ErrorMessage);
                    break;
                case OutputState.BuildErrorContinue:
                    AddItem(_output.ErrorMessage);
                    break;
                case OutputState.BuildEnd:
                    AddItem(_iconStartEnd, line);
                    break;
                case OutputState.BuildTime:
                    _last.Text = _last.Text.TrimEnd(new[] { '.', ' ' }) + ", " + line;
                    break;
            }
        }

        private void AddItem(Icon icon, string text)
        {
            var item = new TreeItem();
            item.Image = icon;
            item.Text = text;

            if (_last != null && _last.Image == _iconProcessing)
            {
                _last.Image = _iconSucceed;
                treeView.RefreshItem(_last);
            }

            _last = item;
            _root.Children.Add(item);

            treeView.RefreshItem(item);

            treeView.Style = "Scroll";
            treeView.Style = "";
        }

        private void AddItem(string text)
        {
            var item = new TreeItem();
            item.Text = text;

            _last.Children.Add(item);
        }
    }
}

