using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.GamerServices
{
    internal class MonoGameGamerServicesHelper
    {
        private static MonoLiveGuide guide = null;

        public static void ShowSigninSheet()
        {
            guide.Enabled = true;
            Guide.IsVisible = true;
        }
    
        internal static void Initialise(Game game)
        {
            if (guide == null)
            {
                guide = new MonoLiveGuide(game);                
                game.Components.Add(guide);
            }
        }}

    internal class MonoLiveGuide : DrawableGameComponent
    {
        SpriteBatch spriteBatch;

        public MonoLiveGuide(Game game) : base(game)
        {
            this.Enabled = false;
            Guide.IsVisible = false;
            this.DrawOrder = Int32.MaxValue;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(this.Game.GraphicsDevice);
            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.End();
            base.Draw(gameTime);
        }

        TimeSpan gt = TimeSpan.Zero;

        public override void Update(GameTime gameTime)
        {
            if (gt == TimeSpan.Zero) gt = gameTime.TotalGameTime;

            if ((gameTime.TotalGameTime - gt).TotalSeconds > 10) // close after 10 seconds
            {
                SignedInGamer sig = new SignedInGamer();
                sig.DisplayName = "MonoGamer";
                sig.Gamertag = "MonoGamer";

                Gamer.SignedInGamers.Add(sig);

                this.Enabled = false;
                Guide.IsVisible = false;
                gt = TimeSpan.Zero;                
            }
            base.Update(gameTime);
        }

    }
}
