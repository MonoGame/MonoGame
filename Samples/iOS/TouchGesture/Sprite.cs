#region File Description
//-----------------------------------------------------------------------------
// Sprite.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace TouchGestureSample
{
    public class Sprite
    {
        // the possible colors for the sprite
        public static readonly Color[] Colors = new[]
        {
            Color.White,
            Color.Red,
            Color.Blue,
            Color.Green
        };

        // this is the amount of velocity that is maintained after
        // the sprite bounces off of the wall
        public const float BounceMagnitude = .5f;

        // this is the percentage of velocity lost each second as
        // the sprite moves around.
        public const float Friction = .9f;

        // the minimum and maximum scale values for the sprite
        public const float MinScale = .5f;
        public const float MaxScale = 2f;

        private Texture2D texture;
        private int colorIndex = 0;
        private float scale = 1f;

        public Vector2 Center;
        public Color Color = Colors[0];
        public Vector2 Velocity;

        public float Scale
        {
            get { return scale; }
            set { scale = MathHelper.Clamp(value, MinScale, MaxScale); }
        }

        public Rectangle HitBounds
        {
            get
            {
                // create a rectangle based on the texture
                Rectangle r = new Rectangle(
                    (int)(Center.X - texture.Width / 2 * Scale),
                    (int)(Center.Y - texture.Height / 2 * Scale),
                    (int)(texture.Width * Scale),
                    (int)(texture.Height * Scale));

                // inflate the texture a little to give us some additional pad room
                r.Inflate(10, 10);

                return r;
            }
        }

        public Sprite(Texture2D texture)
        {
            this.texture = texture;
        }

        public void ChangeColor()
        {
            // increment the color index and clamp to the array size
            colorIndex = (colorIndex + 1) % Colors.Length;

            // update to the new color
            Color = Colors[colorIndex];
        }

        /// <summary>
        /// Updates the sprite.
        /// </summary>
        /// <param name="gameTime">The current game timestamp.</param>
        /// <param name="bounds">The bounds in which the sprite should bounce around.</param>
        public void Update(GameTime gameTime, Rectangle bounds)
        {
            // move the sprite based on the velocity
            Center += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // apply friction to the velocity to slow the sprite down
            Velocity *= 1f - (Friction * (float)gameTime.ElapsedGameTime.TotalSeconds);

            // calculate the scaled width and height for the method
            float halfWidth = (texture.Width * Scale) / 2f;
            float halfHeight = (texture.Height * Scale) / 2f;

            // check each side to make sure the sprite is in the bounds. if
            // the sprite is outside the bounds, we move the sprite and reverse
            // the velocity on that axis.

            if (Center.X < bounds.Left + halfWidth)
            {
                Center.X = bounds.Left + halfWidth;
                Velocity.X *= -BounceMagnitude;
            }

            if (Center.X > bounds.Right - halfWidth)
            {
                Center.X = bounds.Right - halfWidth;
                Velocity.X *= -BounceMagnitude;
            }

            if (Center.Y < bounds.Top + halfHeight)
            {
                Center.Y = bounds.Top + halfHeight;
                Velocity.Y *= -BounceMagnitude;
            }

            if (Center.Y > bounds.Bottom - halfHeight)
            {
                Center.Y = bounds.Bottom - halfHeight;
                Velocity.Y *= -BounceMagnitude;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                texture,
                Center,
                null,
                Color,
                0,
                new Vector2(texture.Width / 2, texture.Height / 2),
                Scale,
                SpriteEffects.None,
                0);
        }
    }
}
