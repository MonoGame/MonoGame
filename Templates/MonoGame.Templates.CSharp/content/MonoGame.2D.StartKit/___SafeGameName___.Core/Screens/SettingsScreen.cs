using ___SafeGameName___.Core.Effects;
using ___SafeGameName___.Core.Localization;
using ___SafeGameName___.ScreenManagers;
using Microsoft.Xna.Framework;
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
    #region Fields
    MenuEntry fullscreenMenuEntry;
    MenuEntry languageMenuEntry;
    MenuEntry particleEffectMenuEntry;
    private MenuEntry backMenuEntry;
    static List<CultureInfo> languages;
    static int currentLanguage = 0;

    private GraphicsDeviceManager gdm;

    static ParticleEffectType currentParticleEffect = ParticleEffectType.Fireworks;
    public static ParticleEffectType CurrentParticleEffect { get => currentParticleEffect; }
    #endregion
    #region Initialization

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

        SetMenuEntryText();
    }

    /// <summary>
    /// Fills in the latest values for the options screen menu text.
    /// </summary>
    void SetMenuEntryText()
    {
        if (gdm == null)
        {
            gdm = ScreenManager.Game.Services.GetService<GraphicsDeviceManager>();
        }
        fullscreenMenuEntry.Text = string.Format(Resources.DisplayMode, gdm.IsFullScreen ? Resources.FullScreen : Resources.Windowed);

        var selectedLanguage = languages[currentLanguage].DisplayName;
        if (selectedLanguage.Contains("Invariant"))
        {
            selectedLanguage = Resources.English;
        }
        languageMenuEntry.Text = Resources.Language + selectedLanguage;
        ;
        particleEffectMenuEntry.Text = Resources.ParticleEffect + currentParticleEffect;
        backMenuEntry.Text = Resources.Back;
    }

    #endregion
    #region Handle Input
    /// <summary>
    /// Event handler for when the Fullscreen menu entry is selected.
    /// </summary>
    void FullScreenMenuEntrySelected(object sender, PlayerIndexEventArgs e)
    {
        gdm.ToggleFullScreen();

        SetMenuEntryText();
    }

    /// <summary>
    /// Event handler for when the Language menu entry is selected.
    /// </summary>
    void LanguageMenuEntrySelected(object sender, PlayerIndexEventArgs e)
    {
        currentLanguage = (currentLanguage + 1) % languages.Count;

        var selectedLanguage = languages[currentLanguage].Name;
        LocalizationManager.SetCulture(selectedLanguage);

        SetMenuEntryText();
    }

    private void ParticleEffectMenuEntrySelected(object sender, PlayerIndexEventArgs e)
    {
        currentParticleEffect++;

        if (currentParticleEffect > ParticleEffectType.Confetti)
        {
            currentParticleEffect = 0;
        }

        SetMenuEntryText();
    }

    #endregion
}
