using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonoGame.Tests.Framework.Components
{
    class InitializeOrderComponent: GameComponent
    {
        static int g_initOrder = 0;

        public int InitOrder { get; private set; }
        public IGameComponent ChildComponent = null;

        public InitializeOrderComponent(Game game):base(game)
        {            
            InitOrder = -1;
        }

        public override void Initialize()
        {
            if (ChildComponent != null)
                Game.Components.Add(ChildComponent);

            InitOrder = g_initOrder++;
        }

    }
}
