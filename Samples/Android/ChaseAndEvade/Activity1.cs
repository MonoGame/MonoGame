using Android.App;
using Android.Content.PM;
using Android.OS;
using ChaseAndEvade;

namespace MonoGame.Samples.ChaseAndEvade.Droid
{
    [Activity(Label = "ChaseAndEvade", MainLauncher = true, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden)]
    public class Activity1 : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            ChaseAndEvadeGame g = new ChaseAndEvadeGame(this);
            SetContentView(g.Window);
            g.Run();
        }
    }
}

