using System.Collections;
using System.Collections.Generic;

namespace MonoGame.Tools.Pipeline
{
    internal delegate void SelectionModified(Selection selection, object source);

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
        
        public void Add(IProjectItem item, object source)
        {
            _list.Add(item);
            if (Modified != null)
                Modified(this, source);
        }

        public void Remove(IProjectItem item, object source)
        {
            _list.Remove(item);
            if (Modified != null)
                Modified(this, source);
        }

        public void Clear(object source)
        {
            _list.Clear();
            if (Modified != null)
                Modified(this, source);
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
