using ___SafeGameName___.Core;
using System.Windows.Forms;

internal class Program
{
    private static void Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);  // Adjust the mode as needed
        using var game = new ___SafeGameName___Game();
        game.Run();
    }
}
