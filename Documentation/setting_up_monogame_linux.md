This section will help you setup MonoGame on Linux.

### Running MonoGame Applications

The following packages are needed for the MonoGame Applications to run on Linux:
* libsdl-mixer1.2
* libopenal-dev
* mono-runtime

For Ubuntu/Debian based Linux systems, you can run:
```
sudo apt-get install libopenal-dev libsdl-mixer1.2 mono-runtime
```

### Developing MonoGame Applications

To setup MonoGame application development on linux do the following:
* Download and install Monodevelop 5 from: [http://www.monodevelop.com/download/](http://www.monodevelop.com/download/)
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
