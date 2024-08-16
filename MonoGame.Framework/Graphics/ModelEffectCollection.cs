using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a collection of effects associated with a model.
    /// </summary>
    public sealed class ModelEffectCollection : ReadOnlyCollection<Effect>
	{
		internal ModelEffectCollection(IList<Effect> list)
			: base(list)
		{

		}

	    internal ModelEffectCollection() : base(new List<Effect>())
	    {
	    }
		
		//ModelMeshPart needs to be able to add to ModelMesh's effects list
		internal void Add(Effect item)
		{
			Items.Add (item);
		}
		internal void Remove(Effect item)
		{
			Items.Remove (item);
		}

        /// <summary>
        /// Returns a <see cref="ModelEffectCollection.Enumerator">ModelEffectCollection.Enumerator</see>
        /// that can iterate through a collection.
        /// </summary>
        public new ModelEffectCollection.Enumerator GetEnumerator()
		{
			return new ModelEffectCollection.Enumerator((List<Effect>)Items);
		}

        /// <summary>
        /// Enumerator to iterate through the <see cref="ModelEffectCollection"/>
        /// </summary>
        public struct Enumerator : IEnumerator<Effect>, IDisposable, IEnumerator
	    {
			List<Effect>.Enumerator enumerator;
            bool disposed;

			internal Enumerator(List<Effect> list)
			{
				enumerator = list.GetEnumerator();
                disposed = false;
			}

	        /// <inheritdoc/>
            public Effect Current { get { return enumerator.Current; } }

	        /// <inheritdoc cref="IDisposable.Dispose()"/>
	        public void Dispose()
            {
                if (!disposed)
                {
                    enumerator.Dispose();
                    disposed = true;
                }
            }

	        /// <inheritdoc/>
	        public bool MoveNext() { return enumerator.MoveNext(); }

	        #region IEnumerator Members

	        object IEnumerator.Current
	        {
	            get { return Current; }
	        }

	        void IEnumerator.Reset()
	        {
				IEnumerator resetEnumerator = enumerator;
				resetEnumerator.Reset ();
				enumerator = (List<Effect>.Enumerator)resetEnumerator;
	        }

	        #endregion
	    }
	}
}
