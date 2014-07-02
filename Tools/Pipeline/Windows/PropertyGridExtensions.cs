using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonoGame.Tools.Pipeline
{
    public static class PropertyGridExtensions
    {
        public static IEnumerable<GridItem> EnumerateGroups(this PropertyGrid propertyGrid)
        {
            if (propertyGrid.SelectedGridItem == null)
                yield break;

            foreach (var i in propertyGrid.EnumerateItems())
            {
                if (i.Expandable)
                {
                    yield return i;
                }
            }
        }

        public static IEnumerable<GridItem> EnumerateItems(this PropertyGrid propertyGrid)
        {
            if (propertyGrid.SelectedGridItem == null)
                yield break;

            var root = propertyGrid.SelectedGridItem;
            while (root.Parent != null)
                root = root.Parent;

            yield return root;

            foreach (var i in root.EnumerateItems())
            {
                yield return i;
            }            
        }

        public static GridItem GetGroup(this PropertyGrid propertyGrid, string label)
        {
            if (propertyGrid.SelectedGridItem == null)
                return null;

            foreach (var i in propertyGrid.EnumerateItems())
            {
                if (i.Expandable && i.Label == label)
                {
                    return i;
                }
            }

            return null;
        }

        private static IEnumerable<GridItem> EnumerateItems(this GridItem item)
        {
            foreach (GridItem i in item.GridItems)
            {
                yield return i;

                foreach (var j in i.EnumerateItems())
                {
                    yield return j;
                }
            }
        }        
    }
}
