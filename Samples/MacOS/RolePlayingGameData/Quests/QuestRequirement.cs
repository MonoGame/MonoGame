#region File Description
//-----------------------------------------------------------------------------
// QuestRequirement.cs
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
    /// A requirement for a particular number of a piece of content.
    /// </summary>
    /// <remarks>Used to track gear acquired and monsters killed.</remarks>
    public class QuestRequirement<T> : ContentEntry<T> where T : ContentObject
    {
        /// <summary>
        /// The quantity of the content entry that has been acquired.
        /// </summary>
        private int completedCount;

        /// <summary>
        /// The quantity of the content entry that has been acquired.
        /// </summary>
        [ContentSerializerIgnore]
        public int CompletedCount
        {
            get { return completedCount; }
            set { completedCount = value; }
        }


        #region Content Type Reader


        /// <summary>
        /// Reads a QuestRequirement object from the content pipeline.
        /// </summary>
        public class QuestRequirementReader : ContentTypeReader<QuestRequirement<T>>
        {
            /// <summary>
            /// Reads a QuestRequirement object from the content pipeline.
            /// </summary>
            protected override QuestRequirement<T> Read(ContentReader input,
                QuestRequirement<T> existingInstance)
            {
                QuestRequirement<T> requirement = existingInstance;
                if (requirement == null)
                {
                    requirement = new QuestRequirement<T>();
                }

                input.ReadRawObject<ContentEntry<T>>(requirement as ContentEntry<T>);
                if (typeof(T) == typeof(Gear))
                {
                    requirement.Content = input.ContentManager.Load<T>(
                        System.IO.Path.Combine("Gear", requirement.ContentName));
                }
                else if (typeof(T) == typeof(Monster))
                {
                    requirement.Content = input.ContentManager.Load<T>(
                        System.IO.Path.Combine(@"Characters\Monsters", 
                        requirement.ContentName));
                }

                return requirement;
            }
        }


        #endregion
    }
}
