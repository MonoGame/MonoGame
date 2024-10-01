using ___SafeGameName___.ScreenManagers;
using ___SafeGameName___.Screens;
using Microsoft.Xna.Framework;

#if !__IOS__
using Microsoft.Xna.Framework.Media;
#endif

namespace ___SafeGameName___.Core;

/// <summary>
/// This is the main entry point for your game
/// </summary>
public class ___SafeGameName___Game : Game
{
    // Resources for drawing.
    private GraphicsDeviceManager graphicsDeviceManager;

    ScreenManager screenManager;

    public ___SafeGameName___Game()
    {
        graphicsDeviceManager = new GraphicsDeviceManager(this);
        // Add it as a service so we can shared 1 instance and grab it as necessary
        Services.AddService(typeof(GraphicsDeviceManager), graphicsDeviceManager);

        Content.RootDirectory = "Content";

        graphicsDeviceManager.IsFullScreen = false;

        //graphics.PreferredBackBufferWidth = 800;
        //graphics.PreferredBackBufferHeight = 480;
        graphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

        // Create the screen manager component.
        screenManager = new ScreenManager(this);

        Components.Add(screenManager);
    }

    /// <summary>
    /// Override Game.Initialize().  Sets up the ScreenManager.
    /// </summary>
    protected override void Initialize()
    {
        base.Initialize();

        // add the background screen to the screen manager
        screenManager.AddScreen(new BackgroundScreen(), null);

        // add the main menu screen to the screen manager
        screenManager.AddScreen(new MainMenuScreen(), null);
    }
}
