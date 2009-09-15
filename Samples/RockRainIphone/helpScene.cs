#region Using Statements

using XnaTouch.Framework;
using XnaTouch.Framework.Graphics;
using RockRainIphone.Core;
using XnaTouch.Framework.Input;

#endregion

namespace RockRainIphone
{
    /// <summary>
    /// This is a game component thats represents the Instrucions Scene
    /// </summary>
    public class HelpScene : GameScene
    {
        public HelpScene(Game game, Texture2D textureBack)
            : base(game)
        {
            Components.Add(new ImageComponent(game, textureBack, 
                ImageComponent.DrawMode.Stretch));
        }
    }
}