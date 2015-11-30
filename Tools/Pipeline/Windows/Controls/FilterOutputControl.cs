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

        BuildIcons _buildIcons;
        TreeNode tmpIter;
        
        Uri folderUri;
        Uri outputUri;

        public FilterOutputControl(): base()
        {
            this._buildIcons = new BuildIcons();
            this.ImageList = _buildIcons.Icons;
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

            text = text.TrimEnd(new[] { ' ','\n','\r','\t' });
            
            if (text.StartsWith("Build "))
            {
                AddItem(BuildIcons.BeginEnd, text);
            }
            else if (text.StartsWith("Time "))
            {
                tmpIter.Text = tmpIter.Text.TrimEnd(new[] {'.', ' '} ) + ", " + text;
                SendMessage(this.Handle, WM_VSCROLL, SB_BOTTOM, 0); //scroll down to the end
            }
            else if (text.StartsWith("Skipping"))
            {
                var tn = AddItem(BuildIcons.Skip, "Skipping " + GetRelativePath(text.Substring(9)));
                tn.ToolTipText = text; 
                AddSubItem(tn, text.Substring(9));
            }
            else if (text.StartsWith("Cleaning"))
            {
                var tn = AddItem(BuildIcons.Clean, "Cleaning " + GetRelativeOutputPath(text.Substring(9)));
                tn.ToolTipText = text;
                AddSubItem(tn, text.Substring(9));
            }
            else if (Char.IsLetter(text[0]) && text[1]==':' && !text.Contains("error"))
            {
                var tn = AddItem(BuildIcons.Processing, "Building " + GetRelativePath(text));
                tn.ToolTipText = text;
                AddSubItem(tn, text);
            }
            else
            {
                if (text.ToLower().Contains("error") || (text.Contains("System") && text.Contains("Exception")))
                {
                    tmpIter.ImageIndex = BuildIcons.Fail;
                    tmpIter.SelectedImageIndex = BuildIcons.Fail;
                    tmpIter.ToolTipText += Environment.NewLine;
                }

                AddSubItem(tmpIter, text);
                tmpIter.ToolTipText += Environment.NewLine + text;
            }

            this.EndUpdate();
            this.ResumeLayout();  
        }
        
        private TreeNode AddItem(int iconIdx, string text)
        {
            if (tmpIter != null && tmpIter.ImageIndex == BuildIcons.Processing)
            {
                tmpIter.ImageIndex = BuildIcons.Succeed;
                tmpIter.SelectedImageIndex = BuildIcons.Succeed;
            }
            
            tmpIter = new TreeNode(text, iconIdx, iconIdx);
            this.Nodes.Add(tmpIter);
            //tmpIter.EnsureVisible(); // too much flicker & doesn't work between BeginUpdate()/EndUpdate()
            SendMessage(this.Handle, WM_VSCROLL, SB_BOTTOM, 0); //scroll down to the end
            return tmpIter;
        }

        private static void AddSubItem(TreeNode treeNode, string text)
        {
            var subTreeNode = new TreeNode(text, BuildIcons.Null, BuildIcons.Null);
            treeNode.Nodes.Add(subTreeNode);
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
