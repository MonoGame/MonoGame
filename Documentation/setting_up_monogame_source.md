This section will help you setup MonoGame by building it from source code.

### Prerequisites

Install the tools for the system you are building from:
* Windows: 
  * [GitHub for Windows](https://windows.github.com/)
  * [Visual Studio](https://www.visualstudio.com/) or [Xamarin Studio](http://www.monodevelop.com/download/)
  * Optional download Xamarin.Android and Windows Phone 8 SDK
* Mac: 
  * [GitHub for Mac](https://mac.github.com/)
  * [Xamarin Studio](http://www.monodevelop.com/download/)
  * Optional download Xamarin.Android and Xamarin.iOS
* Linux: 
  * Install package called "git"
  * [Monodevelop](http://www.monodevelop.com/download/linux/)

### Getting the source code

If on Windows, start up Git Shell, if on Unix, start the terminal and type in the following:
```
git clone https://github.com/mono/MonoGame.git
cd MonoGame
git submodule init
git submodule update
```

### Building from source

If on Windows just start Protobuild.exe.

if on Unix open terminal and type in:
```
cd MonoGame
mono Protobuild.exe
```

Now the sln files are generated and you can build them with either Monodevelop or Visual Studio.
