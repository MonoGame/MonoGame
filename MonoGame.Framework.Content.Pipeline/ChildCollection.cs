// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides a collection of child objects for a content item.
    /// 
    /// Links from a child object to its parent are maintained as the collection contents are modified.
    /// </summary>
    /// <typeparam name="TParent"></typeparam>
    /// <typeparam name="TChild"></typeparam>
    public abstract class ChildCollection<TParent, TChild> : Collection<TChild>
        where TParent : class
        where TChild : class
    {
        TParent parent;

        /// <summary>
        /// Creates an instance of ChildCollection.
        /// </summary>
        /// <param name="parent">Parent object of the child objects returned in the collection.</param>
        protected ChildCollection(TParent parent)
            : base()
        {
            if (parent == null)
                throw new ArgumentNullException("parent");
            this.parent = parent;
        }

        /// <summary>
        /// Removes all children from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            // Remove parent reference from each child before clearing
            foreach (TChild child in this)
                SetParent(child, default(TParent));
            base.ClearItems();
        }

        /// <summary>
        /// Gets the parent of a child object.
        /// </summary>
        /// <param name="child">The child of the parent being retrieved.</param>
        /// <returns>The parent of the child object.</returns>
        protected abstract TParent GetParent(TChild child);

        /// <summary>
        /// Inserts a child object into the collection at the specified location.
        /// </summary>
        /// <param name="index">The position in the collection.</param>
        /// <param name="item">The child object being inserted.</param>
        protected override void InsertItem(int index, TChild item)
        {
            // Make sure we have a 
            if (item == null)
                throw new ArgumentNullException("child");
            if (GetParent(item) != null)
                throw new InvalidOperationException("Child already has a parent");
            SetParent(item, parent);
            base.InsertItem(index, item);
        }

        /// <summary>
        /// Removes a child object from the collection.
        /// </summary>
        /// <param name="index">The index of the item being removed.</param>
        protected override void RemoveItem(int index)
        {
            TChild child = this[index];
            SetParent(child, default(TParent));
            base.RemoveItem(index);
        }

        /// <summary>
        /// Modifies the value of the child object at the specified location.
        /// </summary>
        /// <param name="index">The index of the child object being modified.</param>
        /// <param name="item">The new value for the child object.</param>
        protected override void SetItem(int index, TChild item)
        {
            if (item == null)
                throw new ArgumentNullException("child");
            if (GetParent(item) != null)
                throw new InvalidOperationException("Child already has a parent");
            TChild child = this[index];
            SetParent(child, default(TParent));
            SetParent(item, parent);
            base.SetItem(index, item);
        }

        /// <summary>
        /// Modifies the value of the parent object of the specified child object.
        /// </summary>
        /// <param name="child">The child of the parent being modified.</param>
        /// <param name="parent">The new value for the parent object.</param>
        protected abstract void SetParent(TChild child, TParent parent);
    }
}
