using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using MonoGame.Tests.Framework.Components;

namespace MonoGame.Tests.Framework
{
    public static class GameComponentTest
    {
        [Test]
#if DESKTOPGL
        [Ignore("This crashes inside SDL on Mac!")]
#endif
        public static void InitializeOrderTest()
        {
            var game = new TestGameBase();
            var gdm = new GraphicsDeviceManager(game);
            game.IsFixedTimeStep = false;
            game.ExitCondition = x => x.UpdateNumber > 1;

            var constructor2        = new InitializeOrderComponent(game);
            var preBaseInitialize2  = new InitializeOrderComponent(game);
            var postBaseInitialize2 = new InitializeOrderComponent(game);
            var loadContent2        = new InitializeOrderComponent(game);
            var update2             = new InitializeOrderComponent(game);
            var constructor         = new InitializeOrderComponent(game) {ChildComponent = constructor2};
            var preBaseInitialize   = new InitializeOrderComponent(game) {ChildComponent = preBaseInitialize2};
            var postBaseInitialize  = new InitializeOrderComponent(game) {ChildComponent = postBaseInitialize2};
            var loadContent         = new InitializeOrderComponent(game) {ChildComponent = loadContent2};
            var update              = new InitializeOrderComponent(game) {ChildComponent = update2};
            
            game.Components.Add(constructor);
            
            game.PreInitializeWith += (sender, args) =>
            {
                game.Components.Add(preBaseInitialize);
            };
            game.InitializeWith += (sender, args) =>
            {
                game.Components.Add(postBaseInitialize);
            };
            game.PreLoadContentWith += (sender, args) =>
            {
                game.Components.Add(loadContent);
            };
            game.PreUpdateWith += (sender, args) =>
            {
                game.Components.Add(update);
            };
            game.Run();
            game.Dispose();

            Assert.That(constructor.InitOrder == 0);
            Assert.That(preBaseInitialize.InitOrder == 1);
            Assert.That(constructor2.InitOrder == 2);
            Assert.That(preBaseInitialize2.InitOrder == 3);
            Assert.That(update2.InitOrder == 4);
            Assert.That(update.InitOrder == 5);
            Assert.That(postBaseInitialize.InitOrder == -1);
            Assert.That(postBaseInitialize2.InitOrder == -1);
        }


    }
}
