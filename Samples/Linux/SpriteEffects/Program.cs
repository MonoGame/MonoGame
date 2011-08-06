namespace SpriteEffects
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main (string[] args)
		{
			using (SpriteEffectsGame game = new SpriteEffectsGame ()) {
				game.Run ();
			}
		}
	}		
}

