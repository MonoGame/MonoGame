#!/bin/bash
mkdir Installer

cp Data/. Installer/Data/ -R
cp ../../Tools/Pipeline/bin/Linux/AnyCPU/Release/. Installer/Data/Pipeline/ -R
cp generate.sh Installer/
cp generate_deb.sh Installer/
cp ../../ThirdParty/Dependencies/makeself/. Installer/Makeself/ -R
cp ../../ThirdParty/Dependencies/assimp/libstdc++.so.6 Installer/Data/Main/
