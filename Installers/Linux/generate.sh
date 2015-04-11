#!/bin/bash

PD="Data/Pipeline"

if [ -d $PD ]
then
	rm -rf $PD
fi

cp -r Pipeline.Gtk2/ $PD
./Makeself/makeself.sh Data/ monogame-linux.run "Monogame Pipeline Installer" ./postinstall.sh
rm -rf $PD

cp -r Pipeline.Gtk3/ $PD
./Makeself/makeself.sh Data/ monogame-linux-gtk3.run "Monogame Pipeline Installer" ./postinstall.sh
rm -rf $PD
