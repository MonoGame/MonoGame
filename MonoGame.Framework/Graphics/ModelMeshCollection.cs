using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Microsoft.Xna.Framework.Graphics
{
	// Summary:
	//     Represents a collection of ModelMesh objects.
	public sealed class ModelMeshCollection : ReadOnlyCollection<ModelMesh>
	{
		internal ModelMeshCollection(IList<ModelMesh> list)
			: base(list)
		{

		}
		
	    // Summary:
	    //     Retrieves a ModelMesh from the collection, given the name of the mesh.
	    //
	    // Parameters:
	    //   meshName:
	    //     The name of the mesh to retrieve.
		public ModelMesh this[string meshName] {
			get {
				ModelMesh ret;
				if (!this.TryGetValue(meshName, out ret)) {
					throw new KeyNotFoundException();
				}
				return ret;
			}
		}
		
	    // Summary:
	    //     Finds a mesh with a given name if it exists in the collection.
	    //
	    // Parameters:
	    //   meshName:
	    //     The name of the mesh to find.
	    //
	    //   value:
	    //     [OutAttribute] The mesh named meshName, if found.
	    public bool TryGetValue (string meshName, out ModelMesh value)
		{
			if (string.IsNullOrEmpty (meshName)) {
				throw new ArgumentNullException ("meshName");
			}
			
			foreach (var mesh in this) {
				if (string.Compare(mesh.Name, meshName, StringComparison.Ordinal) == 0) {
					value = mesh;
					return true;
				}
			}
			
			value = null;
			return false;
			throw new NotImplementedException();
		}

	}

	//// Summary:
	////     Represents a collection of ModelMesh objects.
	//public sealed class ModelMeshCollection : ReadOnlyCollection<ModelMesh>
	//{
	//    internal ModelMeshCollection()
	//        : base(new List<ModelMesh>())
	//    {
	//    }

	//    // Summary:
	//    //     Retrieves a ModelMesh from the collection, given the name of the mesh.
	//    //
	//    // Parameters:
	//    //   meshName:
	//    //     The name of the mesh to retrieve.
	//    public ModelMesh this[string meshName] { get { throw new NotImplementedException(); } }

	//    // Summary:
	//    //     Returns a ModelMeshCollection.Enumerator that can iterate through a ModelMeshCollection.
	//    public ModelMeshCollection.Enumerator GetEnumerator() { throw new NotImplementedException(); }
	//    //
	//    // Summary:
	//    //     Finds a mesh with a given name if it exists in the collection.
	//    //
	//    // Parameters:
	//    //   meshName:
	//    //     The name of the mesh to find.
	//    //
	//    //   value:
	//    //     [OutAttribute] The mesh named meshName, if found.
	//    public bool TryGetValue(string meshName, out ModelMesh value) { throw new NotImplementedException(); }

	//    // Summary:
	//    //     Provides the ability to iterate through the bones in an ModelMeshCollection.
	//    public struct Enumerator : IEnumerator<ModelMesh>, IDisposable, IEnumerator
	//    {

	//        // Summary:
	//        //     Gets the current element in the ModelMeshCollection.
	//        public ModelMesh Current { get { throw new NotImplementedException(); } }

	//        // Summary:
	//        //     Immediately releases the unmanaged resources used by this object.
	//        public void Dispose() { throw new NotImplementedException(); }
	//        //
	//        // Summary:
	//        //     Advances the enumerator to the next element of the ModelMeshCollection.
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
