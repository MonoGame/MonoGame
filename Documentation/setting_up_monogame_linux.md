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

* Go to [MonoGame Downloads page](http://www.monogame.net/downloads/)
* Download MonoGame for Linux
* Open up terminal and type in:
```
cd Downloads
chmod +x monogame-sdk.run
sudo ./monogame-sdk.run
```
* During the installation process the installer will give you the following list of dependencies, please make sure they are installed:
  * monodevelop ([http://www.monodevelop.com/download/](http://www.monodevelop.com/download/))
  * libopenal-dev
  * gtk-sharp3
  * referenceassemblies-pcl (needed to use PCL template)
  * ttf-mscorefonts-installer (recommended, but not needed)
* That's it, MonoGame SDK is installed
