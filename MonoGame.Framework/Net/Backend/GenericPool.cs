using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Net.Backend
{
    internal interface IResetable
    {
        void Reset();
    }

    internal class GenericPool<T> where T : IResetable, new()
    {
        private IList<T> freeMessages = new List<T>();

        public T Get()
        {
            T item;

            if (freeMessages.Count > 0)
            {
                int lastIndex = freeMessages.Count - 1;
                item = freeMessages[lastIndex];
                freeMessages.RemoveAt(lastIndex);
            }
            else
            {
                item = new T();
                item.Reset();
            }

            return item;
        }

        public void Recycle(T item)
        {
            item.Reset();
            freeMessages.Add(item);
        }
    }
}
