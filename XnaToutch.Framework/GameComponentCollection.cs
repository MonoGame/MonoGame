#region License
/*
Microsoft Public License (Ms-PL)
XnaTouch - Copyright © 2009 The XnaTouch Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

using System;
using System.Collections.Generic;

namespace XnaTouch.Framework
{
    public sealed class GameComponentCollection : IList<GameComponent>
    {
        //Since our DrawOrder & UpdateOrder are independent, if we want to 
        //perform updates and draws in the correct order, we have two options
        // 1) Sort every cycle (slower, but simpler)
        // 2) Maintain Two Lists (a bit more complex and bug-prone, but more efficient) 

        List<GameComponent> _components = new List<GameComponent>();
        List<DrawableGameComponent> _drawableComponents = new List<DrawableGameComponent>();
        List<GameComponent> _tempUpdateableComponents = new List<GameComponent>();
        List<GameComponent> _tempDrawableComponents = new List<GameComponent>();
        //Used to decouple the sorting from the actual
        //update of the sort order.  Necessary since
        //the sort order can be updated while iterating 
        //throw a list.  We don't want to change the 
        //list at that time.
        bool _updateSortOrder = false;

        internal void Refresh()
        {
            if (_updateSortOrder)
            {
                _drawableComponents.Sort(SortDrawOrder);
                _components.Sort(SortUpdateOrder);
                _updateSortOrder = false;
            }
        }

        static int SortDrawOrder(DrawableGameComponent c1, DrawableGameComponent c2)
        {
            if (c1.DrawOrder == c2.DrawOrder)
                return 0;

            return (c1.DrawOrder < c2.DrawOrder) ? -1 : 1;
        }

        static int SortUpdateOrder(GameComponent gc1, GameComponent gc2)
        {
            if (gc1.UpdateOrder == gc2.UpdateOrder)
                return 0;

            return (gc1.UpdateOrder < gc2.UpdateOrder) ? -1 : 1;
        }


        public int IndexOf(GameComponent item)
        {
            return _components.IndexOf(item);
        }

        public void Insert(int index, GameComponent item)
        {
            _components.Insert(index, item);
            item.UpdateOrderChanged += item_Updated;
            item.Initialize();
            DrawableGameComponent dgc = item as DrawableGameComponent;
            if (dgc != null)
            {
                _drawableComponents.Add(dgc);
                dgc.DrawOrderChanged += item_Updated;
            }

            _updateSortOrder = true;
        }

        //Need to decouple the changing of the sort order from 
        //the actual changes to the properties.
        void item_Updated(object sender, EventArgs e)
        {
            _updateSortOrder = true;
        }

        public void RemoveAt(int index)
        {
            if(index > _components.Count - 1)
                throw new IndexOutOfRangeException();

            DrawableGameComponent removedComponent = _components[index] as DrawableGameComponent;

            if(removedComponent != null)
                _drawableComponents.Remove(removedComponent);

            _components.RemoveAt(index);
        }

        public bool Remove(GameComponent item)
        {
            if (item is DrawableGameComponent)
                _drawableComponents.Remove(item as DrawableGameComponent);

            return _components.Remove(item);
        }

        public GameComponent this[int index]
        {
            get
            {
                return _components[index];
            }
            set
            {
                _components[index] = value;
            }
        }

        public void Add(GameComponent item)
        {
            _components.Add(item);
            item.UpdateOrderChanged += item_Updated;
            item.Initialize();
            DrawableGameComponent dgc = item as DrawableGameComponent;

            if (dgc != null)
            {
                dgc.DrawOrderChanged += item_Updated;
                _drawableComponents.Add(dgc);
            }

            _updateSortOrder = true;
            Refresh();
        }

        public void Clear()
        {
            _components.Clear();
            _drawableComponents.Clear();
        }

        public bool Contains(GameComponent item)
        {
            return _components.Contains(item);
        }

        public void CopyTo(GameComponent[] array, int arrayIndex)
        {
            _components.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _components.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Update(GameTime gameTime)
        {
            _tempUpdateableComponents.Clear();
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i].Enabled)
                {
                    _tempUpdateableComponents.Add(_components[i]);
                }
            }
            foreach (GameComponent gc in _tempUpdateableComponents)
            {
                gc.Update(gameTime);
            }
            
            Refresh();
        }

        public void Draw(GameTime gameTime)
        {
            _tempDrawableComponents.Clear();
            for (int i = 0; i < _drawableComponents.Count; i++)
            {
                if (_drawableComponents[i].Enabled && _drawableComponents[i].Visible)
                {
                    _tempDrawableComponents.Add(_drawableComponents[i]);
                }
            }
            foreach (DrawableGameComponent dc in _tempDrawableComponents)
            {
                dc.Draw(gameTime);
            }

            Refresh();
        }

        public void Initialize()
        {
            foreach (GameComponent gc in _components)
            {
                gc.Initialize();
            }

            Refresh();
        }

        public IEnumerator<GameComponent> GetEnumerator()
        {
            return _components.GetEnumerator();
        }


        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _components.GetEnumerator();
        }
    }
}
