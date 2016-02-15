// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Windows.Forms;

namespace MonoGame.Tools.Pipeline.Windows.Controls
{
    public partial class FilterOutputControl : TreeView
    {
        const int WM_VSCROLL = 0x0115;
        const int SB_BOTTOM  = 0x07;
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        private BuildIcons _buildIcons;
        private TreeNode _lastTreeNode;

        private OutputParser outputParser;
        private string _prevFilename;
        
        Uri folderUri;
        Uri outputUri;

        public FilterOutputControl(): base()
        {
            this._buildIcons = new BuildIcons();
            this.ImageList = _buildIcons.Icons;
            outputParser = new OutputParser();
        }
        
        internal void SetBaseFolder(IController controller)
        {
            string pl = ((PipelineController)controller).ProjectLocation;
            if (!pl.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                pl += System.IO.Path.DirectorySeparatorChar;
            folderUri = new Uri(pl);

            string pod = ((PipelineController)controller).ProjectOutputDir;
            if (!pod.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                pod += System.IO.Path.DirectorySeparatorChar;
            pod = Path.Combine(pl, pod);
            outputUri = new Uri(pod);

            outputParser.Reset();
        }

        protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
        {
            // disable selection for all nodes
            e.Cancel = true;
        }

        internal void AppendText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            this.SuspendLayout();
            this.BeginUpdate();

            outputParser.Parse(text);

            text = text.TrimEnd(new[] { ' ','\n','\r','\t' });
            
            switch (outputParser.State)
            {
                case OutputState.BuildBegin:
                    var tn = AddItem(BuildIcons.BeginEnd, text);
                    break;
                case OutputState.Cleaning:
                    tn = AddItem(BuildIcons.Clean, "Cleaning " + GetRelativeOutputPath(outputParser.Filename));
                    tn.ToolTipText = text; 
                    AddSubItem(tn, text);
                    break;
                case OutputState.Skipping:
                    tn = AddItem(BuildIcons.Skip, "Skipping " + GetRelativePath(outputParser.Filename));
                    tn.ToolTipText = text;
                    AddSubItem(tn, text);
                    break;
                case OutputState.BuildAsset:
                    tn = AddItem(BuildIcons.Skip, "Building " + GetRelativePath(outputParser.Filename));
                    tn.ToolTipText = text;
                    AddSubItem(tn, text);
                    break;
                case OutputState.BuildError:
                    _lastTreeNode.ImageIndex = BuildIcons.Fail;
                    _lastTreeNode.SelectedImageIndex = BuildIcons.Fail;
                    _lastTreeNode.ToolTipText += Environment.NewLine + Environment.NewLine + outputParser.ErrorMessage;
                    AddSubItem(_lastTreeNode, outputParser.ErrorMessage).ForeColor = System.Drawing.Color.DarkRed;
                    break;
                case OutputState.BuildErrorContinue:
                    _lastTreeNode.ToolTipText += Environment.NewLine + outputParser.ErrorMessage;
                    AddSubItem(_lastTreeNode, outputParser.ErrorMessage).ForeColor = System.Drawing.Color.DarkRed;
                    break;
                case OutputState.BuildEnd:
                    tn = AddItem(BuildIcons.BeginEnd, text);
                    break;
                case OutputState.BuildTime:
                    _lastTreeNode.Text = _lastTreeNode.Text.TrimEnd(new[] {'.', ' '} ) + ", " + text;
                    SendMessage(this.Handle, WM_VSCROLL, SB_BOTTOM, 0); //scroll down to the end
                    break;
            }

            _prevFilename = outputParser.Filename;
            
            this.EndUpdate();
            this.ResumeLayout();  
        }
        
        private TreeNode AddItem(int iconIdx, string text)
        {
            if (_lastTreeNode != null && _lastTreeNode.ImageIndex == BuildIcons.Processing)
            {
                _lastTreeNode.ImageIndex = BuildIcons.Succeed;
                _lastTreeNode.SelectedImageIndex = BuildIcons.Succeed;
            }
            
            _lastTreeNode = new TreeNode(text, iconIdx, iconIdx);
            this.Nodes.Add(_lastTreeNode);
            //tmpIter.EnsureVisible(); // too much flicker & doesn't work between BeginUpdate()/EndUpdate()
            SendMessage(this.Handle, WM_VSCROLL, SB_BOTTOM, 0); //scroll down to the end
            return _lastTreeNode;
        }

        private static TreeNode AddSubItem(TreeNode treeNode, string text)
        {
            var subTreeNode = new TreeNode(text, BuildIcons.Null, BuildIcons.Null);
            treeNode.Nodes.Add(subTreeNode);
            return subTreeNode;
        }
        
        private string GetRelativePath(string path)
        {
            var pathUri = new Uri(path);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', System.IO.Path.DirectorySeparatorChar));     
        }

        private string GetRelativeOutputPath(string path)
        {
            var pathUri = new Uri(path);
            return Uri.UnescapeDataString(outputUri.MakeRelativeUri(pathUri).ToString().Replace('/', System.IO.Path.DirectorySeparatorChar));
        }
        
        internal void Clear()
        {
            this.Nodes.Clear();
        }
    }
}
