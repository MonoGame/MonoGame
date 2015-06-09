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
* Go to [MonoGame Downloads page](http://www.monogame.net/downloads/)
* Click on the newest MonoGame release
* Download MonoGame for Linux
* Download fixpermissions script from: https://mega.co.nz/#!HBohkQRa!5qIePcQkePXCoigJfk_92DLcJC3agncRteNsjnyxLVE
* Open up terminal and type in:
```
cd Downloads
unzip MonoGame.Linux.zip -d MonoGame.Linux
cp fixpermissions.sh MonoGame.Linux/
cd MonoGame.Linux
./fixpermissions.sh
./generate.sh
sudo ./monogame_linux.run
```
* During the installation process the installer will ask you if you wish to install any missing dependencies automatically. If you for some reason don't want to install them automatically or the dependency installer is not available for your linux distribution, here is the list of needed packages:
  * monodevelop ([http://www.monodevelop.com/download/](http://www.monodevelop.com/download/))
  * libopenal-dev
  * libsdl-mixer1.2
  * referenceassemblies-pcl
  * ttf-mscorefonts-installer (recommended, but not needed)
* Open up MonoDevelop, go to Tools > Add-in Manager > Gallery, search for MonoGame and install the found addin. It should be found under Game Development.
* That's it, MonoGame is installed.
