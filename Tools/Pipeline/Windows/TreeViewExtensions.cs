// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Windows.Forms;

namespace MonoGame.Tools.Pipeline
{
    public static class TreeViewExtensions
    {
        public static List<TreeNode> AllNodes(this TreeView tree)
        {
            var list = new List<TreeNode>();
            AddTreeNodesRecursive(tree.Nodes, list);
            return list;
        }

        private static void AddTreeNodesRecursive(TreeNodeCollection nodeCollection, List<TreeNode> results)
        {
            foreach (var i in nodeCollection)
            {
                var node = i as TreeNode;
                results.Add(node);

                AddTreeNodesRecursive(node.Nodes, results);
            }
        }
    }

}
