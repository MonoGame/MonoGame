#!/bin/bash
DIR="Installer/Data"
MDIR="Installer/Makeself"
PDIR="Installer/Data/Pipeline"
DDIR="$DIR/Dependencies"

#create temp directories for generating the installer
mkdir "Installer"
mkdir "$DIR"
mkdir "$MDIR"
mkdir "$PDIR"
mkdir "$DDIR"

#copy pipeline data
cp ../../Tools/Pipeline/bin/Linux/AnyCPU/Release/. $PDIR/ -R
cp ../monogame.ico $PDIR
cp uninstall.sh $PDIR

#copy the scripts thats gonna be doing the actual install
cp generate.sh "Installer"
cp postinstall.sh $DIR
cp Dependencies/. $DDIR/ -R

#copy mimetype stuff
cp mgcb.xml $DIR

cp ../../ThirdParty/Dependencies/makeself/. $MDIR -R
cp ../../ThirdParty/Dependencies/assimp/libstdc++.so.6 $DIR
