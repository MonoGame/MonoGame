// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework
{
	public interface IUpdateable
	{
		#region Methods
		void Update(GameTime gameTime);
		#endregion
		
		#region Events
		event EventHandler<EventArgs> EnabledChanged;
		
		event EventHandler<EventArgs> UpdateOrderChanged;
		#endregion
	
		#region Properties
		bool Enabled { get; }
		
		int UpdateOrder { get; }
		#endregion
	}
}
