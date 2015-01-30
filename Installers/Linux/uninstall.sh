#!/bin/sh
DIR=$(pwd)

if [ -f /bin/monogame-pipeline ]
then
	rm /bin/monogame-pipeline
fi

if [ -f /bin/mgcb ]
then
	rm /bin/mgcb
fi

if [ -f /usr/share/applications/Monogame\ Pipeline.desktop ]
then
	rm /usr/share/applications/Monogame\ Pipeline.desktop
fi

rm -rf $DIR
