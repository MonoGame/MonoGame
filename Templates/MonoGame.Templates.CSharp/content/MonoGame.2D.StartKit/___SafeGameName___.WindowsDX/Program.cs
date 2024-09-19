
using ___SafeGameName___.Core;
using System.Windows.Forms;

Application.SetHighDpiMode(HighDpiMode.SystemAware);  // Adjust the mode as needed
using var game = new ___SafeGameName___Game();
game.Run();
