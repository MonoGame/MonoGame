#region File Description
//-----------------------------------------------------------------------------
// GameStartDescription.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
#endregion

namespace RolePlayingGameData
{
    /// <summary>
    /// The data needed to start a new game.
    /// </summary>
    public class GameStartDescription
    {
        #region Map


        /// <summary>
        /// The content name of the  map for a new game.
        /// </summary>
        private string mapContentName;

        /// <summary>
        /// The content name of the  map for a new game.
        /// </summary>
        public string MapContentName
        {
            get { return mapContentName; }
            set { mapContentName = value; }
        }


        #endregion


        #region Party


        /// <summary>
        /// The content names of the players in the party from the beginning.
        /// </summary>
        private List<string> playerContentNames = new List<string>();

        /// <summary>
        /// The content names of the players in the party from the beginning.
        /// </summary>
        public List<string> PlayerContentNames
        {
            get { return playerContentNames; }
            set { playerContentNames = value; }
        }
       

        #endregion


        #region Quest Line


        /// <summary>
        /// The quest line in action when the game starts.
        /// </summary>
        /// <remarks>The first quest will be started before the world is shown.</remarks>
        private string questLineContentName;

        /// <summary>
        /// The quest line in action when the game starts.
        /// </summary>
        /// <remarks>The first quest will be started before the world is shown.</remarks>
        [ContentSerializer(Optional = true)]
        public string QuestLineContentName
        {
            get { return questLineContentName; }
            set { questLineContentName = value; }
        }


        #endregion


        #region Content Type Reader


        /// <summary>
        /// Read a GameStartDescription object from the content pipeline.
        /// </summary>
        public class GameStartDescriptionReader : ContentTypeReader<GameStartDescription>
        {
            protected override GameStartDescription Read(ContentReader input, 
                GameStartDescription existingInstance)
            {
                GameStartDescription desc = existingInstance;
                if (desc == null)
                {
                    desc = new GameStartDescription();
                }

                desc.MapContentName = input.ReadString();
                desc.PlayerContentNames.AddRange(input.ReadObject<List<string>>());
                desc.QuestLineContentName = input.ReadString();

                return desc;
            }
        }


        #endregion
    }
}
