// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
//
// Author: Kenneth James Pouncey
//
using System;

namespace Microsoft.Xna.Framework.Content
{
	// http://msdn.microsoft.com/en-us/library/bb195465.aspx
	// The class definition on msdn site shows: [AttributeUsageAttribute(384)]
	// The following code var ff = (AttributeTargets)384; shows that ff is Field | Property
	//  so that is what we use.
    /// <summary>
    /// Defines a custom <see cref="Attribute"/> that marks a field or property to indicate that it should
    /// not be included in serialization.
    /// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class ContentSerializerIgnoreAttribute : Attribute
	{
	}

}

