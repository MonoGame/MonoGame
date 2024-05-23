// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// A collection of <see cref="IGameComponent"/> instances.
    /// </summary>
    public sealed class GameComponentCollection : Collection<IGameComponent>
    {
        /// <summary>
        /// Event that is triggered when a <see cref="GameComponent"/> is added
        /// to this <see cref="GameComponentCollection"/>.
        /// </summary>
        public event EventHandler<GameComponentCollectionEventArgs> ComponentAdded;

        /// <summary>
        /// Event that is triggered when a <see cref="GameComponent"/> is removed
        /// from this <see cref="GameComponentCollection"/>.
        /// </summary>
        public event EventHandler<GameComponentCollectionEventArgs> ComponentRemoved;

        /// <summary>
        /// Removes every <see cref="GameComponent"/> from this <see cref="GameComponentCollection"/>.
        /// Triggers <see cref="ComponentRemoved"/> once for each <see cref="GameComponent"/> removed.
        /// </summary>
        protected override void ClearItems()
        {
            for (int i = 0; i < base.Count; i++)
            {
                this.OnComponentRemoved(new GameComponentCollectionEventArgs(base[i]));
            }
            base.ClearItems();
        }

        /// <summary>
        /// Inserts an element into the collection at the specified index.
        /// Triggers <see cref="ComponentAdded"/>.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        /// <exception cref="ArgumentException">
        /// Specified <see cref="IGameComponent"/> is already present in the collection.
        /// </exception>
        protected override void InsertItem(int index, IGameComponent item)
        {
            if (base.IndexOf(item) != -1)
            {
                throw new ArgumentException("Cannot Add Same Component Multiple Times");
            }
            base.InsertItem(index, item);
            if (item != null)
            {
                this.OnComponentAdded(new GameComponentCollectionEventArgs(item));
            }
        }

        private void OnComponentAdded(GameComponentCollectionEventArgs eventArgs)
        {
            EventHelpers.Raise(this, ComponentAdded, eventArgs);
        }

        private void OnComponentRemoved(GameComponentCollectionEventArgs eventArgs)
        {
            EventHelpers.Raise(this, ComponentRemoved, eventArgs);
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="GameComponentCollection"/>.
        /// Triggers <see cref="ComponentRemoved"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        protected override void RemoveItem(int index)
        {
            IGameComponent gameComponent = base[index];
            base.RemoveItem(index);
            if (gameComponent != null)
            {
                this.OnComponentRemoved(new GameComponentCollectionEventArgs(gameComponent));
            }
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <remarks>
        /// This method is not supported and will always throw <see cref="NotSupportedException"/>
        /// </remarks>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index.</param>
        /// <exception cref="NotSupportedException">Thrown always</exception>
        protected override void SetItem(int index, IGameComponent item)
        {
            throw new NotSupportedException();
        }
    }
}

