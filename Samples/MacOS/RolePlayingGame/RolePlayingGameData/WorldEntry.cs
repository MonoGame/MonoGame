#region File Description
//-----------------------------------------------------------------------------
// WorldEntry.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#endregion

namespace RolePlayingGameData
{
    /// <summary>
    /// A description of a piece of content, including the name of the map it's on.
    /// </summary>
    public class WorldEntry<T> : MapEntry<T> where T : ContentObject
    {
        /// <summary>
        /// The name of the map where the content is added.
        /// </summary>
        private string mapContentName;

        /// <summary>
        /// The name of the map where the content is added.
        /// </summary>
        public string MapContentName
        {
            get { return mapContentName; }
            set { mapContentName = value; }
        }


        #region Content Type Reader


        /// <summary>
        /// Reads a WorldEntry object from the content pipeline.
        /// </summary>
        public class WorldEntryReader : ContentTypeReader<WorldEntry<T>>
        {
            /// <summary>
            /// Reads a WorldEntry object from the content pipeline.
            /// </summary>
            protected override WorldEntry<T> Read(ContentReader input,
                WorldEntry<T> existingInstance)
            {
                WorldEntry<T> desc = existingInstance;
                if (desc == null)
                {
                    desc = new WorldEntry<T>();
                }

                input.ReadRawObject<MapEntry<T>>(desc as MapEntry<T>);
                desc.MapContentName = input.ReadString();

                return desc;
            }
        }


        #endregion
    }
}
