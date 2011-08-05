using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Robot_Rampage
{
    class Sprite
    {
        #region Declarations
        public Texture2D Texture;

        private Vector2 worldLocation = Vector2.Zero;
        private Vector2 velocity = Vector2.Zero;

        private List<Rectangle> frames = new List<Rectangle>();

        private int currentFrame;
        private float frameTime = 0.1f;
        private float timeForCurrentFrame = 0.0f;

        private Color tintColor = Color.White;

        private float rotation = 0.0f;

        public bool Expired = false;
        public bool Animate = true;
        public bool AnimateWhenStopped = true;

        public bool Collidable = true;
        public int CollisionRadius = 0;
        public int BoundingXPadding = 0;
        public int BoundingYPadding = 0;
        #endregion

        #region Constructors
        public Sprite(
            Vector2 worldLocation,
            Texture2D texture,
            Rectangle initialFrame,
            Vector2 velocity)
        {
            this.worldLocation = worldLocation;
            Texture = texture;
            this.velocity = velocity;

            frames.Add(initialFrame);
        }
        #endregion

        #region Drawing and Animation Properties
        public int FrameWidth
        {
            get { return frames[0].Width; }
        }

        public int FrameHeight
        {
            get { return frames[0].Height; }
        }

        public Color TintColor
        {
            get { return tintColor; }
            set { tintColor = value; }
        }

        public float Rotation
        {
            get { return rotation; }
            set { rotation = value % MathHelper.TwoPi; }
        }

        public int Frame
        {
            get { return currentFrame; }
            set
            {
                currentFrame = (int)MathHelper.Clamp(value, 0,
                    frames.Count - 1);
            }
        }

        public float FrameTime
        {
            get { return frameTime; }
            set { frameTime = MathHelper.Max(0, value); }
        }

        public Rectangle Source
        {
            get { return frames[currentFrame]; }
        }
        #endregion

        #region Positional Properties
        public Vector2 WorldLocation
        {
            get { return worldLocation; }
            set { worldLocation = value; }
        }

        public Vector2 ScreenLocation
        {
            get
            {
                return Camera.Transform(worldLocation);
            }
        }

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        public Rectangle WorldRectangle
        {
            get
            {
                return new Rectangle(
                    (int)worldLocation.X,
                    (int)worldLocation.Y,
                    FrameWidth,
                    FrameHeight);
            }
        }

        public Rectangle ScreenRectangle
        {
            get
            {
                return Camera.Transform(WorldRectangle);
            }
        }

        public Vector2 RelativeCenter
        {
            get { return new Vector2(FrameWidth / 2, FrameHeight / 2); }
        }

        public Vector2 WorldCenter
        {
            get { return worldLocation + RelativeCenter; }
        }

        public Vector2 ScreenCenter
        {
            get
            {
                return Camera.Transform(worldLocation + RelativeCenter);
            }
        }
        #endregion

        #region Collision Related Properties
        public Rectangle BoundingBoxRect
        {
            get
            {
                return new Rectangle(
                    (int)worldLocation.X + BoundingXPadding,
                    (int)worldLocation.Y + BoundingYPadding,
                    FrameWidth - (BoundingXPadding * 2),
                    FrameHeight - (BoundingYPadding * 2));
            }
        }
        #endregion

        #region Collision Detection Methods
        public bool IsBoxColliding(Rectangle OtherBox)
        {
            if ((Collidable) && (!Expired))
            {
                return BoundingBoxRect.Intersects(OtherBox);
            }
            else
            {
                return false;
            }
        }

        public bool IsCircleColliding(
            Vector2 otherCenter,
            float otherRadius)
        {
            if ((Collidable) && (!Expired))
            {
                if (Vector2.Distance(WorldCenter, otherCenter) <
                    (CollisionRadius + otherRadius))
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Animation-Related Methods
        public void AddFrame(Rectangle frameRectangle)
        {
            frames.Add(frameRectangle);
        }

        public void RotateTo(Vector2 direction)
        {
            Rotation = (float)Math.Atan2(direction.Y, direction.X);
        }
        #endregion

        #region Update and Draw Methods
        public virtual void Update(GameTime gameTime)
        {
            if (!Expired)
            {
                float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

                timeForCurrentFrame += elapsed;

                if (Animate)
                {
                    if (timeForCurrentFrame >= FrameTime)
                    {
                        if ((AnimateWhenStopped) ||
                            (velocity != Vector2.Zero))
                        {
                            currentFrame = (currentFrame + 1) %
                                (frames.Count);
                            timeForCurrentFrame = 0.0f;
                        }
                    }
                }

                worldLocation += (velocity * elapsed);
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!Expired)
            {
                if (Camera.ObjectIsVisible(WorldRectangle))
                {
                    spriteBatch.Draw(
                        Texture,
                        ScreenCenter,
                        Source,
                        tintColor,
                        rotation,
                        RelativeCenter,
                        1.0f,
                        SpriteEffects.None,
                        0.0f);
                }
            }
        }
        #endregion

    }
}
