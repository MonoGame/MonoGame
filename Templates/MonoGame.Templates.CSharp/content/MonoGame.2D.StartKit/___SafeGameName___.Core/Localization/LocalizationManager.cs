using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace ___SafeGameName___.Core.Localization;

internal class LocalizationManager
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns>A list of Supported CultureInfo, based on what languages we currently install.</returns>
    public static List<CultureInfo> GetSupportedCultures()
    {
        // Create a list to hold supported cultures
        List<CultureInfo> supportedCultures = new List<CultureInfo>();

        // Get the current assembly
        Assembly assembly = Assembly.GetExecutingAssembly();

        // Resource manager for your Resources.resx
        ResourceManager resourceManager = new ResourceManager("___SafeGameName___.Core.Localization.Resources", assembly);

        // Get all cultures defined in the satellite assemblies
        CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

        foreach (CultureInfo culture in cultures)
        {
            try
            {
                // Try to get the resource set for this culture
                var resourceSet = resourceManager.GetResourceSet(culture, true, false);
                if (resourceSet != null)
                {
                    supportedCultures.Add(culture);
                }
            }
            catch (MissingManifestResourceException)
            {
                // This exception is thrown when there is no .resx for the culture, ignore it
            }
        }

        // Always add the default (invariant) culture - the base .resx file
        supportedCultures.Add(CultureInfo.InvariantCulture);

        return supportedCultures;
    }

    public static void SetCulture(string cultureCode)
    {
        CultureInfo culture = new CultureInfo(cultureCode);
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }
}