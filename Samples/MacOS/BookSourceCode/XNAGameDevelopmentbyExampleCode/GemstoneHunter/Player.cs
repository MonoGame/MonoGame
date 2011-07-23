using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Tile_Engine;

namespace Gemstone_Hunter
{
    public class Player : GameObject
    {
        private Vector2 fallSpeed = new Vector2(0, 20);
        private float moveScale = 180.0f;
        private bool dead = false;
        private int score = 0;
        private int livesRemaining = 3;

        public bool Dead
        {
            get { return dead; }
        }

        public int Score
        {
            get { return score; }
            set { score = value; }
        }

        public int LivesRemaining
        {
            get { return livesRemaining; }
            set { livesRemaining = value; }
        }


        #region Constructor
        public Player(ContentManager content)
        {
            animations.Add("idle",
                new AnimationStrip(
                    content.Load<Texture2D>(@"Textures\Sprites\Player\Idle"),
                    48,
                    "idle"));
            animations["idle"].LoopAnimation = true;

            animations.Add("run",
                new AnimationStrip(
                    content.Load<Texture2D>(@"Textures\Sprites\Player\Run"),
                    48,
                    "run"));
            animations["run"].LoopAnimation = true;

            animations.Add("jump",
                new AnimationStrip(
                    content.Load<Texture2D>(@"Textures\Sprites\Player\Jump"),
                    48,
                    "jump"));
            animations["jump"].LoopAnimation = false;
            animations["jump"].FrameLength = 0.08f;
            animations["jump"].NextAnimation = "idle";

            animations.Add("die",
                new AnimationStrip(
                    content.Load<Texture2D>(@"Textures\Sprites\Player\Die"),
                    48,
                    "die"));
            animations["die"].LoopAnimation = false;

            frameWidth = 48;
            frameHeight = 48;
            CollisionRectangle = new Rectangle(9, 1, 30, 46);

            drawDepth = 0.825f;

            enabled = true;
            codeBasedBlocks = false;
            PlayAnimation("idle");
        }
        #endregion

        #region Public Methods
        public override void Update(GameTime gameTime)
        {
            if (!Dead)
            {
                string newAnimation = "idle";

                velocity = new Vector2(0, velocity.Y);
                GamePadState gamePad = GamePad.GetState(PlayerIndex.One);
                KeyboardState keyState = Keyboard.GetState();

                if (keyState.IsKeyDown(Keys.Left) ||
                    (gamePad.ThumbSticks.Left.X < -0.3f))
                {
                    flipped = false;
                    newAnimation = "run";
                    velocity = new Vector2(-moveScale, velocity.Y);
                }

                if (keyState.IsKeyDown(Keys.Right) ||
                    (gamePad.ThumbSticks.Left.X > 0.3f))
                {
                    flipped = true;
                    newAnimation = "run";
                    velocity = new Vector2(moveScale, velocity.Y);
                }

                if (keyState.IsKeyDown(Keys.Space) ||
                    (gamePad.Buttons.A == ButtonState.Pressed))
                {
                    if (onGround)
                    {
                        Jump();
                        newAnimation = "jump";
                    }
                }

                if (keyState.IsKeyDown(Keys.Up) ||
                    gamePad.ThumbSticks.Left.Y > 0.3f)
                {
                    checkLevelTransition();
                }


                if (currentAnimation == "jump")
                    newAnimation = "jump";

                if (newAnimation != currentAnimation)
                {
                    PlayAnimation(newAnimation);
                }
            }

            velocity += fallSpeed;

            repositionCamera();
            base.Update(gameTime);
        }

        public void Jump()
        {
            velocity.Y = -500;
        }

        public void Kill()
        {
            PlayAnimation("die");
            LivesRemaining--;
            velocity.X = 0;
            dead = true;
        }

        public void Revive()
        {
            PlayAnimation("idle");
            dead = false;
        }

        #endregion

        #region Helper Methods
        private void repositionCamera()
        {
            int screenLocX = (int)Camera.WorldToScreen(worldLocation).X;

            if (screenLocX > 500)
            {
                Camera.Move(new Vector2(screenLocX - 500, 0));
            }

            if (screenLocX < 200)
            {
                Camera.Move(new Vector2(screenLocX - 200, 0));
            }
        }

        private void checkLevelTransition()
        {
            Vector2 centerCell = TileMap.GetCellByPixel(WorldCenter);
            if (TileMap.CellCodeValue(centerCell).StartsWith("T_"))
            {
                string[] code = TileMap.CellCodeValue(centerCell).Split('_');

                if (code.Length != 4)
                    return;

                LevelManager.LoadLevel(int.Parse(code[1]));

                WorldLocation = new Vector2(
                    int.Parse(code[2]) * TileMap.TileWidth,
                    int.Parse(code[3]) * TileMap.TileHeight);

                LevelManager.RespawnLocation = WorldLocation;

                velocity = Vector2.Zero;
            }
        }

        #endregion


    }
}
