#region File Description
//-----------------------------------------------------------------------------
// DebugSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

/*
 * To get started with the GameDebugTools, go to your main game class, override the Initialize method and add the
 * following line of code:
 * 
 * GameDebugTools.DebugSystem.Initialize(this, "MyFont");
 * 
 * where "MyFont" is the name a SpriteFont in your content project. This method will initialize all of the debug
 * tools and add the necessary components to your game. To begin instrumenting your game, add the following line of 
 * code to the top of your Update method:
 *
 * GameDebugTools.DebugSystem.Instance.TimeRuler.StartFrame()
 * 
 * Once you have that in place, you can add markers throughout your game by surrounding your code with BeginMark and
 * EndMark calls of the TimeRuler. For example:
 * 
 * GameDebugTools.DebugSystem.Instance.TimeRuler.BeginMark("SomeCode", Color.Blue);
 * // Your code goes here
 * GameDebugTools.DebugSystem.Instance.TimeRuler.EndMark("SomeCode");
 * 
 * Then you can display these results by setting the Visible property of the TimeRuler to true. This will give you a
 * visual display you can use to profile your game for optimizations.
 *
 * The GameDebugTools also come with an FpsCounter and a DebugCommandUI, which allows you to type commands at runtime
 * and toggle the various displays as well as registering your own commands that enable you to alter your game without
 * having to restart.
 */

#region Using Statements
using Microsoft.Xna.Framework;

namespace HoneycombRush.GameDebugTools
{
    /// <summary>
    /// DebugSystem is a helper class that streamlines the creation of the various GameDebug
    /// pieces. While games are free to add only the pieces they care about, DebugSystem allows
    /// games to quickly create and add all the components by calling the Initialize method.
    /// </summary>
    public class DebugSystem
    {
        private static DebugSystem singletonInstance;

        /// <summary>
        /// Gets the singleton instance of the debug system. You must call Initialize
        /// to create the instance.
        /// </summary>
        public static DebugSystem Instance
        {
            get { return singletonInstance; }
        }

        /// <summary>
        /// Gets the DebugManager for the system.
        /// </summary>
        public DebugManager DebugManager { get; private set; }

        /// <summary>
        /// Gets the DebugCommandUI for the system.
        /// </summary>
        public DebugCommandUI DebugCommandUI { get; private set; }

        /// <summary>
        /// Gets the FpsCounter for the system.
        /// </summary>
        public FpsCounter FpsCounter { get; private set; }

        /// <summary>
        /// Gets the TimeRuler for the system.
        /// </summary>
        public TimeRuler TimeRuler { get; private set; }

#if !WINDOWS_PHONE
        /// <summary>
        /// Gets the RemoteDebugCommand for the system.
        /// </summary>
        public RemoteDebugCommand RemoteDebugCommand { get; private set; }
#endif

        /// <summary>
        /// Initializes the DebugSystem and adds all components to the game's Components collection.
        /// </summary>
        /// <param name="game">The game using the DebugSystem.</param>

#endregion


        /// <param name="debugFont">The font to use by the DebugSystem.</param>
        /// <returns>The DebugSystem for the game to use.</returns>
        public static DebugSystem Initialize(Game game, string debugFont)
        {
            // if the singleton exists, return that; we don't want two systems being created for a game
            if (singletonInstance != null)
                return singletonInstance;

            // Create the system
            singletonInstance = new DebugSystem();

            // Create all of the system components
            singletonInstance.DebugManager = new DebugManager(game, debugFont);
            game.Components.Add(singletonInstance.DebugManager);

            singletonInstance.DebugCommandUI = new DebugCommandUI(game);
            game.Components.Add(singletonInstance.DebugCommandUI);

            singletonInstance.FpsCounter = new FpsCounter(game);
            game.Components.Add(singletonInstance.FpsCounter);

            singletonInstance.TimeRuler = new TimeRuler(game);
            game.Components.Add(singletonInstance.TimeRuler);

#if !WINDOWS_PHONE
            singletonInstance.RemoteDebugCommand = new RemoteDebugCommand(game);
            game.Components.Add(singletonInstance.RemoteDebugCommand);
#endif

            return singletonInstance;
        }

        // Private constructor; games should use Initialize
        private DebugSystem() { }
    }
}
