using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
namespace MouseGetStateAndIsMouseVisibleTester
{
    public class TextManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        public SpriteFont sfStandard;
        protected SpriteBatch spriteBatch = null;
        private Game1 cG;
        
        public TextManager(Game game, SpriteFont sfStandardFont)
            : base(game)
        {
            cG = (Game1)game;
            spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
            sfStandard = sfStandardFont;

        }//TextManager

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.DrawString(sfStandard, "LeftMouse = " + cG.mousestatus.LeftButton.ToString(), new Vector2(0, 50), Color.White);
			
#if LINUX
			spriteBatch.DrawString(sfStandard, "MouseX = " + Mouse.GetState().X.ToString(), new Vector2(0, 100), Color.White);            
            spriteBatch.DrawString(sfStandard, "MouseY = " + Mouse.GetState().Y.ToString(), new Vector2(0, 130), Color.White);
#else			
			try
			{
	            spriteBatch.DrawString(sfStandard, "MouseX = " + cG.Window.Window.MouseLocationOutsideOfEventStream.X.ToString(), new Vector2(0, 100), Color.White);
	            spriteBatch.DrawString(sfStandard, "MouseY = " + cG.Window.Window.MouseLocationOutsideOfEventStream.Y.ToString(), new Vector2(0, 130), Color.White);
			}//try
			catch(Exception ex)
			{
			
			}
#endif   

            spriteBatch.DrawString(sfStandard, "Click here to Toggle Full Screen", new Vector2(0, 200), Color.White);
            
            spriteBatch.DrawString(sfStandard, "Click here to center window if in windowed mode", new Vector2(0, 300), Color.White);


        }//Draw
    }
}
