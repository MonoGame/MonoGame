#region File Description
//-----------------------------------------------------------------------------
// ContentEntry.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework.Content;
using System.Xml.Serialization;
#endregion

namespace RolePlayingGameData
{
    /// <summary>
    /// A description of a piece of content and quantity for various purposes.
    /// </summary>
    public class ContentEntry<T> where T : ContentObject 
    {
        /// <summary>
        /// The content name for the content involved.
        /// </summary>
        private string contentName;

        /// <summary>
        /// The content name for the content involved.
        /// </summary>
        [ContentSerializer(Optional=true)]
        public string ContentName
        {
            get { return contentName; }
            set { contentName = value; }
        }


        /// <summary>
        /// The content referred to by this entry.
        /// </summary>
        /// <remarks>
        /// This will not be automatically loaded, as the content path may be incomplete.
        /// </remarks>
        private T content;

        /// <summary>
        /// The content referred to by this entry.
        /// </summary>
        /// <remarks>
        /// This will not be automatically loaded, as the content path may be incomplete.
        /// </remarks>
        [ContentSerializerIgnore]
        [XmlIgnore]
        public T Content
        {
            get { return content; }
            set { content = value; }
        }
        
        
        /// <summary>
        /// The quantity of this content.
        /// </summary>
        private int count = 1;

        /// <summary>
        /// The quantity of this content.
        /// </summary>
        [ContentSerializer(Optional=true)]
        public int Count
        {
            get { return count; }
            set { count = value; }
        }


        #region Content Type Reader


        /// <summary>
        /// Reads a ContentEntry object from the content pipeline.
        /// </summary>
        public class ContentEntryReader : ContentTypeReader<ContentEntry<T>>
        {
            /// <summary>
            /// Reads a ContentEntry object from the content pipeline.
            /// </summary>
            protected override ContentEntry<T> Read(ContentReader input,
                ContentEntry<T> existingInstance)
            {
                ContentEntry<T> member = existingInstance;
                if (member == null)
                {
                    member = new ContentEntry<T>();
                }

                member.ContentName = input.ReadString();
                member.Count = input.ReadInt32();

                return member;
            }
        }


        #endregion
    }
}
