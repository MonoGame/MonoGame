using Android.App;
using Android.Content.PM;
using Android.OS;

namespace Platformer2D
{
    [Activity(Label = "Platformer2D"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = Android.Content.PM.LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.SensorLandscape
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class Activity1 : Microsoft.Xna.Framework.AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var g = new PlatformerGame();
            
            // With Protobuild, content is included directly under "Assets"
            // on Android, and not under a "Content" subdirectory.
            g.Content.RootDirectory = string.Empty;
            
            SetContentView((Android.Views.View)g.Services.GetService(typeof(Android.Views.View)));
            g.Run();
        }
    }
}

