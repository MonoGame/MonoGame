//-----------------------------------------------------------------------------
// ParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AlienGameSample
{
    /// <summary>
    /// A relatively simple particle system.  We recycle particles instead of creating
    /// and destroying them as we need more.  "Effects" are created via factory methods
    /// on ParticleSystem, rather than a data driven model due to the relatively low
    /// number of effects.
    /// </summary>
    public class ParticleSystem
    {
        Random          random;

        Texture2D       tank_tire;
        Texture2D       tank_top;
        Texture2D       fire;
        Texture2D       smoke;

        SpriteBatch     spriteBatch;

        List<Particle>  particles;

        public ParticleSystem(ContentManager content, SpriteBatch spriteBatch)
        {
            random = new Random();

            particles = new List<Particle>();

            this.spriteBatch = spriteBatch;

            tank_tire = content.Load<Texture2D>("tank_tire");
            tank_top = content.Load<Texture2D>("tank_top");
            fire = content.Load<Texture2D>("fire");
            smoke = content.Load<Texture2D>("smoke");
        }

        /// <summary>
        /// Update all active particles.
        /// </summary>
        /// <param name="elapsed">The amount of time elapsed since last Update.</param>
        public void Update(float elapsed)
        {
            for (int i = 0; i < particles.Count; ++i)
            {
                particles[i].Life -= elapsed;
                if (particles[i].Life <= 0.0f)
                {
                    continue;
                }
                particles[i].Position += particles[i].Velocity * elapsed;
                particles[i].Rotation += particles[i].RotationRate * elapsed;
                particles[i].Alpha += particles[i].AlphaRate * elapsed;
                particles[i].Scale += particles[i].ScaleRate * elapsed;

                if (particles[i].Alpha <= 0.0f)
                    particles[i].Alpha = 0.0f;                                    
            }
        }

        /// <summary>
        /// Draws the particles.
        /// </summary>
        public void Draw()
        {
            for (int i = 0; i < particles.Count; ++i)
            {
                Particle p = particles[i];
                if (p.Life <= 0.0f)
                    continue;

                float alphaF = 255.0f * p.Alpha;
                if (alphaF < 0.0f)
                    alphaF = 0.0f;
                if (alphaF > 255.0f)
                    alphaF = 255.0f;

                spriteBatch.Draw(p.Texture, p.Position, null, new Color(p.Color.R, p.Color.G, p.Color.B, (byte)alphaF), p.Rotation, new Vector2(p.Texture.Width / 2, p.Texture.Height / 2), p.Scale, SpriteEffects.None, 0.0f);
            }
        }

        /// <summary>
        /// Creats a particle, preferring to reuse a dead one in the particles list 
        /// before creating a new one.
        /// </summary>
        /// <returns></returns>
        Particle CreateParticle()
        {
            Particle p = null;

            for (int i = 0; i < particles.Count; ++i)
            {
                if (particles[i].Life <= 0.0f)
                {
                    p = particles[i];
                    break;
                }
            }

            if (p == null)
            {
                p = new Particle();
                particles.Add(p);
            }

            p.Color = Color.White;

            return p;
        }

        /// <summary>
        /// Creats the effect for when an alien dies.
        /// </summary>
        /// <param name="position">Where on the screen to create the effect.</param>
        public void CreateAlienExplosion(Vector2 position)
        {
            Particle p = null;

            for (int i = 0; i < 8; ++i)
            {
                p = CreateParticle();
                p.Position = position;
                p.RotationRate = -6.0f + 12.0f * (float)random.NextDouble();
                p.Scale = 0.5f;
                p.ScaleRate = 0.25f;// *(float)random.NextDouble();
                p.Alpha = 2.0f;
                p.AlphaRate = -1.0f;
                p.Velocity.X = -32.0f + 64.0f * (float)random.NextDouble();
                p.Velocity.Y = -32.0f + 64.0f * (float)random.NextDouble();
                p.Texture = smoke;
                p.Life = 2.0f;
            }

            for (int i = 0; i < 3; ++i)
            {
                p = CreateParticle();
                p.Position = position;
                p.Position.X += -8.0f + 16.0f * (float)random.NextDouble();
                p.Position.Y += -8.0f + 16.0f * (float)random.NextDouble();
                p.RotationRate = -2.0f + 4.0f * (float)random.NextDouble();
                p.Scale = 0.25f;
                p.ScaleRate = 1.0f;// *(float)random.NextDouble();
                p.Alpha = 2.0f;
                p.AlphaRate = -1.0f;
                p.Velocity = Vector2.Zero;
                p.Texture = fire;
                p.Life = 2.0f;
            }
        }

        /// <summary>
        /// Creats the effect for when the player dies.
        /// </summary>
        /// <param name="position">Where on the screen to create the effect.</param>
        public void CreatePlayerExplosion(Vector2 position)
        {
            Particle p = null;

            for (int i = 0; i < 16; ++i)
            {
                p = CreateParticle();
                p.Position = position;
                p.RotationRate = -6.0f + 12.0f * (float)random.NextDouble();
                p.Scale = 0.5f;
                p.ScaleRate = 0.25f;// *(float)random.NextDouble();
                p.Alpha = 2.0f;
                p.AlphaRate = -1.0f;
                p.Velocity.X = -32.0f + 64.0f * (float)random.NextDouble();
                p.Velocity.Y = -32.0f + -48.0f * (float)random.NextDouble();
                p.Texture = smoke;
                p.Life = 2.0f;
            }

            p = CreateParticle();
            p.Texture = tank_tire;
            p.Position = position;
            p.Scale = 1.0f;
            p.ScaleRate = 0.0f;
            p.Alpha = 2.0f;
            p.AlphaRate = -1.0f;
            p.Life = 2.0f;
            p.RotationRate = 0.5f;
            p.Rotation = 0.0f;
            p.Velocity = new Vector2(40.0f, -75.0f);

            p = CreateParticle();
            p.Texture = tank_tire;
            p.Position = position;
            p.Scale = 1.0f;
            p.ScaleRate = 0.0f;
            p.Alpha = 2.0f;
            p.AlphaRate = -1.0f;
            p.Life = 2.0f;
            p.RotationRate = 0.5f;
            p.Rotation = 0.0f;
            p.Velocity = new Vector2(-45.0f, -90.0f);

            p = CreateParticle();
            p.Texture = tank_top;
            p.Position = position;
            p.Scale = 1.0f;
            p.ScaleRate = 0.0f;
            p.Alpha = 2.0f;
            p.AlphaRate = -1.0f;
            p.Life = 2.0f;
            p.RotationRate = 2.5f;
            p.Rotation = 0.0f;
            p.Velocity = new Vector2(0.0f, -60.0f);

            for (int i = 0; i < 8; ++i)
            {
                p = CreateParticle();
                p.Position = position;
                p.Position.X += -16.0f + 32.0f * (float)random.NextDouble();
                p.Position.Y += -16.0f + 32.0f * (float)random.NextDouble();
                p.RotationRate = -2.0f + 4.0f * (float)random.NextDouble();
                p.Scale = 0.25f;
                p.ScaleRate = 1.0f;// *(float)random.NextDouble();
                p.Alpha = 2.0f;
                p.AlphaRate = -1.0f;
                p.Velocity.X = -4.0f + 8.0f * (float)random.NextDouble();
                p.Velocity.Y = -4.0f + -8.0f * (float)random.NextDouble();
                p.Texture = fire;
                p.Life = 2.0f;
            }
        }

        /// <summary>
        /// Creats the mud/dust effect when the player moves.
        /// </summary>
        /// <param name="position">Where on the screen to create the effect.</param>        
        public void CreatePlayerDust(Player player)
        {
            for (int i = 0; i < 2; ++i)
            {
                Particle p = CreateParticle();
                p.Texture = smoke;
                p.Color = new Color(125, 108, 43);
                p.Position.X = player.Position.X + player.Width * (float)random.NextDouble();
                p.Position.Y = player.Position.Y + player.Height - 3.0f * (float)random.NextDouble();
                p.Alpha = 1.0f;
                p.AlphaRate = -2.0f;
                p.Life = 0.5f;
                p.Rotation = 0.0f;
                p.RotationRate = -2.0f + 4.0f * (float)random.NextDouble();
                p.Scale = 0.25f;
                p.ScaleRate = 0.5f;
                p.Velocity.X = -4 + 8.0f * (float)random.NextDouble();
                p.Velocity.Y = -8 + 4.0f * (float)random.NextDouble();                
            }
        }


        /// <summary>
        /// Creats the effect for when the player fires a bullet.
        /// </summary>
        /// <param name="position">Where on the screen to create the effect.</param>        
        public void CreatePlayerFireSmoke(Player player)
        {
            for (int i = 0; i < 8; ++i)
            {
                Particle p = CreateParticle();
                p.Texture = smoke;
                p.Color = Color.White;
                p.Position.X = player.Position.X + player.Width / 2;
                p.Position.Y = player.Position.Y;
                p.Alpha = 1.0f;
                p.AlphaRate = -1.0f;
                p.Life = 1.0f;
                p.Rotation = 0.0f;
                p.RotationRate = -2.0f + 4.0f * (float)random.NextDouble();
                p.Scale = 0.25f;
                p.ScaleRate = 0.25f;
                p.Velocity.X = -4 + 8.0f * (float)random.NextDouble();
                p.Velocity.Y = -16.0f + -32.0f * (float)random.NextDouble();
            }
        }
    }

    /// <summary>
    /// A basic particle.  Since this is strictly a data class, I decided to not go
    /// the full property route and used public fields instead.
    /// </summary>
    public class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Texture2D Texture;
        public float RotationRate;
        public float Rotation;
        public float Life;
        public float AlphaRate;
        public float Alpha;
        public float ScaleRate;
        public float Scale;
        public Color Color = Color.White;
    }    
}