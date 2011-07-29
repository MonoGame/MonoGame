using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Asteroid_Belt_Assault
{
    class Particle : Sprite
    {
        private Vector2 acceleration;
        private float maxSpeed;
        private int initialDuration;
        private int remainingDuration;
        private Color initialColor;
        private Color finalColor;

        public int ElapsedDuration
        {
            get
            {
                return initialDuration - remainingDuration;
            }
        }

        public float DurationProgress
        {
            get
            {
                return (float)ElapsedDuration /
                    (float)initialDuration;
            }
        }

        public bool IsActive
        {
            get
            {
                return (remainingDuration > 0);
            }
        }

        public Particle(
            Vector2 location,
            Texture2D texture,
            Rectangle initialFrame,
            Vector2 velocity,
            Vector2 acceleration,
            float maxSpeed,
            int duration,
            Color initialColor,
            Color finalColor)
            : base(location, texture, initialFrame, velocity)
        {
            initialDuration = duration;
            remainingDuration = duration;
            this.acceleration = acceleration;
            this.initialColor = initialColor;
            this.maxSpeed = maxSpeed;
            this.finalColor = finalColor;
        }

        public override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                velocity += acceleration;
                if (velocity.Length() > maxSpeed)
                {
                    velocity.Normalize();
                    velocity *= maxSpeed;
                }
                TintColor = Color.Lerp(
                    initialColor,
                    finalColor,
                    DurationProgress);
                remainingDuration--;
                base.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                base.Draw(spriteBatch);
            }
        }

    }
}
