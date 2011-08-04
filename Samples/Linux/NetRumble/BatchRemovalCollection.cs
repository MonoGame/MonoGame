#region File Description
//-----------------------------------------------------------------------------
// BatchRemovalCollection.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
#endregion

namespace NetRumble
{
    /// <summary>
    /// A collection with an additional feature to isolate the
    /// removal of the objects.
    /// </summary>
    public class BatchRemovalCollection<T> : List<T>
    {
        #region Pending Removal Data


        /// <summary>
        /// The list of objects to be removed on the next ApplyPendingRemovals().
        /// </summary>
        private List<T> pendingRemovals;


        #endregion


        #region Initialization


        /// <summary>
        /// Constructs a new collection.
        /// </summary>
        public BatchRemovalCollection()
        {
            pendingRemovals = new List<T>();
        }


        #endregion


        #region Collection


        /// <summary>
        /// Queue an item for removal.
        /// </summary>
        /// <param name="item"></param>
        public void QueuePendingRemoval(T item)
        {
            pendingRemovals.Add(item);
        }


        /// <summary>
        /// Remove all of the "garbage" objects from this collection.
        /// </summary>
        public void ApplyPendingRemovals()
        {
            for (int i = 0; i < pendingRemovals.Count; i++)
            {
                Remove(pendingRemovals[i]);
            }
            pendingRemovals.Clear();
        }


        #endregion
    }
}
