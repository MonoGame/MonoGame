using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;


namespace Microsoft.Xna.Framework.Graphics
{
	[Obsolete ("Model is currently a stub")]
	public class Model {
		// FIXME: ModelBoneCollection
		[Obsolete ("Bones is currently a stub")]
		public ICollection<ModelBone> Bones { get { throw new NotImplementedException (); } }

		// FIXME: ModelMeshCollection
		[Obsolete ("Meshes is currently a stub")]
		public ICollection<ModelMesh> Meshes { get { throw new NotImplementedException (); } }

		[Obsolete ("Root is currently a stub")]
		public ModelBone Root { get { throw new NotImplementedException (); } }

		public object Tag { get; set; }

		private Model (GraphicsDevice graphicsDevice, List<ModelBone> bones, List<ModelMesh> meshes)
		{
			throw new NotImplementedException ();
		}

		[Obsolete ("Draw is currently a stub")]
		public void Draw (Matrix world, Matrix view, Matrix projection)
		{
			throw new NotImplementedException ();
		}

		[Obsolete ("CopyAbsoluteBoneTransformsTo is currently a stub")]
		public void CopyAbsoluteBoneTransformsTo (Matrix [] destinationBoneTransforms)
		{
			throw new NotImplementedException ();
		}

		[Obsolete ("CopyBoneTransformsFrom is currently a stub")]
		public void CopyBoneTransformsFrom (Matrix [] sourceBoneTransforms)
		{
			throw new NotImplementedException ();
		}

		[Obsolete ("CopyBoneTransformsTo is currently a stub")]
		public void CopyBoneTransformsTo (Matrix [] destinationBoneTransforms)
		{
			throw new NotImplementedException ();
		}
	}
}
