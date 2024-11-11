using ___SafeGameName___.Core;
using ___SafeGameName___.Core.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace GameStateManagement.Inputs;

/// <summary>
/// Helper for reading input from keyboard, gamepad, and touch input. This class 
/// tracks both the current and previous state of the input devices, and implements 
/// query methods for high level input actions such as "move up through the menu"
/// or "pause the game".
/// </summary>
public class InputState
{
    public const int MaxInputs = 4;

    public AccelerometerState CurrentAccelerometerState;
    public readonly GamePadState[] CurrentGamePadStates;
    public readonly KeyboardState[] CurrentKeyboardStates;
    public MouseState CurrentMouseState;
    public TouchCollection CurrentTouchState;

    public AccelerometerState LastAccelerometerState;
    public readonly GamePadState[] LastGamePadStates;
    public readonly KeyboardState[] LastKeyboardStates;
    public MouseState LastMouseState;
    public TouchCollection LastTouchState;

    public readonly bool[] GamePadWasConnected;

    public readonly List<GestureSample> Gestures = new List<GestureSample>();

    /// <summary>
    /// Cursor move speed in pixels per second
    /// </summary>
    private const float cursorMoveSpeed = 250.0f;

    private Vector2 currentCursorLocation;
    /// <summary>
    /// Current location of our Cursor
    /// </summary>
    public Vector2 CurrentCursorLocation => currentCursorLocation;

    private bool isMouseWheelScrolledDown;
    /// <summary>
    /// Has the user scrolled the mouse wheel down?
    /// </summary>
    public bool IsMouseWheelScrolledDown => isMouseWheelScrolledDown;

    private bool isMouseWheelScrolledUp;
    /// <summary>
    /// Has the user scrolled the mouse wheel up?
    /// </summary>
    public bool IsMouseWheelScrolledUp => isMouseWheelScrolledUp;

    private float horizontalScalingFactor = 1.0f;
    private float verticalScalingFactor = 1.0f;

    /// <summary>
    /// Constructs a new input state.
    /// </summary>
    public InputState()
    {
        CurrentKeyboardStates = new KeyboardState[MaxInputs];
        CurrentGamePadStates = new GamePadState[MaxInputs];

        LastKeyboardStates = new KeyboardState[MaxInputs];
        LastGamePadStates = new GamePadState[MaxInputs];

        GamePadWasConnected = new bool[MaxInputs];

        if (___SafeGameName___Game.IsMobile)
        {
            TouchPanel.EnabledGestures = GestureType.Tap;
        }
        else if (___SafeGameName___Game.IsDesktop)
        {

        }
        else
        {
            // For now, we'll throw an exception if we don't know the platform
            throw new PlatformNotSupportedException();
        }
    }

    /// <summary>
    /// Reads the latest state of all the inputs.
    /// </summary>
    public void Update(GameTime gameTime, Viewport viewport)
    {
        CurrentAccelerometerState = Accelerometer.GetState();

        for (int i = 0; i < MaxInputs; i++)
        {
            LastKeyboardStates[i] = CurrentKeyboardStates[i];
            LastGamePadStates[i] = CurrentGamePadStates[i];

            CurrentKeyboardStates[i] = Keyboard.GetState();
            CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);

            // Keep track of whether a gamepad has ever been
            // connected, so we can detect if it is unplugged.
            if (CurrentGamePadStates[i].IsConnected)
            {
                GamePadWasConnected[i] = true;
            }
        }

        LastMouseState = CurrentMouseState;
        CurrentMouseState = Mouse.GetState();

        int touchCount = 0;
        LastTouchState = CurrentTouchState;
        CurrentTouchState = TouchPanel.GetState();

        Gestures.Clear();
        while (TouchPanel.IsGestureAvailable)
        {
            Gestures.Add(TouchPanel.ReadGesture());
        }

        foreach (TouchLocation location in CurrentTouchState)
        {
            switch (location.State)
            {
                case TouchLocationState.Pressed:
                    touchCount++;
                    currentCursorLocation.X = location.Position.X / horizontalScalingFactor;
                    currentCursorLocation.Y = location.Position.Y / verticalScalingFactor;
                    break;
                case TouchLocationState.Moved:
                    break;
                case TouchLocationState.Released:
                    break;
            }
        }

        if (CurrentMouseState.LeftButton == ButtonState.Released && LastMouseState.LeftButton == ButtonState.Pressed)
        {
            currentCursorLocation.X = CurrentMouseState.X / horizontalScalingFactor;
            currentCursorLocation.Y = CurrentMouseState.Y / verticalScalingFactor;
            touchCount = 1;
        }

        if (CurrentMouseState.MiddleButton == ButtonState.Released && LastMouseState.MiddleButton == ButtonState.Pressed)
        {
            touchCount = 2;
        }

        if (CurrentMouseState.RightButton == ButtonState.Released && LastMouseState.RightButton == ButtonState.Pressed)
        {
            touchCount = 3;
        }

        isMouseWheelScrolledUp = false;
        isMouseWheelScrolledDown = false;

        if (CurrentMouseState.ScrollWheelValue != LastMouseState.ScrollWheelValue)
        {
            int scrollWheelDelta = CurrentMouseState.ScrollWheelValue - LastMouseState.ScrollWheelValue;

            // Handle the scroll wheel event based on the delta
            if (scrollWheelDelta > 0)
            {
                // Mouse wheel scrolled down
                isMouseWheelScrolledDown = true;
            }
            else if (scrollWheelDelta < 0)
            {
                // Mouse wheel scrolled up
                isMouseWheelScrolledUp = true;
            }
        }

        // Update the cursor location by listening for left thumbstick input on
        // the 1st GamePad and direction key input on the Keyboard, making sure to
        // keep the cursor inside the screen boundary
        float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        currentCursorLocation.X += CurrentGamePadStates[0].ThumbSticks.Left.X * elapsedTime * cursorMoveSpeed;
        currentCursorLocation.Y -= CurrentGamePadStates[0].ThumbSticks.Left.Y * elapsedTime * cursorMoveSpeed;

        if (CurrentKeyboardStates[0].IsKeyDown(Keys.Up))
        {
            currentCursorLocation.Y -= elapsedTime * cursorMoveSpeed;
        }
        if (CurrentKeyboardStates[0].IsKeyDown(Keys.Down))
        {
            currentCursorLocation.Y += elapsedTime * cursorMoveSpeed;
        }
        if (CurrentKeyboardStates[0].IsKeyDown(Keys.Left))
        {
            currentCursorLocation.X -= elapsedTime * cursorMoveSpeed;
        }
        if (CurrentKeyboardStates[0].IsKeyDown(Keys.Right))
        {
            currentCursorLocation.X += elapsedTime * cursorMoveSpeed;
        }

        currentCursorLocation.X = MathHelper.Clamp(currentCursorLocation.X, 0f, viewport.Width);
        currentCursorLocation.Y = MathHelper.Clamp(currentCursorLocation.Y, 0f, viewport.Height);
    }


    /// <summary>
    /// Helper for checking if a key was newly pressed during this update. The
    /// controllingPlayer parameter specifies which player to read input for.
    /// If this is null, it will accept input from any player. When a keypress
    /// is detected, the output playerIndex reports which player pressed it.
    /// </summary>
    public bool IsNewKeyPress(Keys key, PlayerIndex? controllingPlayer,
                                        out PlayerIndex playerIndex)
    {
        if (controllingPlayer.HasValue)
        {
            // Read input from the specified player.
            playerIndex = controllingPlayer.Value;

            int i = (int)playerIndex;

            return (CurrentKeyboardStates[i].IsKeyDown(key) &&
                    LastKeyboardStates[i].IsKeyUp(key));
        }
        else
        {
            // Accept input from any player.
            return (IsNewKeyPress(key, PlayerIndex.One, out playerIndex) ||
                    IsNewKeyPress(key, PlayerIndex.Two, out playerIndex) ||
                    IsNewKeyPress(key, PlayerIndex.Three, out playerIndex) ||
                    IsNewKeyPress(key, PlayerIndex.Four, out playerIndex));
        }
    }


    /// <summary>
    /// Helper for checking if a button was newly pressed during this update.
    /// The controllingPlayer parameter specifies which player to read input for.
    /// If this is null, it will accept input from any player. When a button press
    /// is detected, the output playerIndex reports which player pressed it.
    /// </summary>
    public bool IsNewButtonPress(Buttons button, PlayerIndex? controllingPlayer,
                                                 out PlayerIndex playerIndex)
    {
        if (controllingPlayer.HasValue)
        {
            // Read input from the specified player.
            playerIndex = controllingPlayer.Value;

            int i = (int)playerIndex;

            return (CurrentGamePadStates[i].IsButtonDown(button) &&
                    LastGamePadStates[i].IsButtonUp(button));
        }
        else
        {
            // Accept input from any player.
            return (IsNewButtonPress(button, PlayerIndex.One, out playerIndex) ||
                    IsNewButtonPress(button, PlayerIndex.Two, out playerIndex) ||
                    IsNewButtonPress(button, PlayerIndex.Three, out playerIndex) ||
                    IsNewButtonPress(button, PlayerIndex.Four, out playerIndex));
        }
    }


    /// <summary>
    /// Checks for a "menu select" input action.
    /// The controllingPlayer parameter specifies which player to read input for.
    /// If this is null, it will accept input from any player. When the action
    /// is detected, the output playerIndex reports which player pressed it.
    /// </summary>
    public bool IsMenuSelect(PlayerIndex? controllingPlayer,
                             out PlayerIndex playerIndex)
    {
        return IsNewKeyPress(Keys.Space, controllingPlayer, out playerIndex) ||
               IsNewKeyPress(Keys.Enter, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.A, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
    }


    /// <summary>
    /// Checks for a "menu cancel" input action.
    /// The controllingPlayer parameter specifies which player to read input for.
    /// If this is null, it will accept input from any player. When the action
    /// is detected, the output playerIndex reports which player pressed it.
    /// </summary>
    public bool IsMenuCancel(PlayerIndex? controllingPlayer,
                             out PlayerIndex playerIndex)
    {
        return IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.B, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex);
    }


    /// <summary>
    /// Checks for a "menu up" input action.
    /// The controllingPlayer parameter specifies which player to read
    /// input for. If this is null, it will accept input from any player.
    /// </summary>
    public bool IsMenuUp(PlayerIndex? controllingPlayer)
    {
        PlayerIndex playerIndex;

        return IsNewKeyPress(Keys.Up, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.DPadUp, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.LeftThumbstickUp, controllingPlayer, out playerIndex) ||
               IsMouseWheelScrolledUp;
    }


    /// <summary>
    /// Checks for a "menu down" input action.
    /// The controllingPlayer parameter specifies which player to read
    /// input for. If this is null, it will accept input from any player.
    /// </summary>
    public bool IsMenuDown(PlayerIndex? controllingPlayer)
    {
        PlayerIndex playerIndex;

        return IsNewKeyPress(Keys.Down, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.DPadDown, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.LeftThumbstickDown, controllingPlayer, out playerIndex) ||
               IsMouseWheelScrolledDown;
    }


    /// <summary>
    /// Checks for a "pause the game" input action.
    /// The controllingPlayer parameter specifies which player to read
    /// input for. If this is null, it will accept input from any player.
    /// </summary>
    public bool IsPauseGame(PlayerIndex? controllingPlayer)
    {
        PlayerIndex playerIndex;

        return IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex) ||
               IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
    }

    internal void UpdateScalingFactor(float horizontalScalingFactor, float verticalScalingFactor)
    {
        this.horizontalScalingFactor = horizontalScalingFactor;
        this.verticalScalingFactor = verticalScalingFactor;
    }
}
