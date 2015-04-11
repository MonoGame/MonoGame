#!/bin/bash
DIR="Installer/Data"
MDIR="Installer/Makeself"
PDIR2="Installer/Pipeline.Gtk2"
PDIR3="Installer/Pipeline.Gtk3"
DDIR="$DIR/Dependencies"

#create temp directories for generating the installer
mkdir "Installer"
mkdir "$DIR"
mkdir "$MDIR"
mkdir "$PDIR2"
mkdir "$PDIR3"
mkdir "$DDIR"

#copy pipeline data
cp ../../Tools/Pipeline/bin/Linux/AnyCPU/Release/. $PDIR2/ -R
cp ../../Tools/Pipeline/bin/Pipeline.Gtk3/Linux/AnyCPU/Release/. $PDIR3/ -R
mv $PDIR3/Pipeline.Gtk3.exe $PDIR3/Pipeline.exe
cp ../monogame.ico $PDIR2
cp ../monogame.ico $PDIR3
cp uninstall.sh $PDIR2
cp uninstall.sh $PDIR3

#remove gtk2 libraries so that the pipeline tool would use system libraries instead
rm $PDIR2/atk-sharp.dll
rm $PDIR2/atk-sharp.dll.config
rm $PDIR2/gdk-sharp.dll
rm $PDIR2/gdk-sharp.dll.config
rm $PDIR2/glade-sharp.dll
rm $PDIR2/glade-sharp.dll.config
rm $PDIR2/glib-sharp.dll
rm $PDIR2/glib-sharp.dll.config
rm $PDIR2/gtk-dotnet.dll
rm $PDIR2/gtk-sharp.dll
rm $PDIR2/gtk-sharp.dll.config
rm $PDIR2/libatksharpglue-2.so
rm $PDIR2/libgdksharpglue-2.so
rm $PDIR2/libgladesharpglue-2.so
rm $PDIR2/libglibsharpglue-2.so
rm $PDIR2/libgtksharpglue-2.so
rm $PDIR2/libpangosharpglue-2.so
rm $PDIR2/Mono.Posix.dll
rm $PDIR2/pango-sharp.dll
rm $PDIR2/pango-sharp.dll.config

#copy the scripts thats gonna be doing the actual install
cp generate.sh "Installer"
cp postinstall.sh $DIR
cp Dependencies/. $DDIR/ -R

#copy mimetype stuff
cp mgcb.xml $DIR

cp ../../ThirdParty/Dependencies/makeself/. $MDIR -R

cp ../../ThirdParty/Dependencies/assimp/libstdc++.so.6 $DIR
