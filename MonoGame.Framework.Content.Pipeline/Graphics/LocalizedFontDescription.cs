#region File Description
//-----------------------------------------------------------------------------
// LocalizedFontDescription.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
#endregion

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Normally, when you add a .spritefont file to your project, this data is
    /// deserialized into a FontDescription object, which is then built into a
    /// SpriteFontContent by the FontDescriptionProcessor. But to localize the
    /// font, we want to add some additional data, so our custom processor can
    /// know what .resx files it needs to scan. We do this by defining our own
    /// custom font description class, deriving from the built in FontDescription
    /// type, and adding a new property to store the resource filenames.
    /// </summary>
    public class LocalizedFontDescription : FontDescription
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public LocalizedFontDescription()
            : base("Arial", 14, 0)
        {
        }


        /// <summary>
        /// Add a new property to our font description, which will allow us to
        /// include a ResourceFiles element in the .spritefont XML. We use the
        /// ContentSerializer attribute to mark this as optional, so existing
        /// .spritefont files that do not include this ResourceFiles element
        /// can be imported as well.
        /// </summary>
        [ContentSerializer(Optional = true, CollectionItemName = "Resx")]
        public List<string> ResourceFiles
        {
            get { return resourceFiles; }
        }

        List<string> resourceFiles = new List<string>();
    }
}
