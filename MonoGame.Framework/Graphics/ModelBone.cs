using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
	[Obsolete ("ModelBone is currently a stub")]
	public sealed class ModelBone
	{
		// FIXME: ModelBoneCollection
		[Obsolete ("Children is currently a stub")]
		public ICollection<ModelBone> Children { get { throw new NotImplementedException (); } }

		[Obsolete ("Index is currently a stub")]
		public int Index { get { throw new NotImplementedException (); } }

		[Obsolete ("Name is currently a stub")]
		public string Name { get { throw new NotImplementedException (); } }

		[Obsolete ("Parent is currently a stub")]
		public ModelBone Parent { get { throw new NotImplementedException (); } }

		[Obsolete ("Transform is currently a stub")]
		public Matrix Transform 
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}
