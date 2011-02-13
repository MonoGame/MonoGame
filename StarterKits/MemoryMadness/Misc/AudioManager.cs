#region File Description

//-----------------------------------------------------------------------------
// AudioManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

#endregion

namespace MemoryMadness
{
    /// <summary>
    /// Component that manages audio playback for all sounds.
    /// </summary>
    public class AudioManager : GameComponent
    {
        #region Fields

        #region Singleton

        /// <summary>
        /// The singleton for this type.
        /// </summary>
        static AudioManager audioManager = null;
        public static AudioManager Instance
        {
            get { return audioManager; }
        }

        public static bool IsInitialized
        {
            get { return audioManager.isInitialized; }
        }

        #endregion

        #region Audio Data

        SoundEffectInstance musicSound;
        Dictionary<string, SoundEffectInstance> soundBank;
        string[,] soundNames;

        #endregion

        bool isInitialized;

        #endregion

        #region Initialization

        private AudioManager(Game game)
            : base(game) { }

        /// <summary>
        /// Initialize the static AudioManager functionality.
        /// </summary>
        /// <param name="game">The game that this component will be attached to.</param>
        public static void Initialize(Game game)
        {
            audioManager = new AudioManager(game);

            game.Components.Add(audioManager);
        }

        #endregion

        #region Loading Methodes

        /// <summary>
        /// Loads a sounds and organizes them for future usage
        /// </summary>
        public static void LoadSounds()
        {
            string soundLocation = "Sounds/";

            audioManager.soundNames = new string[,] 
            { 
                {"RedButton", "red"},
                {"GreenButton", "green"},
                {"BlueButton", "blue"},
                {"YellowButton", "yellow"},
                {"HighScoreScreen", "doorOpen"},
                {"LevelComplete", "success"},
                {"DefeatBuzzer", "fail"}
            };

            audioManager.soundBank = new Dictionary<string, SoundEffectInstance>();

            for (int i = 0; i < audioManager.soundNames.GetLength(0); i++)
            {
                SoundEffect se = audioManager.Game.Content.Load<SoundEffect>(
                    soundLocation + audioManager.soundNames[i, 0]);
                audioManager.soundBank.Add(
                    audioManager.soundNames[i, 1], se.CreateInstance());
            }

            audioManager.isInitialized = true;
        }

        #endregion

        #region Sound Methods

        /// <summary>
        /// Indexer. Return a sound instance by name
        /// </summary>
        public SoundEffectInstance this[string soundName]
        {
            get
            {
                if (audioManager.soundBank.ContainsKey(soundName))
                    return audioManager.soundBank[soundName];
                else
                    return null;
            }
        }

        /// <summary>
        /// Plays a sound by name.
        /// </summary>
        /// <param name="soundName">The name of the sound to play</param>
        public static void PlaySound(string soundName)
        {
            // If the sound exists, start it
            if (audioManager.soundBank.ContainsKey(soundName))
                audioManager.soundBank[soundName].Play();
        }

        /// <summary>
        /// Plays a sound by name.
        /// </summary>
        /// <param name="soundName">The name of the sound to play</param>
        /// <param name="isLooped">Indicates if the sound should loop</param>
        public static void PlaySound(string soundName, bool isLooped)
        {
            // If the sound exists, start it
            if (audioManager.soundBank.ContainsKey(soundName))
            {
                if (audioManager.soundBank[soundName].IsLooped != isLooped)
                    audioManager.soundBank[soundName].IsLooped = isLooped;

                audioManager.soundBank[soundName].Play();
            }
        }


        /// <summary>
        /// Stops a sound mid-play. If the sound is not playing, this
        /// method does nothing.
        /// </summary>
        /// <param name="soundName">The name of the sound to stop</param>
        public static void StopSound(string soundName)
        {
            // If the sound exists, stop it
            if (audioManager.soundBank.ContainsKey(soundName))
                audioManager.soundBank[soundName].Stop();
        }

        /// <summary>
        /// Stops all currently playing sounds.
        /// </summary>
        public static void StopSounds()
        {
            foreach (var sound in audioManager.soundBank.Values)
            {
                if (sound.State != SoundState.Stopped)
                {
                    sound.Stop();
                }
            }            
        }

        /// <summary>
        /// Checks whether or not sounds are currently playing.
        /// </summary>
        /// <returns>True if some sounds are playing, false otherwise.</returns>
        public static bool AreSoundsPlaying()
        {
            foreach (var sound in audioManager.soundBank.Values)
            {
                if (sound.State == SoundState.Playing)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Pause or resume all sounds.
        /// </summary>
        /// <param name="resumeSounds">True to resume all paused sounds or false
        /// to pause all playing sounds.</param>
        public static void PauseResumeSounds(bool resumeSounds)
        {
            SoundState state = resumeSounds ? SoundState.Paused : SoundState.Playing;

            foreach (var sound in audioManager.soundBank.Values)
            {
                if (sound.State == state)
                {
                    if (resumeSounds)
                    {
                        sound.Resume();
                    }
                    else
                    {
                        sound.Pause();
                    }
                }
            }            
        }
        /// <summary>
        /// Play music by sound name.
        /// </summary>
        /// <param name="musicSoundName">The name of the music sound.</param>
        public static void PlayMusic(string musicSoundName)
        {
            // Stop the old music sound
            if (audioManager.musicSound != null)
                audioManager.musicSound.Stop(true);

            // If the music sound exists
            if (audioManager.soundBank.ContainsKey(musicSoundName))
            {
                // Get the instance and start it
                audioManager.musicSound = audioManager.soundBank[musicSoundName];
                if (!audioManager.musicSound.IsLooped)
                    audioManager.musicSound.IsLooped = true;
                audioManager.musicSound.Play();
            }
        }

        #endregion

        #region Instance Disposal Methods

        /// <summary>
        /// Clean up the component when it is disposing.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    foreach (var item in soundBank)
                    {
                        item.Value.Dispose();
                    }
                    soundBank.Clear();
                    soundBank = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        #endregion
    }
}
