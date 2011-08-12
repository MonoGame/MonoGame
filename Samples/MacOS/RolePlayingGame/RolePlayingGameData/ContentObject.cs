#region File Description
//-----------------------------------------------------------------------------
// ContentObject.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework.Content;
#endregion

namespace RolePlayingGameData
{
    /// <summary>
    /// Base type for all of the data types that load via the content pipeline.
    /// </summary>
    public abstract class ContentObject
    {
        /// <summary>
        /// The name of the content pipeline asset that contained this object.
        /// </summary>
        private string assetName;

        /// <summary>
        /// The name of the content pipeline asset that contained this object.
        /// </summary>
        [ContentSerializerIgnore]
        public string AssetName
        {
            get { return assetName; }
            set { assetName = value; }
        }
    }
}
