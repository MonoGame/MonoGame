// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework
{
    public sealed class GameComponentCollection : Collection<IGameComponent>
    {
        public event EventHandler<GameComponentCollectionEventArgs> ComponentAdded;

        public event EventHandler<GameComponentCollectionEventArgs> ComponentRemoved;

        protected override void ClearItems()
        {
            for (int i = 0; i < base.Count; i++)
            {
                this.OnComponentRemoved(new GameComponentCollectionEventArgs(base[i]));
            }
            base.ClearItems();
        }

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
            if (this.ComponentAdded != null)
            {
                this.ComponentAdded(this, eventArgs);
            }
        }

        private void OnComponentRemoved(GameComponentCollectionEventArgs eventArgs)
        {
            if (this.ComponentRemoved != null)
            {
                this.ComponentRemoved(this, eventArgs);
            }
        }

        protected override void RemoveItem(int index)
        {
            IGameComponent gameComponent = base[index];
            base.RemoveItem(index);
            if (gameComponent != null)
            {
                this.OnComponentRemoved(new GameComponentCollectionEventArgs(gameComponent));
            }
        }

        protected override void SetItem(int index, IGameComponent item)
        {
            throw new NotSupportedException();
        }
    }
}

