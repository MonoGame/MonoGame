// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Windows.Forms;

namespace MonoGame.Tools.Pipeline
{
    static class TreeViewExtensions
    {
        public static List<TreeNode> AllNodes(this TreeView tree)
        {
            var list = new List<TreeNode>();
            AddTreeNodesRecursive(tree.Nodes, list);
            return list;
        }

        public static void AddTreeNodesRecursive(TreeNodeCollection nodeCollection, List<TreeNode> results)
        {
            foreach (var i in nodeCollection)
            {
                var node = i as TreeNode;
                results.Add(node);

                AddTreeNodesRecursive(node.Nodes, results);
            }
        }

        public static ContentItem GetSelectedContentItem(this TreeView tree)
        {
            if (tree.SelectedNode == null)
                return null;

            return tree.SelectedNode.Tag as ContentItem;
        }

    }

}
