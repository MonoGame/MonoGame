using ___SafeGameName___.Core.Effects;
using ___SafeGameName___.Core.Localization;
using ___SafeGameName___.Core.Settings;
using ___SafeGameName___.ScreenManagers;
using ___SafeGameName___.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


#if !__IOS__
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
#endif

namespace ___SafeGameName___.Core;

/// <summary>
/// This is the main entry point for your game
/// </summary>
public class ___SafeGameName___Game : Game
{
    // Resources for drawing.
    GraphicsDeviceManager graphicsDeviceManager;

    ScreenManager screenManager;

    SettingsManager<___SafeGameName___Settings> settingsManager;
    SettingsManager<___SafeGameName___Leaderboard> leaderboardManager;
    private Texture2D particleTexture;
    private ParticleManager particleManager;

    public ___SafeGameName___Game()
    {
        graphicsDeviceManager = new GraphicsDeviceManager(this);
        // Add it as a service so we can shared 1 instance and grab it as necessary
        Services.AddService(typeof(GraphicsDeviceManager), graphicsDeviceManager);

        ISettingsStorage storage;
        // The platform-specific code will be in the platform-specific project
        if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
        {
            storage = new MobileSettingsStorage();
        }
        else if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            storage = new DesktopSettingsStorage();
        }
        else
        {
            // For now, we'll throw an exception if we don't know the platform
            throw new PlatformNotSupportedException();
        }

        // This will also load the settings file, if it exists
        settingsManager = new SettingsManager<___SafeGameName___Settings>(storage);
        Services.AddService(typeof(SettingsManager<___SafeGameName___Settings>), settingsManager);

        leaderboardManager = new SettingsManager<___SafeGameName___Leaderboard>(storage);
        Services.AddService(typeof(SettingsManager<___SafeGameName___Leaderboard>), leaderboardManager);

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

        // Get a list of all languages our app supports
        List<CultureInfo> cultures = LocalizationManager.GetSupportedCultures();
        var languages = new List<CultureInfo>();
        for (int i = 0; i < cultures.Count; i++)
        {
            languages.Add(cultures[i]);
        }

        var selectedLanguage = languages[settingsManager.Settings.Language].Name;
        LocalizationManager.SetCulture(selectedLanguage);

        // add the background screen to the screen manager
        screenManager.AddScreen(new BackgroundScreen(), null);

        // add the main menu screen to the screen manager
        screenManager.AddScreen(new MainMenuScreen(), null);
    }

    protected override void LoadContent()
    {
        // Create a particle manager at the center of the screen
        particleTexture = Content.Load<Texture2D>("Sprites/blank");
        particleManager = new ParticleManager(particleTexture, new Vector2(400, 200));

        // Let's reuse the particle manager across the game
        Services.AddService(typeof(ParticleManager), particleManager);
    }
}
