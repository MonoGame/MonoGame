using ___SafeGameName___.Core;
using ___SafeGameName___.Core.Effects;
using ___SafeGameName___.Core.Localization;
using ___SafeGameName___.Core.Settings;
using ___SafeGameName___.ScreenManagers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Globalization;

namespace ___SafeGameName___.Screens;

/// <summary>
/// The settings screen is brought up over the top of the main menu
/// screen, and gives the user a chance to configure the game
/// in various hopefully useful ways.
/// </summary>
class SettingsScreen : MenuScreen
{
    MenuEntry fullscreenMenuEntry;
    MenuEntry languageMenuEntry;
    MenuEntry particleEffectMenuEntry;
    private MenuEntry backMenuEntry;
    static List<CultureInfo> languages;
    static int currentLanguage = 0;

    private GraphicsDeviceManager gdm;

    static ParticleEffectType currentParticleEffect = ParticleEffectType.Fireworks;
    public static ParticleEffectType CurrentParticleEffect { get => currentParticleEffect; }

    private SettingsManager<___SafeGameName___Settings> settingsManager;
    private ParticleManager particleManager;

    /// <summary>
    /// Constructor.
    /// </summary>
    public SettingsScreen()
        : base(Resources.Settings)
    {
        List<CultureInfo> cultures = LocalizationManager.GetSupportedCultures();
        languages = new List<CultureInfo>();
        for (int i = 0; i < cultures.Count; i++)
        {
            languages.Add(cultures[i]);
        }

        // Create our menu entries.
        fullscreenMenuEntry = new MenuEntry(string.Empty);
        languageMenuEntry = new MenuEntry(string.Empty);
        particleEffectMenuEntry = new MenuEntry(string.Empty);
        backMenuEntry = new MenuEntry(string.Empty);

        // Hook up menu event handlers.
        fullscreenMenuEntry.Selected += FullScreenMenuEntrySelected;
        languageMenuEntry.Selected += LanguageMenuEntrySelected;
        particleEffectMenuEntry.Selected += ParticleEffectMenuEntrySelected;
        backMenuEntry.Selected += OnCancel;

        // Add entries to the menu.
        MenuEntries.Add(fullscreenMenuEntry);
        MenuEntries.Add(languageMenuEntry);
        MenuEntries.Add(particleEffectMenuEntry);
        MenuEntries.Add(backMenuEntry);
    }

    public override void LoadContent()
    {
        base.LoadContent();

        // Lazy Load some things
        gdm ??= ScreenManager.Game.Services.GetService<GraphicsDeviceManager>();

        settingsManager ??= ScreenManager.Game.Services.GetService<SettingsManager<___SafeGameName___Settings>>();

        settingsManager.Settings.PropertyChanged += (s, e) =>
        {
            SetLanguageText();

            settingsManager.Save();
        };

        currentLanguage = settingsManager.Settings.Language;
        currentParticleEffect = settingsManager.Settings.ParticleEffect;
        gdm.IsFullScreen = settingsManager.Settings.FullScreen;

        SetLanguageText();

        particleManager ??= ScreenManager.Game.Services.GetService<ParticleManager>();
    }

    public override void Update(GameTime gameTime,
       bool otherScreenHasFocus,
       bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        particleManager.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, ScreenManager.GlobalTransformation);

        particleManager.Draw(spriteBatch);

        spriteBatch.End();

        base.Draw(gameTime);
    }

    /// <summary>
    /// Fills in the latest values for the options screen menu text.
    /// </summary>
    void SetLanguageText()
    {
        fullscreenMenuEntry.Text = string.Format(Resources.DisplayMode, gdm.IsFullScreen ? Resources.FullScreen : Resources.Windowed);

        var selectedLanguage = languages[currentLanguage].DisplayName;
        if (selectedLanguage.Contains("Invariant"))
        {
            selectedLanguage = Resources.English;
        }
        languageMenuEntry.Text = Resources.Language + selectedLanguage;

        particleEffectMenuEntry.Text = Resources.ParticleEffect + currentParticleEffect;

        backMenuEntry.Text = Resources.Back;

        Title = Resources.Settings;
    }

    /// <summary>
    /// Event handler for when the Fullscreen menu entry is selected.
    /// </summary>
    void FullScreenMenuEntrySelected(object sender, PlayerIndexEventArgs e)
    {
        gdm.ToggleFullScreen();

        settingsManager.Settings.FullScreen = gdm.IsFullScreen;
    }

    /// <summary>
    /// Event handler for when the Language menu entry is selected.
    /// </summary>
    void LanguageMenuEntrySelected(object sender, PlayerIndexEventArgs e)
    {
        currentLanguage = (currentLanguage + 1) % languages.Count;

        var selectedLanguage = languages[currentLanguage].Name;
        LocalizationManager.SetCulture(selectedLanguage);

        settingsManager.Settings.Language = currentLanguage;
    }

    private void ParticleEffectMenuEntrySelected(object sender, PlayerIndexEventArgs e)
    {
        currentParticleEffect++;

        if (currentParticleEffect > ParticleEffectType.Confetti)
        {
            currentParticleEffect = 0;
        }

        settingsManager.Settings.ParticleEffect = currentParticleEffect;

        particleManager.Emit(100, currentParticleEffect);  // Emit 100 particles
    }
}
