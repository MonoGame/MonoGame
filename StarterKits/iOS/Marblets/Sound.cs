#region File Description
//-----------------------------------------------------------------------------
// Sound.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Marblets
{
	/// <summary>
	/// An enum for all of the Marblets sounds
	/// </summary>
	public enum SoundEntry
	{
		/// <summary>
		/// Title Screen music
		/// </summary>
		MusicTitle,
		/// <summary>
		/// In game music
		/// </summary>
		MusicGame,
		/// <summary>
		/// GameOver
		/// </summary>
		MusicGameOver,
		/// <summary>
		/// Board cleared
		/// </summary>
		MusicBoardCleared,
		/// <summary>
		/// Start 3d game
		/// </summary>
		Menu3DStart,
		/// <summary>
		/// Start 2d game
		/// </summary>
		Menu2DStart,
		/// <summary>
		/// Move cursor
		/// </summary>
		Navigate,
		/// <summary>
		/// Clear Marbles
		/// </summary>
		ClearMarbles,
		/// <summary>
		/// Illegal clear less than 2 marbles
		/// </summary>
		ClearIllegal,
		/// <summary>
		/// Bonus sound for large clear
		/// </summary>
		ClearBonus,
		/// <summary>
		/// Marbles landing after breaking
		/// </summary>
		LandMarbles,
	}

	/// <summary>
	/// Abstracts away the sounds for a simple interface using the Sounds enum
	/// </summary>
	public static class Sound
	{
		private static string[] cueNames = new string[]
        {
            "IntroMus", //Title Screen
            "MusLoop_Temp1", //In-Game Music
			"MusLoop_Temp1", //Game Over
            "MusLoop_Temp1", //Clear Board
            "start_3", //Menu: 3D select (button press)
            "start_3", //Menu: 2D select (button press)
            "navigate_1", //In-Game Cursor Move
            "clear_4", //Clear  marbles (Press A)
            "clear_illegal", //Illegal clear (press A w/<2 marbles selected)
            "clear_bonus", //Large break bonus
            "drop2", //Marbles impact sound (after fall)
        };

		const int SoundCount = 11;
		private static SoundEffect[] soundEffects = new SoundEffect[SoundCount];


		public static void Play(SoundEntry sound)
		{
			soundEffects[(int)sound].Play();
		}

		public static SoundEffectInstance PlayMusic(SoundEntry sound)
		{
			SoundEffectInstance instance = soundEffects[(int)sound].CreateInstance();
			instance.IsLooped = (sound == SoundEntry.MusicGame);
			instance.Play();
			return instance;
		}

		public static void StopMusic(SoundEffectInstance instance)
		{
			if(instance != null)
			{
				instance.Stop();
				instance.Dispose();
			}
		}

		public static void LoadContent(ContentManager content)
		{
			for(int i = 0; i < SoundCount; i++)
			{
				soundEffects[i] = content.Load<SoundEffect>("Audio/Wav/" + cueNames[i]);
			}
		}
	}
}
