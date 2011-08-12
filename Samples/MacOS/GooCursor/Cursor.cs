using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GooCursor
{
    public class Cursor : DrawableGameComponent
    {
        #region Private Structures
        private struct TrailNode
        {
            public Vector2 Position;
            public Vector2 Velocity;
        }
        #endregion

        #region Fields and properties
        // this is the sprite that is drawn at the current cursor position.
        // textureCenter is used to center the sprite when drawing.
        private Texture2D cursorTexture;
        private Vector2 textureCenter;
        private SpriteBatch spriteBatch;

        private Vector2 position;
        private int trailNodeCount;
        private TrailNode[] trailNodes;


        /// <summary>
        /// Gets of Sets the screen position of the cursor
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        /// <summary>
        /// Gets of Sets the stiffness of the trail
        /// A lower number means the trail will be longer
        /// </summary>
        public float TrailStiffness { get; set; }

        /// <summary>
        /// Controls the damping of the velocity of trail nodes
        /// </summary>
        public float TrailDamping { get; set; }

        /// <summary>
        /// Mass of a trails node
        /// </summary>
        public float TrailNodeMass { get; set; }

        /// <summary>
        /// Controls how fast the gamepad moves the cursor. 
        /// Measured in pixels per second.
        /// </summary>
        public float CursorSpeed { get; set; }

        /// <summary>
        /// The scaling applied at the tip of the cursor
        /// </summary>
        public float StartScale { get; set; }
        /// <summary>
        /// The scaling applied at the end of the cursor
        /// </summary>
        public float EndScale { get; set; }

        /// <summary>
        /// use this to control the rate of change between the 
        /// StartScale and the EndScale
        /// </summary>
        public float LerpExponent { get; set; }

        /// <summary>
        /// Color used to fill the cursor
        /// </summary>
        public Color FillColor { get; set; }

        /// <summary>
        /// color used for the cursor border
        /// </summary>
        public Color BorderColor { get; set; }

        /// <summary>
        /// Size of the border (in pixels)
        /// </summary>
        public float BorderSize { get; set; }


        #endregion

        #region Creation and initialization

        public Cursor(Game game, int trailNodesNo, float stiffness, float damping)
            :base(game)
        {
            trailNodeCount = trailNodesNo;
            TrailStiffness = stiffness;
            TrailDamping = damping;

            trailNodes = new TrailNode[trailNodeCount];
            CursorSpeed = 600;
            StartScale = 1.0f;
            EndScale = 0.3f;
            LerpExponent = 0.5f;
            TrailNodeMass = 11.2f;

            FillColor = Color.Black;
            BorderColor = Color.White;
            BorderSize = 10;
        }

        public Cursor(Game game, int trailNodesNo)
            : this(game, trailNodesNo, 30000, 600)
        {
        }

        public Cursor(Game game)
            : this(game,50,30000,600)
        {
        }

        protected override void LoadContent()
        {
            cursorTexture = Game.Content.Load<Texture2D>("cursor");

            textureCenter = new Vector2(cursorTexture.Width / 2, cursorTexture.Height / 2);

            spriteBatch = new SpriteBatch(GraphicsDevice);

            base.LoadContent();
        }

        //we can center the cursor once we
        //know how big the viewport will be
        //this only really effects the 360 code
        //where no mouse is available
        public override void Initialize()
        {            
            base.Initialize();

            Viewport vp = GraphicsDevice.Viewport;

            position.X = vp.X + (vp.Width / 2);
            position.Y = vp.Y + (vp.Height / 2);
        }

        #endregion

        #region Draw

        public override void Draw(GameTime gameTime)
        {

            //spriteBatch.Begin SpriteBlendMode.AlphaBlend);
            //spriteBatch.Begin(0, BlendState.AlphaBlend);
            spriteBatch.Begin ();
            //First we draw all the trail nodes using the border color
            //we need to draw them slightly larger, so the border is left visible
            //later, when we draw the actual nodes

            //adjust the StartScale and EndScale to take into consideration the border
            float borderStartScale = StartScale  + BorderSize / cursorTexture.Width;
            float borderEndScale = EndScale + BorderSize / cursorTexture.Width;

            //draw all nodes with the new scales
            for (int i = 0; i < trailNodeCount; i++)
            {
                TrailNode node = trailNodes[i];
                float lerpFactor = (float)i / (float)(trailNodeCount - 1);
                lerpFactor = (float)Math.Pow(lerpFactor, LerpExponent);
                float scale = MathHelper.Lerp(borderStartScale, borderEndScale, lerpFactor);
                
                //draw using the Border Color
                spriteBatch.Draw(cursorTexture, node.Position, null, BorderColor, 0.0f,
                    textureCenter, scale, SpriteEffects.None, 0.0f);
            }

            //Next, we draw all the nodes normally, using the Fill Color
            //Because before we drew them larger, after we draw them at
            //their normal size, a border will remain visible.
            for (int i = 0; i < trailNodeCount; i++)
            {
                TrailNode node = trailNodes[i];
                float lerpFactor = (float)i / (float)(trailNodeCount - 1);
                lerpFactor = (float)Math.Pow(lerpFactor, LerpExponent);
                float scale = MathHelper.Lerp(StartScale, EndScale, lerpFactor);
                
                //draw using the fill color
                spriteBatch.Draw(cursorTexture, node.Position, null, FillColor, 0.0f,
                    textureCenter, scale, SpriteEffects.None, 0.0f);
            }
            
            spriteBatch.End();
        }

        #endregion

        #region Update

        Vector2 deltaMovement;

        private void UpdateTrailNodes(float elapsed)
        {
            for (int i = 1; i < trailNodeCount; i++)
            {
                TrailNode tn = trailNodes[i];

                // Calculate spring force
                Vector2 stretch = tn.Position - trailNodes[i - 1].Position;
                Vector2 force = -TrailStiffness * stretch - TrailDamping * tn.Velocity;

                // Apply acceleration
                Vector2 acceleration = force / TrailNodeMass;
                tn.Velocity += acceleration * elapsed;

                // Apply velocity
                tn.Position += tn.Velocity * elapsed;
                trailNodes[i] = tn;
            }

        }

        public override void Update(GameTime gameTime)
        {

            // first, use the GamePad to update the cursor position

            // down on the thumbstick is -1. however, in screen coordinates, values
            // increase as they go down the screen. so, we have to flip the sign of the
            // y component of delta.
            deltaMovement = GamePad.GetState(PlayerIndex.One).ThumbSticks.Left;
            deltaMovement.Y *= -1;



            #if !XBOX360
            //use the mouse position as the cursor position
            MouseState mouseState = Mouse.GetState();
            position.X = mouseState.X;
            position.Y = mouseState.Y;
            #endif

            // modify position using delta, the CursorSpeed, and
            // the elapsed game time.
            position += deltaMovement * CursorSpeed *
                (float)gameTime.ElapsedGameTime.TotalSeconds;


            #if XBOX360
            // clamp the cursor position to the viewport, so that it can't move off the
            // screen.
            Viewport vp = GraphicsDevice.Viewport;
            position.X = MathHelper.Clamp(position.X, vp.X, vp.X + vp.Width);
            position.Y = MathHelper.Clamp(position.Y, vp.Y, vp.Y + vp.Height);
            #else
            // set the new mouse position using the combination of mouse and gamepad data.
            Mouse.SetPosition((int)position.X, (int)position.Y);    
            #endif


            //set position of first trail node;
            trailNodes[0].Position = position;
            //update the trails
            UpdateTrailNodes((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        #endregion
    }
}
