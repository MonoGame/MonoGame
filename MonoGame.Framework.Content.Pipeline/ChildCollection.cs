#region License
/*
 Microsoft Public License (Ms-PL)
 MonoGame - Copyright © 2012 The MonoGame Team
 
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
            base.InsertItem(index, item);
        }

        /// <summary>
        /// Modifies the value of the parent object of the specified child object.
        /// </summary>
        /// <param name="child">The child of the parent being modified.</param>
        /// <param name="parent">The new value for the parent object.</param>
        protected abstract void SetParent(TChild child, TParent parent);
    }
}
