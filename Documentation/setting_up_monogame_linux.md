This section will help you setup MonoGame on Linux.

### Running MonoGame Applications

The following packages are needed for the MonoGame Applications to run on Linux:
* libopenal-dev
* mono-runtime

For Ubuntu/Debian based Linux systems, you can run:
```
sudo apt-get install libopenal-dev mono-runtime
```

### Developing MonoGame Applications

##### From Ubuntu 15.04 or newer:

* Go to [MonoGame Downloads page](http://www.monogame.net/downloads/)
* Download MonoGame for Ubuntu
* Open up "monogame-sdk.deb" and install it
* That's it, MonoGame SDK is installed

##### From other Linux distros:

* Go to [MonoGame Downloads page](http://www.monogame.net/downloads/)
* Download MonoGame for other Linux distros
* Open up terminal and type in:
```
cd Downloads
chmod +x monogame-sdk.run
sudo ./monogame-sdk.run
```
* During the installation process the installer will ask you if you wish to install any missing dependencies automatically. If you for some reason don't want to install them automatically or the dependency installer is not available for your linux distribution, here is the list of needed packages:
  * monodevelop ([http://www.monodevelop.com/download/](http://www.monodevelop.com/download/))
  * libopenal-dev
  * referenceassemblies-pcl
  * gtk-sharp3
  * ttf-mscorefonts-installer (recommended, but not needed)
* That's it, MonoGame SDK is installed
