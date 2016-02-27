#!/bin/sh

# Check installation priviledge
if [ "$(id -u)" != "0" ]; then
	echo "Please make sure you are running this installer with sudo or as root." 1>&2
	exit 1
fi

# Check previous versions
if type "mgcb" > /dev/null 2>&1
then
	echo "Please uninstall any previous versions of MonoGame SDK" 1>&2
	exit 1
fi

DIR=$(pwd)
IDIR="/usr/lib/mono/xbuild/MonoGame/v3.0"

# Show dependency list
echo "Please make sure the following packages are installed:"
echo " - monodevelop"
echo " - libopenal-dev"
echo " - referenceassemblies-pcl / mono-pcl"
echo " - ttf-mscorefonts-installer / mscore-fonts"
echo " - gtk-sharp3"
read -p "Continue (Y, n): " choice2
case "$choice2" in 
	n|N ) exit ;;
	*) ;;
esac

# MonoDevelop addin
read -p "Install monodevelop addin(Y, n): " choice2
case "$choice2" in 
	n|N ) ;;
	*)
	sudo -H -u $SUDO_USER bash -c "mdtool setup install -y $DIR/Main/MonoDevelop.MonoGame.mpack"
esac

# MonoGame SDK installation
echo "Installing MonoGame SDK..."

rm -rf "$IDIR"
mkdir -p "$IDIR"
cp -rf "$DIR/MonoGameSDK/." "$IDIR" -R
rm -rf "/opt/MonoGameSDK"
ln -s "$IDIR" "/opt/MonoGameSDK"

# Fix Permissions
chmod +x "$IDIR/Tools/ffmpeg"
chmod +x "$IDIR/Tools/ffprobe"

# Monogame Pipeline terminal commands
echo "Creating launcher items..."

cp $DIR/Main/monogame-pipeline /usr/bin/monogame-pipeline
chmod +x /usr/bin/monogame-pipeline

cp $DIR/Main/mgcb /usr/bin/mgcb
chmod +x /usr/bin/mgcb

# MonoGame icon
mkdir -p /usr/share/icons/hicolor/scalable/mimetypes
cp $DIR/Main/monogame.svg /usr/share/icons/hicolor/scalable/mimetypes/monogame.svg
gtk-update-icon-cache /usr/share/icons/hicolor/ -f

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

