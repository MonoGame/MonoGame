#!/bin/bash

#check removale priviledge
if [ "$(id -u)" != "0" ]; then
	echo "Please make sure you are running this uninstaller with sudo or as root." 1>&2
	exit 1
fi

#remove terminal commands for mgcb and pipeline tool
rm -f /usr/bin/monogame-pipeline-tool
rm -f /usr/bin/monogame-uninstall
rm -f /usr/bin/mgcb
rm -f /etc/bash_completion.d/mgcb

#remove application icon
rm -rf /usr/share/icons/gnome/scalable/mimetypes/monogame.svg

#remove pipeline tool application launcher
rm -rf /usr/share/applications/Monogame\ Pipeline.desktop

#remove MonoGame SDK
rm -rf /usr/lib/mono/xbuild/MonoGame
rm -rf /opt/MonoGameSDK

# Remove man pages
IFS=':' read -r -a ARRAY <<< "$(manpath)"
for MANPATH in "${ARRAY[@]}"
do
	rm -rf "$MANPATH/man1/mgcb.1.gz"
done
