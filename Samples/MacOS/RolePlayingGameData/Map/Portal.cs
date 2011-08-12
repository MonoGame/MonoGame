#region File Description
//-----------------------------------------------------------------------------
// Portal.cs
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
    /// A transition point from one map to another.  
    /// </summary>
    public class Portal : ContentObject
    {
        #region Description


        /// <summary>
        /// The name of the object.
        /// </summary>
        private string name;

        /// <summary>
        /// The name of the object.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }


        #endregion


        #region Landing Data


        /// <summary>
        /// The map coordinate that the party will automatically walk to 
        /// after spawning on this portal.
        /// </summary>
        private Point landingMapPosition;

        /// <summary>
        /// The map coordinate that the party will automatically walk to 
        /// after spawning on this portal.
        /// </summary>
        public Point LandingMapPosition
        {
            get { return landingMapPosition; }
            set { landingMapPosition = value; }
        }


        #endregion


        #region Destination Map Data


        /// <summary>
        /// The content name of the map that the portal links to.
        /// </summary>
        private string destinationMapContentName;

        /// <summary>
        /// The content name of the map that the portal links to.
        /// </summary>
        public string DestinationMapContentName
        {
            get { return destinationMapContentName; }
            set { destinationMapContentName = value; }
        }


        /// <summary>
        /// The name of the portal that the party spawns at on the destination map.
        /// </summary>
        private string destinationMapPortalName;

        /// <summary>
        /// The name of the portal that the party spawns at on the destination map.
        /// </summary>
        public string DestinationMapPortalName
        {
            get { return destinationMapPortalName; }
            set { destinationMapPortalName = value; }
        }


        #endregion


        #region Content Type Reader


        /// <summary>
        /// Reads a Portal object from the content pipeline.
        /// </summary>
        public class PortalReader : ContentTypeReader<Portal>
        {
            protected override Portal Read(ContentReader input, 
                Portal existingInstance)
            {
                Portal portal = existingInstance;
                if (portal == null)
                {
                    portal = new Portal();
                }

                portal.AssetName = input.AssetName;

                portal.Name = input.ReadString();

                portal.LandingMapPosition = input.ReadObject<Point>();
                portal.DestinationMapContentName = input.ReadString();
                portal.DestinationMapPortalName = input.ReadString();

                return portal;
            }
        }


        #endregion
    }
}
