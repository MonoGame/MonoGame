#region File Description
//-----------------------------------------------------------------------------
// WeightedContentEntry.cs
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
    /// A description of a piece of content, quantity and weight for various purposes.
    /// </summary>
    public class WeightedContentEntry<T> : ContentEntry<T> where T : ContentObject
    {
        /// <summary>
        /// The weight of this content within the group, for statistical distribution.
        /// </summary>
        private int weight;

        /// <summary>
        /// The weight of this content within the group, for statistical distribution.
        /// </summary>
        public int Weight
        {
            get { return weight; }
            set { weight = value; }
        }


        #region Content Type Reader


        /// <summary>
        /// Reads a WeightedContentEntry object from the content pipeline.
        /// </summary>
        public class WeightedContentEntryReader : 
            ContentTypeReader<WeightedContentEntry<T>>
        {
            /// <summary>
            /// Reads a WeightedContentEntry object from the content pipeline.
            /// </summary>
            protected override WeightedContentEntry<T> Read(ContentReader input,
                WeightedContentEntry<T> existingInstance)
            {
                WeightedContentEntry<T> entry = existingInstance;
                if (entry == null)
                {
                    entry = new WeightedContentEntry<T>();
                }

                input.ReadRawObject<ContentEntry<T>>(entry as ContentEntry<T>);

                entry.Weight = input.ReadInt32();

                return entry;
            }
        }


        #endregion
    }
}
