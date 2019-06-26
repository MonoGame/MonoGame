#region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
#endregion License 

using System;

namespace Microsoft.Xna.Framework.GamerServices
{
	public enum GamerPresenceMode
	{
		ArcadeMode, //Arcade Mode
		AtMenu, //At Menu
		BattlingBoss	, //Battling Boss
		CampaignMode, //	Campaign Mode
		ChallengeMode, //	Challenge Mode
		ConfiguringSettings, // // Configuring Settings
		CoOpLevel, //	Co-Op: Level. Includes a numeric value specified with PresenceValue.
		CoOpStage,// 	Co-Op: Stage. Includes a numeric value specified with PresenceValue.
		CornflowerBlue, //Cornflower Blue
		CustomizingPlayer, //Customizing Player
		DifficultyEasy, //Difficulty: Easy
		DifficultyExtreme, //Difficulty: Extreme
		DifficultyHard, //Difficulty: Hard
		DifficultyMedium	, // Difficulty: Medium
		EditingLevel	, // Editing Level
		ExplorationMode, //Exploration Mode
		FoundSecret, //Found Secret
		FreePlay	, // Free Play
		GameOver	, //Game Over
		InCombat, 	// In Combat
		InGameStore, //In Game Store
		Level, //Level. Includes a numeric value specified with PresenceValue.
		LocalCoOp, //Local Co-Op
		LocalVersus, //Local Versus
		LookingForGames, //Looking For Games
		Losing, //Losing
		Multiplayer, //Multiplayer
		NearlyFinished, // Nearly Finished
		None	, // NoPresence String Displayed
		OnARoll, // On a Roll
		OnlineCoOp, //Online Co-Op
		OnlineVersus, //Online Versus
		Outnumbered, //Outnumbered
		Paused, //Paused
		PlayingMinigame, //Playing Minigame
		PlayingWithFriends, //Playing With Friends
		PracticeMode	, //Practice Mode
		PuzzleMode, //Puzzle Mode
		ScenarioMode	, //Scenario Mode
		Score, //Score. Includes a numeric value specified with PresenceValue.
		ScoreIsTied, //Score is Tied
		SettingUpMatch, //Setting Up Match
		SinglePlayer, //	Single Player
		Stage, //Stage. Includes a numeric value specified with PresenceValue.
		StartingGame, //Starting Game
		StoryMode, //Story Mode
		StuckOnAHardBit, //	Stuck on a Hard Bit
		SurvivalMode	, //Survival Mode
		TimeAttack, //Time Attack
		TryingForRecord, //	Trying For Record
		TutorialMode	, //Tutorial Mode
		VersusComputer, //Versus Computer
		VersusScore, //	Versus: Score. Includes a numeric value specified with PresenceValue.
		WaitingForPlayers, //Waiting For Players
		WaitingInLobby, //Waiting In Lobby
		WastingTime, //	Wasting Time
		WatchingCredits, //	Watching Credits
		WatchingCutscene, //Watching Cutscene
		Winning, //	Winning
		WonTheGame, //	Won the Game
	}
}
