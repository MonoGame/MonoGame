using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Graphics
{
	// Summary:
	//     Represents a collection of effects associated with a model.
	public sealed class ModelEffectCollection : ReadOnlyCollection<Effect>
	{
		public ModelEffectCollection(IList<Effect> list)
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

	    // Summary:
	    //     Returns a ModelEffectCollection.Enumerator that can iterate through a ModelEffectCollection.
	    public new ModelEffectCollection.Enumerator GetEnumerator()
		{
			return new ModelEffectCollection.Enumerator((List<Effect>)Items);
		}

	    // Summary:
	    //     Provides the ability to iterate through the bones in an ModelEffectCollection.
	    public struct Enumerator : IEnumerator<Effect>, IDisposable, IEnumerator
	    {
			List<Effect>.Enumerator enumerator;
			internal Enumerator(List<Effect> list)
			{
				enumerator = list.GetEnumerator();
			}

	        // Summary:
	        //     Gets the current element in the ModelEffectCollection.
	        public Effect Current { get { return enumerator.Current; } }

	        // Summary:
	        //     Immediately releases the unmanaged resources used by this object.
	        public void Dispose() { enumerator.Dispose(); }
	        //
	        // Summary:
	        //     Advances the enumerator to the next element of the ModelEffectCollection.
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
