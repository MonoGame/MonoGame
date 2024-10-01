using ___SafeGameName___.Core;

internal class Program
{
    private static void Main(string[] args)
    {
        using var game = new ___SafeGameName___Game();
        game.Run();
    }
}
