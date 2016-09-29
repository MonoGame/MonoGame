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
        private Image _iconInformation, _iconFail, _iconProcessing, _iconSkip, _iconSucceed, _iconStart, _iconEndSucceed, _iconEndFailed;
        private Eto.Forms.CheckCommand _cmdFilterOutput, _cmdAutoScroll;

        public BuildOutput()
        {
            InitializeComponent();

            _output = new OutputParser();

            _iconInformation = Global.GetXwtIcon("Build.Information.png");
            _iconFail = Global.GetXwtIcon("Build.Fail.png");
            _iconProcessing = Global.GetXwtIcon("Build.Processing.png");
            _iconSkip = Global.GetXwtIcon("Build.Skip.png");
            _iconStart = Global.GetXwtIcon("Build.Start.png");
            _iconEndSucceed = Global.GetXwtIcon("Build.EndSucceed.png");
            _iconEndFailed = Global.GetXwtIcon("Build.EndFailed.png");
            _iconSucceed = Global.GetXwtIcon("Build.Succeed.png");

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
                    AddItem(_iconStart, line);
                    break;
                case OutputState.Cleaning:
                    AddItem(_iconInformation, "Cleaning " + PipelineController.Instance.GetRelativePath(_output.Filename));
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
                    if (line.Contains("0 failed"))
                        AddItem(_iconEndSucceed, line);
                    else
                        AddItem(_iconEndFailed, line);
                    break;
                case OutputState.BuildTime:
                    var node = _treeStore.GetNavigatorAt(_last);
                    var text = node.GetValue(_dataText);

                    AddItem(node.GetValue(_dataImage), text.TrimEnd(new[] { '.', ' ' }) + ", " + line);
                    node.Remove();
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

