#region File Description
//-----------------------------------------------------------------------------
// QuestNpcScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using RolePlayingGameData;
#endregion

namespace RolePlaying
{
    class QuestNpcScreen : NpcScreen<QuestNpc>
    {
        /// <summary>
        /// Constructs a new QuestNpcScreen object.
        /// </summary>
        /// <param name="mapEntry">The map entry for the quest NPC.</param>
        public QuestNpcScreen(MapEntry<QuestNpc> mapEntry)
            : base(mapEntry)
        {
            // assign and check the parameter
            QuestNpc questNpc = character as QuestNpc;
            if (questNpc == null)
            {
                throw new ArgumentException(
                    "QuestNpcScreen requires a MapEntry with a QuestNpc");
            }

            // check to see if this is NPC is the current quest destination
            if ((Session.Quest != null) && 
                (Session.Quest.Stage == Quest.QuestStage.RequirementsMet) &&
                TileEngine.Map.AssetName.EndsWith(
                    Session.Quest.DestinationMapContentName) &&
                (Session.Quest.DestinationNpcContentName == mapEntry.ContentName))
            {
                // use the quest completion dialog
                this.DialogueText = Session.Quest.CompletionMessage;
                // mark the quest for completion
                // -- the session will not update until the pop-up screens are cleared
                Session.Quest.Stage = Quest.QuestStage.Completed;
            }
            else
            {
                // this NPC is not the destination, so use the npc's welcome text
                this.DialogueText = questNpc.IntroductionDialogue;
            }
        }
    }
}