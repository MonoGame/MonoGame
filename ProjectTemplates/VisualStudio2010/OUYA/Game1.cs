using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Ouya.Console.Api;

// There are three things that need to be done for each new OUYA project:
// 1. Insert your OUYA developer key in the Initialize method below.
// 2. Replace Resources\Raw\key.der with your application key downloaded from the OUYA dev portal.
// 3. Replace the product IDs in the call to RequestProductListAsync with your product IDs.

namespace $safeprojectname$
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        OuyaFacade facade;

        public Game1()  
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            facade = OuyaFacade.Instance;
            facade.Init(Activity, "insert your developer key here");

            base.Initialize();
            
            // Start the IAP test as an async task so it runs in the background.
            DoIapTest();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        async void DoIapTest()
        {
            // Retrieve the receipts to see what items the user has previously purchased.
            var receipts = await facade.RequestReceiptsAsync();
            // Retrieve the known products from the OUYA Store.
            var products = await facade.RequestProductListAsync("__TEST__01", "__TEST__02");
            // Make a purchase of the first product in the list.
            // NOTE: This would usually be initiated by a user action such as a button press.
            var purchaseResult = await facade.RequestPurchaseAsync(products[0]);
            // If the purchase was successful...
            if (purchaseResult)
            {
                // ...retrieve the receipts again from the OUYA Store.
                // This will include the new purchase.
                receipts = await facade.RequestReceiptsAsync();
            }
        }
    }
}
