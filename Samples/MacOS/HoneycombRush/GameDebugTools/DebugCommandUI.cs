#region File Description
//-----------------------------------------------------------------------------
// DebugCommandUI.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace HoneycombRush.GameDebugTools
{
    /// <summary>
    /// Command Window class for Debug purpose.
    /// </summary>
    /// <remarks>
    /// Debug command UI that runs in the Game.
    /// You can type commands using the keyboard, even on the Xbox
    /// just connect a USB keyboard to it
    /// This works on all 3 platforms (Xbox, Windows, Phone)
    /// 
    /// How to Use:
    /// 1) Add this component to the game.
    /// 2) Register command by RegisterCommand method.
    /// 3) Open/Close Debug window by Tab key.
    /// </remarks>
    public class DebugCommandUI : DrawableGameComponent, IDebugCommandHost
    {
        #region Constants

        /// <summary>
        /// Maximum lines that shows in Debug command window.
        /// </summary>
        const int MaxLineCount = 20;

        /// <summary>
        /// Maximum command history number.
        /// </summary>
        const int MaxCommandHistory = 32;

        /// <summary>
        /// Cursor character.
        /// </summary>
        const string Cursor = "_";

        /// <summary>
        /// Default Prompt string.
        /// </summary>
        public const string DefaultPrompt = "CMD>";

        #endregion

        #region Properties

        /// <summary>
        /// Gets/Sets Prompt string.
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// Is it waiting for key inputs?
        /// </summary>
        public bool Focused { get { return state != State.Closed; } }

        #endregion

        #region Fields

        // Command window states.
        enum State
        {
            Closed,
            Opening,
            Opened,
            Closing
        }

        /// <summary>
        /// CommandInfo class that contains information to run the command.
        /// </summary>
        class CommandInfo
        {
            public CommandInfo(
                string command, string description, DebugCommandExecute callback)
            {
                this.command = command;
                this.description = description;
                this.callback = callback;
            }

            // command name
            public string command;

            // Description of command.
            public string description;

            // delegate for execute the command.
            public DebugCommandExecute callback;
        }

        // Reference to DebugManager.
        private DebugManager debugManager;

        // Current state
        private State state = State.Closed;

        // timer for state transition.
        private float stateTransition;

        // Registered echo listeners.
        List<IDebugEchoListner> listenrs = new List<IDebugEchoListner>();

        // Registered command executioner.
        Stack<IDebugCommandExecutioner> executioners = new Stack<IDebugCommandExecutioner>();

        // Registered commands
        private Dictionary<string, CommandInfo> commandTable =
                                                new Dictionary<string, CommandInfo>();

        // Current command line string and cursor position.
        private string commandLine = String.Empty;
        private int cursorIndex = 0;

        private Queue<string> lines = new Queue<string>();

        // Command history buffer.
        private List<string> commandHistory = new List<string>();

        // Selecting command history index.
        private int commandHistoryIndex;

        #region variables for keyboard input handling.

        // Previous frame keyboard state.
        private KeyboardState prevKeyState;

        // Key that pressed last frame.
        private Keys pressedKey;

        // Timer for key repeating.
        private float keyRepeatTimer;

        // Key repeat duration in seconds for the first key press.
        private float keyRepeatStartDuration = 0.3f;

        // Key repeat duration in seconds after the first key press.
        private float keyRepeatDuration = 0.03f;

        #endregion

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public DebugCommandUI(Game game)
            : base(game)
        {
            Prompt = DefaultPrompt;

            // Add this instance as a service.
            Game.Services.AddService(typeof(IDebugCommandHost), this);            

            // Adding default commands.

            // Help command displays registered command information.
            RegisterCommand("help", "Show Command helps",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                int maxLen = 0;
                foreach (CommandInfo cmd in commandTable.Values)
                    maxLen = Math.Max(maxLen, cmd.command.Length);

                string fmt = String.Format("{{0,-{0}}}    {{1}}", maxLen);

                foreach (CommandInfo cmd in commandTable.Values)
                {
                    Echo(String.Format(fmt, cmd.command, cmd.description));
                }
            });

            // Clear screen command
            RegisterCommand("cls", "Clear Screen",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                lines.Clear();
            });

            // Echo command
            RegisterCommand("echo", "Display Messages",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                Echo(command.Substring(5));
            });
        }

        /// <summary>
        /// Initialize component
        /// </summary>
        public override void Initialize()
        {
            // Draw the command UI on top of everything
            DrawOrder = int.MaxValue;

            debugManager =
                Game.Services.GetService(typeof(DebugManager)) as DebugManager;

            if (debugManager == null)
                throw new InvalidOperationException("Coudn't find DebugManager.");

            base.Initialize();
        }

        #endregion

        #region IDebugCommandHostinterface implemenration

        public void RegisterCommand(
            string command, string description, DebugCommandExecute callback)
        {
            string lowerCommand = command.ToLower();
            if (commandTable.ContainsKey(lowerCommand))
            {
                throw new InvalidOperationException(
                    String.Format("Command \"{0}\" is already registered.", command));
            }

            commandTable.Add(
                lowerCommand, new CommandInfo(command, description, callback));
        }

        public void UnregisterCommand(string command)
        {
            string lowerCommand = command.ToLower();
            if (!commandTable.ContainsKey(lowerCommand))
            {
                throw new InvalidOperationException(
                    String.Format("Command \"{0}\" is not registered.", command));
            }

            commandTable.Remove(command);
        }

        public void ExecuteCommand(string command)
        {
            // Call registered executioner.
            if (executioners.Count != 0)
            {
                executioners.Peek().ExecuteCommand(command);
                return;
            }

            // Run the command.
            char[] spaceChars = new char[] { ' ' };

            Echo(Prompt + command);

            command = command.TrimStart(spaceChars);

            List<string> args = new List<string>(command.Split(spaceChars));
            string cmdText = args[0];
            args.RemoveAt(0);

            CommandInfo cmd;
            if (commandTable.TryGetValue(cmdText.ToLower(), out cmd))
            {
                try
                {
                    // Call registered command delegate.
                    cmd.callback(this, command, args);
                }
                catch (Exception e)
                {
                    // Exception occurred while running command.
                    EchoError("Unhandled Exception occurred");

                    string[] lines = e.Message.Split(new char[] { '\n' });
                    foreach (string line in lines)
                        EchoError(line);
                }
            }
            else
            {
                Echo("Unknown Command");
            }

            // Add to command history.
            commandHistory.Add(command);
            while (commandHistory.Count > MaxCommandHistory)
                commandHistory.RemoveAt(0);

            commandHistoryIndex = commandHistory.Count;
        }

        public void RegisterEchoListner(IDebugEchoListner listner)
        {
            listenrs.Add(listner);
        }

        public void UnregisterEchoListner(IDebugEchoListner listner)
        {
            listenrs.Remove(listner);
        }

        public void Echo(DebugCommandMessage messageType, string text)
        {
            lines.Enqueue(text);
            while (lines.Count >= MaxLineCount)
                lines.Dequeue();

            // Call registered listeners.
            foreach (IDebugEchoListner listner in listenrs)
                listner.Echo(messageType, text);
        }

        public void Echo(string text)
        {
            Echo(DebugCommandMessage.Standard, text);
        }

        public void EchoWarning(string text)
        {
            Echo(DebugCommandMessage.Warning, text);
        }

        public void EchoError(string text)
        {
            Echo(DebugCommandMessage.Error, text);
        }

        public void PushExecutioner(IDebugCommandExecutioner executioner)
        {
            executioners.Push(executioner);
        }

        public void PopExecutioner()
        {
            executioners.Pop();
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Show Debug Command window.
        /// </summary>
        public void Show()
        {
            if (state == State.Closed)
            {
                stateTransition = 0.0f;
                state = State.Opening;
            }
        }

        /// <summary>
        /// Hide Debug Command window.
        /// </summary>
        public void Hide()
        {
            if (state == State.Opened)
            {
                stateTransition = 1.0f;
                state = State.Closing;
            }
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            const float OpenSpeed = 8.0f;
            const float CloseSpeed = 8.0f;

            switch (state)
            {
                case State.Closed:
                    if (keyState.IsKeyDown(Keys.Tab))
                        Show();
                    break;
                case State.Opening:
                    stateTransition += dt * OpenSpeed;
                    if (stateTransition > 1.0f)
                    {
                        stateTransition = 1.0f;
                        state = State.Opened;
                    }
                    break;
                case State.Opened:
                    ProcessKeyInputs(dt);
                    break;
                case State.Closing:
                    stateTransition -= dt * CloseSpeed;
                    if (stateTransition < 0.0f)
                    {
                        stateTransition = 0.0f;
                        state = State.Closed;
                    }
                    break;
            }

            prevKeyState = keyState;

            base.Update(gameTime);
        }

        /// <summary>
        /// Hand keyboard input.
        /// </summary>
        /// <param name="dt"></param>
        public void ProcessKeyInputs(float dt)
        {
            KeyboardState keyState = Keyboard.GetState();
            Keys[] keys = keyState.GetPressedKeys();

            bool shift = keyState.IsKeyDown(Keys.LeftShift) ||
                            keyState.IsKeyDown(Keys.RightShift);

            foreach (Keys key in keys)
            {
                if (!IsKeyPressed(key, dt)) continue;

                char ch;
                if (KeyboardUtils.KeyToString(key, shift, out ch))
                {
                    // Handle typical character input.
                    commandLine = commandLine.Insert(cursorIndex, new string(ch, 1));
                    cursorIndex++;
                }
                else
                {
                    switch (key)
                    {
                        case Keys.Back:
                            if (cursorIndex > 0)
                                commandLine = commandLine.Remove(--cursorIndex, 1);
                            break;
                        case Keys.Delete:
                            if (cursorIndex < commandLine.Length)
                                commandLine = commandLine.Remove(cursorIndex, 1);
                            break;
                        case Keys.Left:
                            if (cursorIndex > 0)
                                cursorIndex--;
                            break;
                        case Keys.Right:
                            if (cursorIndex < commandLine.Length)
                                cursorIndex++;
                            break;
                        case Keys.Enter:
                            // Run the command.
                            ExecuteCommand(commandLine);
                            commandLine = string.Empty;
                            cursorIndex = 0;
                            break;
                        case Keys.Up:
                            // Show command history.
                            if (commandHistory.Count > 0)
                            {
                                commandHistoryIndex =
                                    Math.Max(0, commandHistoryIndex - 1);

                                commandLine = commandHistory[commandHistoryIndex];
                                cursorIndex = commandLine.Length;
                            }
                            break;
                        case Keys.Down:
                            // Show command history.
                            if (commandHistory.Count > 0)
                            {
                                commandHistoryIndex = Math.Min(commandHistory.Count - 1,
                                                                commandHistoryIndex + 1);
                                commandLine = commandHistory[commandHistoryIndex];
                                cursorIndex = commandLine.Length;
                            }
                            break;
                        case Keys.Tab:
                            Hide();
                            break;
                    }
                }
            }

        }

        /// <summary>
        /// Pressing check with key repeating.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsKeyPressed(Keys key, float dt)
        {
            // Treat it as pressed if given key has not pressed in previous frame.
            if (prevKeyState.IsKeyUp(key))
            {
                keyRepeatTimer = keyRepeatStartDuration;
                pressedKey = key;
                return true;
            }

            // Handling key repeating if given key has pressed in previous frame.
            if (key == pressedKey)
            {
                keyRepeatTimer -= dt;
                if (keyRepeatTimer <= 0.0f)
                {
                    keyRepeatTimer += keyRepeatDuration;
                    return true;
                }
            }

            return false;
        }

        public override void Draw(GameTime gameTime)
        {
            // Do nothing when command window is completely closed.
            if (state == State.Closed)
                return;

            SpriteFont font = debugManager.DebugFont;
            SpriteBatch spriteBatch = debugManager.SpriteBatch;
            Texture2D whiteTexture = debugManager.WhiteTexture;

            // Compute command window size and draw.
            float w = GraphicsDevice.Viewport.Width;
            float h = GraphicsDevice.Viewport.Height;
            float topMargin = h * 0.1f;
            float leftMargin = w * 0.1f;

            Rectangle rect = new Rectangle();
            rect.X = (int)leftMargin;
            rect.Y = (int)topMargin;
            rect.Width = (int)(w * 0.8f);
            rect.Height = (int)(MaxLineCount * font.LineSpacing);

            Matrix mtx = Matrix.CreateTranslation(
                        new Vector3(0, -rect.Height * (1.0f - stateTransition), 0));

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, mtx);

            spriteBatch.Draw(whiteTexture, rect, new Color(0, 0, 0, 200));

            // Draw each lines.
            Vector2 pos = new Vector2(leftMargin, topMargin);
            foreach (string line in lines)
            {
                spriteBatch.DrawString(font, line, pos, Color.White);
                pos.Y += font.LineSpacing;
            }

            // Draw prompt string.
            string leftPart = Prompt + commandLine.Substring(0, cursorIndex);
            Vector2 cursorPos = pos + font.MeasureString(leftPart);
            cursorPos.Y = pos.Y;

            spriteBatch.DrawString(font,
                String.Format("{0}{1}", Prompt, commandLine), pos, Color.White);
            spriteBatch.DrawString(font, Cursor, cursorPos, Color.White);

            spriteBatch.End();
        }

        #endregion

    }
}
