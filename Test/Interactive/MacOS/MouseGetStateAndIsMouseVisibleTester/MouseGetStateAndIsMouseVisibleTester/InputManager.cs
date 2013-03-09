using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Media;
using System.IO;
using Microsoft.Xna.Framework.GamerServices;

namespace MouseGetStateAndIsMouseVisibleTester
{
    public class InputManager
    {
        private Game1 cG;
        private MouseState mousestatus;
        private MsState mssButtonLeft;
        public TimeSpan tsTimeSinceLastClick;
        private int nClicksForDoubleClick;


        public InputManager(Game game)
        {
            cG = (Game1)game;
            mssButtonLeft = MsState.ButtonWasReleased;
            tsTimeSinceLastClick = TimeSpan.Zero;
            nClicksForDoubleClick = 0;

        }//InputManger

        public void InputHandler(MouseState mst, GameTime gameTime)
        {
            if (!cG.IsActive) return;
            
            mousestatus = mst;

            HandleMouseLeftButton(gameTime);

            if (mssButtonLeft == MsState.ButtonWasPressed)
            {
                if (mousestatus.X >= 0 && mousestatus.X <= 405)
                {
                    if (mousestatus.Y >= 209 && mousestatus.Y <= 250)
                    {
                        cG.Toggle();
                    }//if
                }//if
            }//if
            
            if (mssButtonLeft == MsState.ButtonWasPressed)
            {
                if (mousestatus.X >= 0 && mousestatus.X <= 628)
                {
                    if (mousestatus.Y >= 304 && mousestatus.Y <= 350)
                    {
                        cG.CenterWindow();
                    }//if
                }//if
            }//if
        }//InputHandler

        private void HandleMouseLeftButton(GameTime gTime)
        {
            tsTimeSinceLastClick += gTime.ElapsedGameTime;

            if (tsTimeSinceLastClick >= TimeSpan.FromMilliseconds(250))
            {
                nClicksForDoubleClick = 0;
            }//if

            if (mousestatus.LeftButton == ButtonState.Pressed)
            {
                if (mssButtonLeft == MsState.ButtonWasReleased)
                {
                    if (GetMouseX() >= 0 && GetMouseX() <= cG.GetBackBufferWidth())
                    {
                        if (GetMouseY() >= 0 && GetMouseY() <= cG.GetBackBufferHeight())
                        {
                            mssButtonLeft = MsState.ButtonWasPressed;
                            nClicksForDoubleClick++;

                            if (nClicksForDoubleClick == 1)
                            {
                                tsTimeSinceLastClick = TimeSpan.Zero;
                            }//if

                            if (nClicksForDoubleClick == 2)
                            {
                                if (tsTimeSinceLastClick < TimeSpan.FromMilliseconds(250))
                                {
                                    nClicksForDoubleClick = 0;
                                    mssButtonLeft = MsState.ButtonWasDoublePressed;
                                }//if
                            }//if

                            if (nClicksForDoubleClick == 3) nClicksForDoubleClick = 0;

                        }//if
                    }//if
                }//if
                else
                {
                    if (mssButtonLeft == MsState.ButtonWasPressed || mssButtonLeft == MsState.ButtonWasDoublePressed)
                    {
                        mssButtonLeft = MsState.ButtonStillPressed;
                    }//if
                }//else
            }//if

            if (mousestatus.LeftButton == ButtonState.Released)
            {
                mssButtonLeft = MsState.ButtonWasReleased;
            }//if
        }//HandleMouseLeftButton

        public float GetMouseX()
        {
            return mousestatus.X;
        }//GetMouseX

        public float GetMouseY()
        {
            return mousestatus.Y;
        }//GetMouseY
    }
}
