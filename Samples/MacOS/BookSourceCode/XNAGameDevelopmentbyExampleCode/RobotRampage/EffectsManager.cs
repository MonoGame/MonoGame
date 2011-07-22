using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Robot_Rampage
{
    static class EffectsManager
    {
        #region Declarations
        static public List<Particle> Effects = new List<Particle>();
        static Random rand = new Random();
        static public Texture2D Texture;
        static public Rectangle ParticleFrame = new Rectangle(0, 288, 2, 2);
        static public List<Rectangle> ExplosionFrames =
            new List<Rectangle>();
        #endregion

        #region Initialization
        public static void Initialize(
            Texture2D texture,
            Rectangle particleFrame,
            Rectangle explosionFrame,
            int explosionFrameCount)
        {
            Texture = texture;
            ParticleFrame = particleFrame;
            ExplosionFrames.Clear();
            ExplosionFrames.Add(explosionFrame);
            for (int x = 1; x < explosionFrameCount; x++)
            {
                explosionFrame.Offset(explosionFrame.Width, 0);
                ExplosionFrames.Add(explosionFrame);
            }
        }
        #endregion

        #region Helper Methods
        public static Vector2 randomDirection(float scale)
        {
            Vector2 direction;
            do
            {
                direction = new Vector2(
                rand.Next(0, 100) - 50,
                rand.Next(0, 100) - 50);
            } while (direction.Length() == 0);
            direction.Normalize();
            direction *= scale;

            return direction;
        }
        #endregion

        #region Public Methods
        static public void Update(GameTime gameTime)
        {
            for (int x = Effects.Count - 1; x >= 0; x--)
            {
                Effects[x].Update(gameTime);
                if (Effects[x].Expired)
                {
                    Effects.RemoveAt(x);
                }
            }
        }

        static public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Sprite sprite in Effects)
            {
                sprite.Draw(spriteBatch);
            }
        }

        public static void AddExplosion(
            Vector2 location,
            Vector2 momentum,
            int minPointCount,
            int maxPointCount,
            int minPieceCount,
            int maxPieceCount,
            float pieceSpeedScale,
            int duration,
            Color initialColor,
            Color finalColor)
        {
            float explosionMaxSpeed = 30f;
            int pointSpeedMin = (int)pieceSpeedScale * 2;
            int pointSpeedMax = (int)pieceSpeedScale * 3;

            Vector2 pieceLocation = location -
                new Vector2(ExplosionFrames[0].Width / 2,
                    ExplosionFrames[0].Height / 2);

            int pieces = rand.Next(minPieceCount, maxPieceCount + 1);
            for (int x = 0; x < pieces; x++)
            {
                Effects.Add(new Particle(
                    pieceLocation,
                    Texture,
                    ExplosionFrames[rand.Next(0, ExplosionFrames.Count)],
                    randomDirection(pieceSpeedScale) + momentum,
                    Vector2.Zero,
                    explosionMaxSpeed,
                    duration,
                    initialColor,
                    finalColor));
            }

            int points = rand.Next(minPointCount, maxPointCount + 1);
            for (int x = 0; x < points; x++)
            {
                Effects.Add(new Particle(
                    location,
                    Texture,
                    ParticleFrame,
                    randomDirection((float)rand.Next(
                        pointSpeedMin, pointSpeedMax)) + momentum,
                    Vector2.Zero,
                    explosionMaxSpeed,
                    duration,
                    initialColor,
                    finalColor));

            }
        }

        public static void AddExplosion(Vector2 location, Vector2 momentum)
        {
            AddExplosion(
                location,
                momentum,
                15,
                20,
                2,
                4,
                6.0f,
                90,
                new Color(1.0f, 0.3f, 0f, 0.5f),
                new Color(0.0f, 0.0f, 0f, 0f));
        }

        public static void AddLargeExplosion(Vector2 location)
        {
            AddExplosion(
                location,
                Vector2.Zero,
                15,
                20,
                4,
                6,
                30f,
                90,
                new Color(1.0f, 0.3f, 0f, 0.5f),
                new Color(0.0f, 0.0f, 0f, 0f));
        }

        public static void AddSparksEffect(
            Vector2 location,
            Vector2 impactVelocity)
        {
            int particleCount = rand.Next(10, 20);
            for (int x = 0; x < particleCount; x++)
            {
                Particle particle = new Particle(
                    location - (impactVelocity / 60),
                    Texture,
                    ParticleFrame,
                    randomDirection((float)rand.Next(10, 20)),
                    Vector2.Zero,
                    60,
                    20,
                    Color.Yellow,
                    Color.Orange);
                Effects.Add(particle);
            }
        }
        #endregion

    }
}
