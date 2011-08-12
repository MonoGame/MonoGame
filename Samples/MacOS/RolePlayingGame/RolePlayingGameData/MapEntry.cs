#region File Description
//-----------------------------------------------------------------------------
// MapEntry.cs
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
    /// The description of where an instance of a world object is in the world.
    /// </summary>
    public class MapEntry<T> : ContentEntry<T> where T : ContentObject
    {
        #region Map Data


        /// <summary>
        /// The position of this object on the map.
        /// </summary>
        private Point mapPosition;

        /// <summary>
        /// The position of this object on the map.
        /// </summary>
        public Point MapPosition
        {
            get { return mapPosition; }
            set { mapPosition = value; }
        }


        /// <summary>
        /// The orientation of this object on the map.
        /// </summary>
        private Direction direction;

        /// <summary>
        /// The orientation of this object on the map.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Direction Direction
        {
            get { return direction; }
            set { direction = value; }
        }


        #endregion


        #region Object Implementation


        /// <summary>
        /// Tests for equality between reference objects.
        /// </summary>
        /// <remarks>
        /// Implemented so that player-removed map entries from save games can be 
        /// compared to the data-driven map entries.
        /// </remarks>
        /// <returns>True if "equal".</returns>
        public override bool Equals(object obj)
        {
            MapEntry<T> mapEntry = obj as MapEntry<T>;
            return ((mapEntry != null) &&
                (mapEntry.Content == Content) &&
                (mapEntry.mapPosition == mapPosition) &&
                (mapEntry.Direction == Direction));
        }


        /// <summary>
        /// Calculates the hash code for comparisons with this reference type.
        /// </summary>
        /// <remarks>Recommended specified overload when Equals is overridden.</remarks>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        #endregion


        #region Graphics Data


        /// <summary>
        /// The animating sprite for the map view.
        /// </summary>
        /// <remarks>
        /// Only used when there might be several of the same WorldObject 
        /// in the scene at once.
        /// </remarks>
        private AnimatingSprite mapSprite;

        /// <summary>
        /// The animating sprite for the map view.
        /// </summary>
        /// <remarks>
        /// Only used when there might be several of the same WorldObject 
        /// in the scene at once.
        /// </remarks>
        [ContentSerializer(Optional = true)]
        public AnimatingSprite MapSprite
        {
            get { return mapSprite; }
            set { mapSprite = value; }
        }


        #endregion


        #region Content Type Reader


        /// <summary>
        /// Read a MapEntry object from the content pipeline.
        /// </summary>
        public class MapEntryReader : ContentTypeReader<MapEntry<T>>
        {
            /// <summary>
            /// Read a MapEntry object from the content pipeline.
            /// </summary>
            protected override MapEntry<T> Read(ContentReader input,
                MapEntry<T> existingInstance)
            {
                MapEntry<T> desc = existingInstance;
                if (desc == null)
                {
                    desc = new MapEntry<T>();
                }

                input.ReadRawObject<ContentEntry<T>>(desc as ContentEntry<T>);
                desc.MapPosition = input.ReadObject<Point>();
                desc.Direction = (Direction)input.ReadInt32();

                return desc;
            }
        }


        #endregion
    }
}
