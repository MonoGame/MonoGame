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


using System.Windows.Forms;

namespace MouseGetStateAndIsMouseVisibleTester
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public TextManager aTm;
        public InputManager cIm;
        private SpriteFont sfStandard;
        public MouseState mousestatus;
        public Texture2D t2dUiCursor;
        public Object aObjects;
        bool bWindowIsCentered = false;
        bool bFullScreen = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            cIm = new InputManager(this);
			Window.AllowUserResizing = true;

			// Uncomment the line below to test full screen on startup.
			//graphics.IsFullScreen = false;
			//bFullScreen = true;
			
			 // Subscribe to the game window's ClientSizeChanged event.
			Window.ClientSizeChanged += Window_ClientSizeChanged;
			Activated += HandleActivated;
			Deactivated += HandleDeactivated;
        }

        void HandleDeactivated (object sender, EventArgs e)
        {
               Console.WriteLine("DeActivated - IsActive? " + IsActive);
        }

        void HandleActivated (object sender, EventArgs e)
        {
			Console.WriteLine("Activated - IsActive? " + IsActive);
	}

    void Window_ClientSizeChanged( object sender, EventArgs e )
    {
			// Make changes to handle the new window size.
			//Console.WriteLine("Window size changed " + Window.ClientBounds);
    }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }


        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);

            sfStandard = Content.Load<SpriteFont>("fntStandard");
            t2dUiCursor = Content.Load<Texture2D>("UiCursor");

            aTm = new TextManager(this, sfStandard);
            Components.Add(aTm);

            aObjects = new Object(this, ref t2dUiCursor);
            Components.Add(aObjects);

            aObjects.pos.X = 200;
            aObjects.pos.Y = 200;
        }


        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public void Toggle()
        {
            graphics.ToggleFullScreen();
            
			if(bFullScreen)
			{
				bFullScreen = false;
			}//if
			else
			{
				bFullScreen = true;
			}//else
        }//Toggle

        public int GetBackBufferWidth()
        {
            return graphics.PreferredBackBufferWidth;
        }//GetBackBufferWidth

        public int GetBackBufferHeight()
        {
            return graphics.PreferredBackBufferHeight;
        }//GetBackBufferWidth
        

        protected override void Update(GameTime gameTime)
        {
            mousestatus = Mouse.GetState();

            cIm.InputHandler(mousestatus, gameTime);

            aObjects.pos.X = mousestatus.X;
            aObjects.pos.Y = mousestatus.Y;

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                base.Draw(gameTime); 
            spriteBatch.End();

       
        }
        
        public void CenterWindow()
        {
        	if(bFullScreen) return;
        	
#if MAC	
            int index;
            int upperBound;
            float fScreenWidth, fScreenHeight, fNewX, fNewY, fWindowWidth, fWindowHeight, fTitleBarHeight;
	        Screen[] screens = Screen.AllScreens;
	        
	        fScreenWidth = fScreenHeight = 0;
	        
            upperBound = screens.GetUpperBound(0);
            for (index = 0; index <= upperBound; index++)
            {
                if (screens[index].Primary)
                {
					fScreenWidth = (float)screens[index].Bounds.Width;
                    fScreenHeight = (float)screens[index].Bounds.Height;  
                    index = upperBound;
                }//if
            }//for
            
            fWindowWidth = graphics.PreferredBackBufferWidth;
            fWindowHeight = graphics.PreferredBackBufferHeight;
            	
            fNewX = (fScreenWidth - fWindowWidth) / 2;
            fNewY = (fScreenHeight - fWindowHeight) / 2;
            
            fTitleBarHeight = this.Window.Window.Frame.Height - fWindowHeight;
            
			System.Drawing.PointF pfLocation = new System.Drawing.PointF(fNewX,fNewY);
			System.Drawing.PointF pfSize = new System.Drawing.PointF(fWindowWidth, fWindowHeight + fTitleBarHeight);
			System.Drawing.SizeF sfSize = new System.Drawing.SizeF(pfSize);
			System.Drawing.RectangleF rectTemp = new System.Drawing.RectangleF(pfLocation, sfSize);
			this.Window.Window.SetFrame(rectTemp, true);
#endif		
        }//CenterWindow
    }
}
