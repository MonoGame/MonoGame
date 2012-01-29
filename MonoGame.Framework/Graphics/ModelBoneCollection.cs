using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Microsoft.Xna.Framework.Graphics
{
	// Summary:
	//     Represents a set of bones associated with a model.
	public class ModelBoneCollection : ReadOnlyCollection<ModelBone>
	{
		public ModelBoneCollection(IList<ModelBone> list)
			: base(list)
		{

		}

	    // Summary:
	    //     Retrieves a ModelBone from the collection, given the name of the bone.
	    //
	    // Parameters:
	    //   boneName:
	    //     The name of the bone to retrieve.
	    public ModelBone this[string boneName]
		{
			get {
				ModelBone ret;
				if (TryGetValue(boneName, out ret)) {
					return ret;
				}
				throw new KeyNotFoundException();
			}
		}

	 //   // Summary:
	//    //     Returns a ModelBoneCollection.Enumerator that can iterate through a ModelBoneCollection.
	//    public ModelBoneCollection.Enumerator GetEnumerator() { throw new NotImplementedException(); }
	    //
	    // Summary:
	    //     Finds a bone with a given name if it exists in the collection.
	    //
	    // Parameters:
	    //   boneName:
	    //     The name of the bone to find.
	    //
	    //   value:
	    //     [OutAttribute] The bone named boneName, if found.
	    public bool TryGetValue(string boneName, out ModelBone value)
		{
			foreach (ModelBone bone in base.Items)
			{
				if (bone.Name == boneName) {
					value = bone;
					return true;
				}
			}
			value = null;
			return false;
		}

	//    // Summary:
	//    //     Provides the ability to iterate through the bones in an ModelBoneCollection.
	//    public struct Enumerator : IEnumerator<ModelBone>, IDisposable, IEnumerator
	//    {

	//        // Summary:
	//        //     Gets the current element in the ModelBoneCollection.
	//        public ModelBone Current { get { throw new NotImplementedException(); } }

	//        // Summary:
	//        //     Immediately releases the unmanaged resources used by this object.
	//        public void Dispose() { throw new NotImplementedException(); }
	//        //
	//        // Summary:
	//        //     Advances the enumerator to the next element of the ModelBoneCollection.
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
	}
}
