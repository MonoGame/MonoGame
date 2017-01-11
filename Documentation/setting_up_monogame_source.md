This section will help you setup MonoGame by building it from source code.

### Prerequisites

Install the tools for the system you are building from:
* Windows: 
  * [Git for Windows](https://git-scm.com/download/win)
  * [Visual Studio](https://www.visualstudio.com/)
  * [Xamarin.Android](https://www.xamarin.com/download) (Optional)
  * [Windows Phone 8 SDK](https://www.microsoft.com/en-us/download/details.aspx?id=35471) (Optional)
* Mac: 
  * [Git](https://git-scm.com/download/mac)
  * [Xamarin Studio](https://store.xamarin.com/)
  * Xamarin.Android and Xamarin.iOS can be installed with the Xamarin Studio installer (Optional)
* Linux: 
  * [Git](https://git-scm.com/download/linux)
  * [Monodevelop](http://www.monodevelop.com/download/linux/)

### Getting the source code

Start up a Terminal (Mac/Linux) or Git Bash (Windows) and clone the MonoGame repository:
```
git clone https://github.com/MonoGame/MonoGame.git
cd MonoGame
git submodule init
git submodule update
```

### Building from source

MonoGame uses [Protobuild](https://protobuild.org/) to generate project and solution files. Protobuild.exe will be in your MonoGame folder. To run Protobuild:

- On Windows run Protobuild.exe either by double-clicking or by executing it from the command line.
- On Mac/Linux open a terminal and run `mono Protobuild.exe` in the MonoGame folder.

Once the project and solution files are generated you can build them with the IDE you installed.

### Referencing the projects

First get the MonoGame SDK from the [downloads page](http://www.monogame.net/downloads/) and install it to get the IDE templates. Start up the IDE you have installed and create a new project from one of the templates. Click Add > Existing Project... on your solution and select the MonoGame.Framework project that matches the template (i.e. MonoGame.Framework.Windows.csproj for a MonoGame Windows project template). The project files are located in MonoGame/MonoGame.Framework. Delete the existing MonoGame.Framework reference and add a reference to the added project by clicking Add Reference... > Projects and selecting the project. You can run your game now. If you make changes to the MonoGame.Framework project it will automatically rebuild when running your game.
