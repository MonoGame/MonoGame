#!/bin/bash
DIR="Data"

if [ ! -d "$DIR" ]
then
	mkdir "$DIR"
fi

cp ../../Tools/Pipeline/bin/Linux/AnyCPU/Release/. $DIR/ -R

rm $DIR/atk-sharp.dll
rm $DIR/atk-sharp.dll.config
rm $DIR/gdk-sharp.dll
rm $DIR/gdk-sharp.dll.config
rm $DIR/glade-sharp.dll
rm $DIR/glade-sharp.dll.config
rm $DIR/glib-sharp.dll
rm $DIR/glib-sharp.dll.config
rm $DIR/gtk-dotnet.dll
rm $DIR/gtk-sharp.dll
rm $DIR/gtk-sharp.dll.config
rm $DIR/Mono.Posix.dll
rm $DIR/pango-sharp.dll
rm $DIR/pango-sharp.dll.config

cp ../monogame.ico $DIR
cp postinstall.sh $DIR
cp uninstall.sh $DIR
./../../ThirdParty/Dependencies/makeself/makeself.sh Data/ ../../monogame-linux.run "Monogame Pipeline Installer" ./postinstall.sh
rm -rf Data
