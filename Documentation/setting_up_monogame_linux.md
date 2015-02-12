This section will help you setup MonoGame on Linux.

The following packages are needed for the MonoGame to work on Linux:
* libsdl-mixer1.2
* libopenal-dev

For Ubuntu/Debian based Linux systems, you can run:
```
sudo apt-get install libopenal-dev libsdl-mixer1.2
```

For developing MonoGame applications from linux, Monodevelop 5 is needed. Please download and install it from: [http://www.monodevelop.com/download/](http://www.monodevelop.com/download/)

To install the MonoGame on linux do the following:
* Go to [MonoGame Downloads page](http://www.monogame.net/downloads/)
* Click on the newest MonoGame release
* Download MonoGame for Linux
* Open up terminal and type in:
```
cd Downloads
chmod +x monogame_linux.run
sudo ./monogame_linux.run
```
* That's it, MonoGame is installed.

If you can't find MonoGame Project in Monodevelop, go to Tools > Add-in Manager, Expand Game Development and make sure that MonoGame Addin is enabled.
