using Android.App;
using Android.Content.PM;
using Android.OS;
using PeerToPeer;

namespace MonoGame.Samples.PeerToPeerGame.Droid
{
    [Activity(Label = "Peer2Peer", MainLauncher = true, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden)]
    public class Activity1 : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var g = new PeerToPeer.PeerToPeerGame(this);
            SetContentView(g.Window);
            g.Run();
        }
    }
}

