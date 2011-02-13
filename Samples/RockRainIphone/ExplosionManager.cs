using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RockRainIphone.Core;


namespace RockRainIphone
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ExplosionManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        protected List<ExplosionParticleSystem> explosions;

        public ExplosionManager(Game game)
            : base(game)
        {
            explosions = new List<ExplosionParticleSystem>();
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        public void AddExplosion(Vector2 position)
        {
            ExplosionParticleSystem explosion = new ExplosionParticleSystem(Game, 1);
            explosion.AddParticles(position);
            explosions.Add(explosion);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < explosions.Count; i++)
            {
                if (!explosions[i].Active)
                {
                    explosions.RemoveAt(i);
                    i--;
                }
                else
                {
                    explosions[i].Update(gameTime);
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            for (int i = 0; i < explosions.Count; i++)
            {
                explosions[i].Draw(gameTime);
            }

            base.Draw(gameTime);
        }
    }
}