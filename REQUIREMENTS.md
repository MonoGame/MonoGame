Windows Requirements
====================

Mac Requirements
================

Linux Requirements
==================

The following is the list of packages needed to compile MonoGame from Linux:
 * monodevelop
 * libopenal-dev
 * referenceassemblies-pcl
 * ttf-mscorefonts-installer
 * gtk-sharp3

If on Ubuntu, you can install the packages with the following commands:
```Shell
sudo apt-key adv --keyserver keyserver.ubuntu.com --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb http://download.mono-project.com/repo/debian wheezy main" | sudo tee /etc/apt/sources.list.d/mono-xamarin.list
sudo apt-get update
sudo apt-get install -y monodevelop libopenal-dev referenceassemblies-pcl ttf-mscorefonts-installer gtk-sharp3
```

If on Fedora, you can install the packages with the following commands:
```Shell
sudo rpm --import "http://keyserver.ubuntu.com/pks/lookup?op=get&search=0x3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF"
sudo dnf config-manager --add-repo http://download.mono-project.com/repo/centos/
sudo dnf update
sudo dnf install -y monodevelop referenceassemblies-pcl mscore-fonts gtk-sharp3
```
