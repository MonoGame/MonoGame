using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Graphics
{
	[Obsolete ("ModelEffect is currently a stub")]
	public class ModelEffect : BasicEffect {
		private ModelEffect (GraphicsDevice device)
			: base (device)
		{
		}
	}

	[Obsolete ("ModelMeshPart is currently a stub")]
	public class ModelMeshPart { }

	[Obsolete ("ModelMesh is currently a stub")]
	public sealed class ModelMesh {
		private ModelMesh()
		{
			throw new NotImplementedException ();
		}

		[Obsolete ("BoundingSphere is currently a stub")]
		public BoundingSphere BoundingSphere { get { throw new NotImplementedException (); } }

		// FIXME: ModelEffectCollection
		[Obsolete ("Effects is currently a stub")]
		public ICollection<ModelEffect> Effects { get { throw new NotImplementedException (); } }

		// FIXME: ModelMeshPartCollection
		[Obsolete ("MeshParts is currently a stub")]
		public ICollection<ModelMeshPart> MeshParts { get { throw new NotImplementedException (); } }

		[Obsolete ("Name is currently a stub")]
		public string Name { get { throw new NotImplementedException (); } }

		[Obsolete ("ParentBone is currently a stub")]
		public ModelBone ParentBone { get { throw new NotImplementedException (); } }

		public object Tag { get; set; }

		[Obsolete ("Draw is currently a stub")]
		public void Draw()
		{
			throw new NotImplementedException ();
		}
	}
}
