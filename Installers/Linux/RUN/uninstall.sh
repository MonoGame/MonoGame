#!/bin/bash

# Check removale priviledge
if [ "$(id -u)" != "0" ]; then
	echo "Please make sure you are running this uninstaller with sudo or as root." 1>&2
	exit 1
fi

# Remove terminal commands for mgcb and pipeline tool
rm -f /usr/bin/monogame-pipeline-tool
rm -f /usr/bin/monogame-uninstall
rm -f /usr/bin/mgcb
rm -f /etc/bash_completion.d/mgcb

# Remove application icon
rm -rf /usr/share/icons/gnome/scalable/mimetypes/monogame.svg

# Remove pipeline tool application launcher
rm -rf /usr/share/applications/Monogame\ Pipeline.desktop

# Remove mgcb mimetype
touch /opt/MonoGameSDK/x-mgcb.xml
xdg-mime uninstall /opt/MonoGameSDK/x-mgcb.xml

# Remove MonoGame SDK
rm -rf /usr/lib/mono/xbuild/MonoGame
rm -rf /opt/MonoGameSDK

# Remove man pages
IFS=':' read -r -a ARRAY <<< "$(manpath)"
for MANPATH in "${ARRAY[@]}"
do
	rm -rf "$MANPATH/man1/mgcb.1.gz"
done
