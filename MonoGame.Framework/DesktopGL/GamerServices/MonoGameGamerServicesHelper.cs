using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.GamerServices
{
    internal class MonoGameGamerServicesHelper
    {
        private static MonoLiveGuide guide = null;



        public static void ShowSigninSheet()
        {
            guide.Enabled = true;
            guide.Visible = true;
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
        Texture2D signInProgress;
        Color alphaColor = new Color(128, 128, 128, 0);
        byte startalpha = 0;

        public MonoLiveGuide(Game game)
            : base(game)
        {
            this.Enabled = false;
            this.Visible = false;
            //Guide.IsVisible = false;
            this.DrawOrder = Int32.MaxValue;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        Texture2D Circle(GraphicsDevice graphics, int radius)
        {
            int aDiameter = radius * 2;
            Vector2 aCenter = new Vector2(radius, radius);

            Texture2D aCircle = new Texture2D(graphics, aDiameter, aDiameter, false, SurfaceFormat.Color);
            Color[] aColors = new Color[aDiameter * aDiameter];

            for (int i = 0; i < aColors.Length; i++)
            {
                int x = (i + 1) % aDiameter;
                int y = (i + 1) / aDiameter;

                Vector2 aDistance = new Vector2(Math.Abs(aCenter.X - x), Math.Abs(aCenter.Y - y));


                if (Math.Sqrt((aDistance.X * aDistance.X) + (aDistance.Y * aDistance.Y)) > radius)
                {
                    aColors[i] = Color.Transparent;
                }
                else
                {
                    aColors[i] = Color.White;
                }
            }

            aCircle.SetData<Color>(aColors);

            return aCircle;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(this.Game.GraphicsDevice);

            signInProgress = Circle(this.Game.GraphicsDevice, 10);

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            Vector2 center = new Vector2(this.Game.GraphicsDevice.PresentationParameters.BackBufferWidth / 2, this.Game.GraphicsDevice.PresentationParameters.BackBufferHeight - 100);
            Vector2 loc = Vector2.Zero;
            alphaColor.A = startalpha;
            for (int i = 0; i < 12; i++)
            {
                float angle = (float)(i / 12.0 * Math.PI * 2);
                loc = new Vector2(center.X + (float)Math.Cos(angle) * 50, center.Y + (float)Math.Sin(angle) * 50);
                spriteBatch.Draw(signInProgress, loc, alphaColor);
                alphaColor.A += 255 / 12;
                if (alphaColor.A > 255) alphaColor.A = 0;
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }

        TimeSpan gt = TimeSpan.Zero;
        TimeSpan last = TimeSpan.Zero;

        public override void Update(GameTime gameTime)
        {
            if (gt == TimeSpan.Zero) gt = last = gameTime.TotalGameTime;

            if ((gameTime.TotalGameTime - last).Milliseconds > 100)
            {
                last = gameTime.TotalGameTime;
                startalpha += 255 / 12;
            }

            if ((gameTime.TotalGameTime - gt).TotalSeconds > 5) // close after 10 seconds
            {
                string strUsr = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

                if (strUsr.Contains(@"\"))
                {
                    int idx = strUsr.IndexOf(@"\") + 1;
                    strUsr = strUsr.Substring(idx, strUsr.Length - idx);
                }

                SignedInGamer sig = new SignedInGamer();
                sig.DisplayName = strUsr;
                sig.Gamertag = strUsr;

                Gamer.SignedInGamers.Add(sig);

                this.Visible = false;
                this.Enabled = false;
                //Guide.IsVisible = false;
                gt = TimeSpan.Zero;
            }
            base.Update(gameTime);
        }

    }
}
