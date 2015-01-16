#!/bin/bash
DIR="Data"

if [ ! -d "$DIR" ]
then
	mkdir "$DIR"
fi

cp ../../Tools/Pipeline/bin/Linux/AnyCPU/Release/. $DIR/ -R
cp ../monogame.ico $DIR
cp postinstall.sh $DIR
cp uninstall.sh $DIR
./../../ThirdParty/Dependencies/makeself/makeself.sh Data/ ../../monogame-linux.run "Monogame Pipeline Installer" ./postinstall.sh
rm -rf Data
