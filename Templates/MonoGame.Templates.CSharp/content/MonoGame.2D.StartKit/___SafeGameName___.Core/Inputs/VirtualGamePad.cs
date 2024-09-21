//-----------------------------------------------------------------------------
// VirtualGamePad.cs
//
// MonoGame Foundation Game Platform
// Copyright (C) MonoGame Foundation. All rights reserved.
//-----------------------------------------------------------------------------

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace ___SafeGameName___.Core.Inputs;

class VirtualGamePad
{
    private readonly Vector2 baseScreenSize;
    private Matrix globalTransformation;
    private readonly Texture2D texture;

    private float secondsSinceLastInput;
    private float opacity;

    public VirtualGamePad(Vector2 baseScreenSize, Matrix globalTransformation, Texture2D texture)
    {
        this.baseScreenSize = baseScreenSize;
        this.globalTransformation = Matrix.Invert(globalTransformation);
        this.texture = texture;
        secondsSinceLastInput = float.MaxValue;
    }

    public void NotifyPlayerIsMoving()
    {
        secondsSinceLastInput = 0;
    }

    public void Update(GameTime gameTime)
    {
        var secondsElapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
        secondsSinceLastInput += secondsElapsed;

        //If the player is moving, fade the controls out
        // otherwise, if they haven't moved in 4 seconds, fade the controls back in
        if (secondsSinceLastInput < 4)
            opacity = Math.Max(0, opacity - secondsElapsed * 4);
        else
            opacity = Math.Min(1, opacity + secondsElapsed * 2);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var spriteCenter = new Vector2(64, 64);
        var color = Color.Multiply(Color.White, opacity);

        spriteBatch.Draw(texture, new Vector2(64, baseScreenSize.Y - 64), null, color, -MathHelper.PiOver2, spriteCenter, 1, SpriteEffects.None, 0);
        spriteBatch.Draw(texture, new Vector2(192, baseScreenSize.Y - 64), null, color, MathHelper.PiOver2, spriteCenter, 1, SpriteEffects.None, 0);
        spriteBatch.Draw(texture, new Vector2(baseScreenSize.X - 128, baseScreenSize.Y - 128), null, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
    }

    /// <summary>
    /// Generates a GamePadState based on the touch input provided (as applied to the on screen controls) and the gamepad state
    /// </summary>
    public GamePadState GetState(TouchCollection touchState, GamePadState gpState)
    {
        //Work out what buttons are pressed based on the touchState
        Buttons buttonsPressed = 0;

        foreach (var touch in touchState)
        {
            if (touch.State == TouchLocationState.Moved || touch.State == TouchLocationState.Pressed)
            {
                //Scale the touch position to be in _baseScreenSize coordinates
                Vector2 pos = touch.Position;
                Vector2.Transform(ref pos, ref globalTransformation, out pos);

                if (pos.X < 128)
                    buttonsPressed |= Buttons.DPadLeft;
                else if (pos.X < 256)
                    buttonsPressed |= Buttons.DPadRight;
                else if (pos.X >= baseScreenSize.X - 128)
                    buttonsPressed |= Buttons.A;
            }
        }

        //Combine the buttons of the real gamepad
        var gpButtons = gpState.Buttons;
        buttonsPressed |= gpButtons.A == ButtonState.Pressed ? Buttons.A : 0;
        buttonsPressed |= gpButtons.B == ButtonState.Pressed ? Buttons.B : 0;
        buttonsPressed |= gpButtons.X == ButtonState.Pressed ? Buttons.X : 0;
        buttonsPressed |= gpButtons.Y == ButtonState.Pressed ? Buttons.Y : 0;

        buttonsPressed |= gpButtons.Start == ButtonState.Pressed ? Buttons.Start : 0;
        buttonsPressed |= gpButtons.Back == ButtonState.Pressed ? Buttons.Back : 0;

        buttonsPressed |= gpState.IsButtonDown(Buttons.DPadDown) ? Buttons.DPadDown : 0;
        buttonsPressed |= gpState.IsButtonDown(Buttons.DPadLeft) ? Buttons.DPadLeft : 0;
        buttonsPressed |= gpState.IsButtonDown(Buttons.DPadRight) ? Buttons.DPadRight : 0;
        buttonsPressed |= gpState.IsButtonDown(Buttons.DPadUp) ? Buttons.DPadUp : 0;

        buttonsPressed |= gpButtons.BigButton == ButtonState.Pressed ? Buttons.BigButton : 0;
        buttonsPressed |= gpButtons.LeftShoulder == ButtonState.Pressed ? Buttons.LeftShoulder : 0;
        buttonsPressed |= gpButtons.RightShoulder == ButtonState.Pressed ? Buttons.RightShoulder : 0;

        buttonsPressed |= gpButtons.LeftStick == ButtonState.Pressed ? Buttons.LeftStick : 0;
        buttonsPressed |= gpButtons.RightStick == ButtonState.Pressed ? Buttons.RightStick : 0;

        var buttons = new GamePadButtons(buttonsPressed);

        return new GamePadState(gpState.ThumbSticks, gpState.Triggers, buttons, gpState.DPad);
    }
}
