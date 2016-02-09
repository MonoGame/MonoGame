#!/bin/sh

# Check installation priviledge
if [ "$(id -u)" != "0" ]; then
	echo "Please make sure you are running this installer with sudo or as root." 1>&2
	exit 1
fi

# Check previous versions
if [ -f /bin/mgcb ]
then
	echo "Please uninstall any previous versions of MonoGame SDK" 1>&2
	exit 1
fi

DIR=$(pwd)
IDIR="/usr/lib/mono/xbuild/MonoGame/v3.0"

# Automatic dependency installer
./Dependencies/dependencies.sh

# MonoDevelop addin
read -p "Install monodevelop addin(Y, n): " choice2
case "$choice2" in 
	n|N ) ;;
	*)
	sudo -H -u $SUDO_USER bash -c "mdtool setup install -y $DIR/Main/MonoDevelop.MonoGame.mpack"
esac

# Pipeline Tool installation
echo "Installing SDK..."

rm -rf "$IDIR"
mkdir -p "$IDIR"
cp -rf "$DIR/MonoGameSDK/." "$IDIR" -R
rm -rf "/opt/MonoGameSDK"
ln -s "$IDIR" "/opt/MonoGameSDK"

echo "Creating launcher items..."

# Monogame Pipeline terminal commands
rm -f /bin/monogame-pipeline
cp $DIR/Main/monogame-pipeline /bin/monogame-pipeline
chmod +x /bin/monogame-pipeline

rm -f /bin/mgcb
cp $DIR/Main/mgcb /bin/mgcb
chmod +x /bin/mgcb

# MonoGame icon
mkdir -p /usr/share/icons/gnome/scalable/mimetypes

cp $DIR/Main/monogame.svg /usr/share/icons/gnome/scalable/mimetypes/monogame.svg

if [ -f /usr/share/icons/default/index.theme ]
then
	sudo gtk-update-icon-cache /usr/share/icons/default/ -f
else
	sudo gtk-update-icon-cache /usr/share/icons/gnome/ -f
fi

# Application launcher
rm -f /usr/share/applications/Monogame\ Pipeline.desktop
echo -e "[Desktop Entry]\nVersion=1.0\nEncoding=UTF-8\nName=MonoGame Pipeline\nGenericName=MonoGame Pipeline\nComment=Used to create platform specific .xnb files\nExec=monogame-pipeline %F\nTryExec=monogame-pipeline\nIcon=monogame\nStartupNotify=true\nTerminal=false\nType=Application\nMimeType=text/mgcb;\nCategories=Development;" | sudo tee --append /usr/share/applications/Monogame\ Pipeline.desktop > /dev/null

# Mimetype
echo "Adding mimetype..."
xdg-mime install $DIR/Main/mgcb.xml --novendor
xdg-mime default "Monogame Pipeline.desktop" text/mgcb

# Uninstall script
chmod +x $IDIR/uninstall.sh
echo "To uninstall the pipeline please run $IDIR/uninstall.sh"

