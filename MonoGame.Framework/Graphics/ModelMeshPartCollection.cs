using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a collection of <see cref="ModelMeshPart"/> objects.
    /// </summary>
	public sealed class ModelMeshPartCollection : ReadOnlyCollection<ModelMeshPart>
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelMeshPartCollection"/>
        /// class that is a read-only wrapper around the specified list.
        /// </summary>
        /// <param name="list">The list to wrap.</param>
		public ModelMeshPartCollection(IList<ModelMeshPart> list)
			: base(list)
		{

		}
	}

	//// Summary:
	////     Represents a collection of ModelMeshPart objects.
	//public sealed class ModelMeshPartCollection : ReadOnlyCollection<ModelMeshPart>
	//{
	//    internal ModelMeshPartCollection()
	//        : base(new List<ModelMeshPart>())
	//    {
	//    }

	//    // Summary:
	//    //     Returns a ModelMeshPartCollection.Enumerator that can iterate through a ModelMeshPartCollection.
	//    public ModelMeshPartCollection.Enumerator GetEnumerator() { throw new NotImplementedException(); }

	//    // Summary:
	//    //     Provides the ability to iterate through the bones in an ModelMeshPartCollection.
	//    public struct Enumerator : IEnumerator<ModelMeshPart>, IDisposable, IEnumerator
	//    {

	//        // Summary:
	//        //     Gets the current element in the ModelMeshPartCollection.
	//        public ModelMeshPart Current { get { throw new NotImplementedException(); } }

	//        // Summary:
	//        //     Immediately releases the unmanaged resources used by this object.
	//        public void Dispose() { throw new NotImplementedException(); }
	//        //
	//        // Summary:
	//        //     Advances the enumerator to the next element of the ModelMeshPartCollection.
	//        public bool MoveNext() { throw new NotImplementedException(); }

	//        #region IEnumerator Members

	//        object IEnumerator.Current
	//        {
	//            get { throw new NotImplementedException(); }
	//        }

	//        public void Reset()
	//        {
	//            throw new NotImplementedException();
	//        }

	//        #endregion
	//    }
	//}
}
