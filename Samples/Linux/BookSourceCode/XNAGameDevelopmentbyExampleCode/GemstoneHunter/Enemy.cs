using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Tile_Engine;

namespace Gemstone_Hunter
{
    public class Enemy : GameObject
    {
        private Vector2 fallSpeed = new Vector2(0, 20);
        private float walkSpeed = 60.0f;
        private bool facingLeft = true;
        public bool Dead = false;

        #region Constructor
        public Enemy(ContentManager content, int cellX, int cellY)
        {
            animations.Add("idle",
                new AnimationStrip(
                    content.Load<Texture2D>(
                        @"Textures\Sprites\MonsterC\Idle"),
                    48,
                    "idle"));
            animations["idle"].LoopAnimation = true;

            animations.Add("run",
                new AnimationStrip(
                    content.Load<Texture2D>(
                        @"Textures\Sprites\MonsterC\Run"),
                    48,
                    "run"));
            animations["run"].FrameLength = 0.1f;
            animations["run"].LoopAnimation = true;

            animations.Add("die",
                new AnimationStrip(
                    content.Load<Texture2D>(
                        @"Textures\Sprites\MonsterC\Die"),
                    48,
                    "die"));
            animations["die"].LoopAnimation = false;

            frameWidth = 48;
            frameHeight = 48;
            CollisionRectangle = new Rectangle(9, 1, 30, 46);

            worldLocation = new Vector2(
                cellX * TileMap.TileWidth,
                cellY * TileMap.TileHeight);

            enabled = true;

            codeBasedBlocks = true;
            PlayAnimation("run");
        }
        #endregion

        #region Public Methods
        public override void Update(GameTime gameTime)
        {
            Vector2 oldLocation = worldLocation;

            if (!Dead)
            {
                velocity = new Vector2(0, velocity.Y);

                Vector2 direction = new Vector2(1, 0);
                flipped = true;

                if (facingLeft)
                {
                    direction = new Vector2(-1, 0);
                    flipped = false;
                }

                direction *= walkSpeed;
                velocity += direction;
                velocity += fallSpeed;
            }

            base.Update(gameTime);

            if (!Dead)
            {
                if (oldLocation == worldLocation)
                {
                    facingLeft = !facingLeft;
                }
            }
            else
            {
                if (animations[currentAnimation].FinishedPlaying)
                {
                    enabled = false;
                }
            }

        }
        #endregion

    }
}
