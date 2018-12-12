using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Microsoft.Xna.Framework.Forms
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    [CLSCompliant(false)]
    public partial class GameView : ContentView
	{
        public static readonly BindableProperty GameProperty = BindableProperty.Create("Game", typeof(Microsoft.Xna.Framework.Game), typeof(GameView), null, BindingMode.TwoWay, null,
                (bindable, oldvalue, newvalue) =>
                {
                    ((GameView)bindable).Game = newvalue as Microsoft.Xna.Framework.Game;
                });


        public Microsoft.Xna.Framework.Game Game
        {
            get
            {
                return (Microsoft.Xna.Framework.Game)GetValue(GameProperty);
            }
            set
            {
                SetValue(GameProperty, value);
            }
        }


        public GameView ()
		{
			InitializeComponent ();
		}
	}
}
