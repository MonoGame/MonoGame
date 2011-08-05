namespace TiledSprites
{
	class Program
	{
		static void Main (string [] args)
		{
			using (TiledSpritesSample game = new TiledSpritesSample ()) {
				game.Run ();
			}
		}
	}
}
