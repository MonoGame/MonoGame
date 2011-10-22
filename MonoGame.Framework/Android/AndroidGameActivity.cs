using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Microsoft.Xna.Framework
{
    public class AndroidGameActivity : Activity
    {
        public Game Game { get; set; }

        protected override void OnPause()
        {
            base.OnPause();
            if (Game != null) Game.EnterBackground();
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (Game != null) Game.EnterForeground();
        }
    }
}