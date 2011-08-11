#region File Description
//-----------------------------------------------------------------------------
// SaveGameDescription.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Xml.Serialization;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// The description of a save game file.
    /// </summary>
    /// <remarks>
    /// This data is saved in a separate file, and loaded by 
    /// the Load and Save Game menu screens.
    /// </remarks>
#if WINDOWS
    [Serializable]
#endif
    public class SaveGameDescription
    {
        /// <summary>
        /// The name of the save file with the game data. 
        /// </summary>
        public string FileName;

        /// <summary>
        /// The short description of how far the player has progressed in the game.
        /// </summary>
        /// <remarks>Here, it's the current quest.</remarks>
        public string ChapterName;

        /// <summary>
        /// The short description of how far the player has progressed in the game.
        /// </summary>
        /// <remarks>Here, it's the time played.</remarks>
        public string Description;
    }
}
