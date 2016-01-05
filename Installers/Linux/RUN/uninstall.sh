#!/bin/sh

#remove terminal commands for mgcb and pipeline tool
if [ -f /bin/monogame-pipeline ]
then
	rm /bin/monogame-pipeline
fi

if [ -f /bin/mgcb ]
then
	rm /bin/mgcb
fi

#remove application icon
if [ -f /usr/share/icons/gnome/scalable/mimetypes/monogame.svg ]
then
	rm -rf /usr/share/icons/gnome/scalable/mimetypes/monogame.svg
fi

#remove pipeline tool application launcher
if [ -f /usr/share/applications/Monogame\ Pipeline.desktop ]
then
	rm /usr/share/applications/Monogame\ Pipeline.desktop
fi

#remove MonoGame xbuild data
if [ -d /usr/lib/mono/xbuild/MonoGame ]
then
	rm -rf /usr/lib/mono/xbuild/MonoGame
fi

#remove pipeline tool and self, the command for it is added by postinstall.sh
