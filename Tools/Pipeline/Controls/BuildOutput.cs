// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Xwt;
using Xwt.Drawing;

namespace MonoGame.Tools.Pipeline
{
    public partial class BuildOutput
    {
        private OutputParser _output;
        private TreeStore _treeStore;
        private readonly DataField<Image> _dataImage;
        private readonly DataField<string> _dataText;
        private TreePosition _last;
        private Image _iconClean, _iconFail, _iconProcessing, _iconSkip, _iconStartEnd, _iconSucceed;
        private Eto.Forms.CheckCommand _cmdFilterOutput, _cmdAutoScroll;

        public BuildOutput()
        {
            InitializeComponent();

            _output = new OutputParser();

            _iconClean = Image.FromResource("Build.Clean.png");
            _iconFail = Image.FromResource("Build.Fail.png");
            _iconProcessing = Image.FromResource("Build.Processing.png");
            _iconSkip = Image.FromResource("Build.Skip.png");
            _iconStartEnd = Image.FromResource("Build.StartEnd.png");
            _iconSucceed = Image.FromResource("Build.Succeed.png");

            _dataImage = new DataField<Image>();
            _dataText = new DataField<string>();

            _treeStore = new TreeStore(_dataImage, _dataText);

            _cmdFilterOutput = new Eto.Forms.CheckCommand();
            _cmdFilterOutput.MenuText = "Filter Output";
            _cmdFilterOutput.CheckedChanged += CmdFilterOutput_CheckedChanged;
            AddCommand(_cmdFilterOutput);

            _cmdAutoScroll = new Eto.Forms.CheckCommand();
            _cmdAutoScroll.MenuText = "Auto Scroll";
            _cmdAutoScroll.CheckedChanged += CmdAutoScroll_CheckedChanged;
            AddCommand(_cmdAutoScroll);

            treeView.DataSource = _treeStore;
            treeView.Columns.Add("", _dataImage, _dataText);
        }

        public override void LoadSettings()
        {
            _cmdFilterOutput.Checked = PipelineSettings.Default.FilterOutput;
            _cmdAutoScroll.Checked = PipelineSettings.Default.AutoScrollBuildOutput;
        }

        private void CmdFilterOutput_CheckedChanged(object sender, EventArgs e)
        {
            panel.Content = _cmdFilterOutput.Checked ? treeView.ToEto() : textArea;
            PipelineSettings.Default.FilterOutput = _cmdFilterOutput.Checked;
        }

        private void CmdAutoScroll_CheckedChanged(object sender, EventArgs e)
        {
            PipelineSettings.Default.AutoScrollBuildOutput = _cmdAutoScroll.Checked;
        }

        public void ClearOutput()
        {
            textArea.Text = "";
            _treeStore.Clear();
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
                    _treeStore.GetNavigatorAt(_last).SetValue(_dataImage, _iconFail);
                    AddItem(_output.ErrorMessage);
                    break;
                case OutputState.BuildErrorContinue:
                    AddItem(_output.ErrorMessage);
                    break;
                case OutputState.BuildWarning:
                    AddItem(_output.ErrorMessage);
                    break;
                case OutputState.BuildEnd:
                    AddItem(_iconStartEnd, line);
                    break;
                case OutputState.BuildTime:
                    var text = _treeStore.GetNavigatorAt(_last).GetValue(_dataText);
                    _treeStore.GetNavigatorAt(_last).SetValue(_dataText, text.TrimEnd(new[] { '.', ' ' }) + ", " + line);
                    break;
            }
        }

        private void AddItem(Image image, string text)
        {
            var item = _treeStore.AddNode();
            item.SetValue(_dataImage, image);
            item.SetValue(_dataText, text);

            if (_last != null && _treeStore.GetNavigatorAt(_last).GetValue(_dataImage) == _iconProcessing)
                _treeStore.GetNavigatorAt(_last).SetValue(_dataImage, _iconSucceed);

            if (_cmdAutoScroll.Checked)
                treeView.ScrollToRow(item.CurrentPosition);

            _last = item.CurrentPosition;
        }

        private void AddItem(string text)
        {
            _treeStore.GetNavigatorAt(_last).AddChild().SetValue(_dataText, text);
        }
    }
}

