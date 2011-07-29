namespace ChaseAndEvade
{
	class Program
	{
		static void Main (string[] args)
		{
			using (ChaseAndEvadeGame game = new ChaseAndEvadeGame ()) {
				game.Run ();
			}
		}
	}
}
