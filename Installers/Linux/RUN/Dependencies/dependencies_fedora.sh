#!/bin/bash
sudo rpm --import "http://keyserver.ubuntu.com/pks/lookup?op=get&search=0x3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF"
sudo dnf config-manager --add-repo http://download.mono-project.com/repo/centos/
sudo dnf update
sudo dnf install -y monodevelop referenceassemblies-pcl mscore-fonts gtk-sharp3
