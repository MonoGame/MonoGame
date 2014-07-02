using System;
using System.Collections;
using System.Collections.Generic;

namespace MonoGame.Tools.Pipeline
{
    internal struct SelectionModifiedArgs
    {
        public IEnumerable<IProjectItem> PreviousItems;
    }

    internal delegate void SelectionModified(object sender, Selection selection, SelectionModifiedArgs args);    

    internal class Selection : IEnumerable<IProjectItem>
    {
        private readonly List<IProjectItem> _list;

        public event SelectionModified Modified;        

        public int Count
        {
            get { return _list.Count; }
        }

        public Selection()
        {
            _list = new List<IProjectItem>();
        }
        
        public void Add(object sender, IProjectItem item)
        {
            var args = new SelectionModifiedArgs()
                {
                    PreviousItems = new List<IProjectItem>(_list),
                };
            
            _list.Add(item);

            if (Modified != null)
                Modified(sender, this, args);
        }

        public void Remove(object sender, IProjectItem item)
        {
            var args = new SelectionModifiedArgs()
            {
                PreviousItems = new List<IProjectItem>(_list),
            };

            _list.Remove(item);

            if (Modified != null)
                Modified(sender, this, args);
        }

        public void Clear(object sender)
        {
            var args = new SelectionModifiedArgs()
            {
                PreviousItems = new List<IProjectItem>(_list),
            };

            _list.Clear();

            if (Modified != null)
                Modified(sender, this, args);
        }

        public bool Equals(IEnumerable<IProjectItem> other)
        {
            foreach (var i in other)
            {
                var found = false;
                foreach (var j in _list)
                {
                    if (j.OriginalPath.Equals(i.OriginalPath, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    return false;                
            }

            foreach (var i in _list)
            {
                var found = false;
                foreach (var j in other)
                {
                    if (j.OriginalPath.Equals(i.OriginalPath, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return (_list != null ? _list.GetHashCode() : 0);
        }

        public override bool Equals(object obj)
        {
            var other = obj as IEnumerable<IProjectItem>;
            if (other == null)
                return false;

            return Equals(other);
        }

        public IEnumerator<IProjectItem> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
