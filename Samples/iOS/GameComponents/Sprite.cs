using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;   //   for Texture2D
using Microsoft.Xna.Framework;  //  for Vector2

namespace Microsoft.Xna.Samples.GameComponents
{
    class Sprite : DrawableGameComponent
    {
        private readonly Texture2D texture;    //  sprite texture 
        private Vector2 position;     //  sprite position on screen
        private Vector2 speed;     //  speed in pixels
        private readonly SpriteBatch spriteBatch;

        public Sprite(Game game, Texture2D Texture, Vector2 Position, Vector2 Speed, SpriteBatch spriteBatch)
            : base(game)
        {
            this.texture = Texture;
            this.position = Position;
            this.spriteBatch = spriteBatch;
			this.speed = Speed;
        }

        public override void Update(GameTime gameTime)
        {
            //  Keep inside the screen
            //  right
            if(position.X + texture.Width + speed.X >  Game.Window.ClientBounds.Width)
                speed.X = -speed.X;
            //  bottom
            if (position.Y + texture.Height + speed.Y > Game.Window.ClientBounds.Height)
                speed.Y = -speed.Y;
            //  left
            if (position.X + speed.X < 0)
                speed.X = -speed.X;
            //  top
            if (position.Y + speed.Y < 0)
                speed.Y = -speed.Y;
            //  update position
            position += speed;
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Draw(texture, position, Color.White);
        }
    }
}



