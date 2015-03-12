#!/bin/bash
DIR="Data"
PDIR="$DIR/Pipeline"
DDIR="$DIR/Dependencies"

#create temp directories for generating the installer
if [ ! -d "$DIR" ]
then
	mkdir "$DIR"
fi

if [ ! -d "$PDIR" ]
then
	mkdir "$PDIR"
fi

if [ ! -d "$DDIR" ]
then
	mkdir "$DDIR"
fi

#copy pipeline data
cp ../../Tools/Pipeline/bin/Linux/AnyCPU/Release/. $PDIR/ -R
cp ../monogame.ico $PDIR
cp uninstall.sh $PDIR

#remove gtk libraries so that the pipeline tool would use system libraries instead
rm $PDIR/atk-sharp.dll
rm $PDIR/atk-sharp.dll.config
rm $PDIR/gdk-sharp.dll
rm $PDIR/gdk-sharp.dll.config
rm $PDIR/glade-sharp.dll
rm $PDIR/glade-sharp.dll.config
rm $PDIR/glib-sharp.dll
rm $PDIR/glib-sharp.dll.config
rm $PDIR/gtk-dotnet.dll
rm $PDIR/gtk-sharp.dll
rm $PDIR/gtk-sharp.dll.config
rm $PDIR/Mono.Posix.dll
rm $PDIR/pango-sharp.dll
rm $PDIR/pango-sharp.dll.config

#copy the scripts thats gonna be doing the actual install
cp postinstall.sh $DIR
cp Dependencies/. $DDIR/ -R

#copy mimetype stuff
cp mgcb.xml $DIR

#build the installer
./../../ThirdParty/Dependencies/makeself/makeself.sh Data/ monogame-linux.run "Monogame Pipeline Installer" ./postinstall.sh

#remove temp directory
rm -rf Data
