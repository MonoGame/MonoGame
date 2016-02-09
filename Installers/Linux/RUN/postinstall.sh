#!/bin/sh

#check installation priviledge
if [ "$(id -u)" != "0" ]; then
	echo "Please make sure you are running this installer with sudo or as root." 1>&2
	exit 1
fi

#check previous versions
if [ -f /bin/mgcb ]
then
	echo "Please uninstall any previous versions of MonoGame SDK" 1>&2
	exit 1
fi

#pipeline installation
DIR=$(pwd)
IDIR="/opt/monogame-pipeline"

rm -rf "$IDIR"

mkdir "$IDIR"
echo "Copying files..."

cp "$DIR/Pipeline/." "$IDIR/" -R

#automatic dependency installer
./Dependencies/dependencies.sh

#monodevelop addin
read -p "Install monodevelop addin(Y, n): " choice2
case "$choice2" in 
	n|N ) ;;
	*)
	sudo -H -u $SUDO_USER bash -c "mdtool setup install -y $DIR/Main/MonoDevelop.MonoGame.mpack"
esac

#MonoGame.xbuild data
rm -rf /usr/lib/mono/xbuild/MonoGame

mkdir -p /usr/lib/mono/xbuild/MonoGame/v3.0/Assemblies/
cp "$DIR/Assemblies/." /usr/lib/mono/xbuild/MonoGame/v3.0/Assemblies/ -R

sudo ln -s /opt/monogame-pipeline /usr/lib/mono/xbuild/MonoGame/v3.0/Tools
sudo cp $DIR/Main/MonoGame.Content.Builder.targets /usr/lib/mono/xbuild/MonoGame/v3.0/

echo "Creating launcher items..."

#monogame pipeline terminal command
rm -f /bin/monogame-pipeline
cp $DIR/Main/monogame-pipeline /bin/monogame-pipeline
chmod +x /bin/monogame-pipeline

#mgcb terminal command
rm -f /bin/mgcb
cp $DIR/Main/mgcb /bin/mgcb
chmod +x /bin/mgcb

#application/mimetype icon
mkdir -p /usr/share/icons/gnome/scalable/mimetypes

cp $DIR/Main/monogame.svg /usr/share/icons/gnome/scalable/mimetypes/monogame.svg

if [ -f /usr/share/icons/default/index.theme ]
then
	sudo gtk-update-icon-cache /usr/share/icons/default/ -f
else
	sudo gtk-update-icon-cache /usr/share/icons/gnome/ -f
fi

#application launcher
rm -f /usr/share/applications/Monogame\ Pipeline.desktop
echo -e "[Desktop Entry]\nVersion=1.0\nEncoding=UTF-8\nName=MonoGame Pipeline\nGenericName=MonoGame Pipeline\nComment=Used to create platform specific .xnb files\nExec=monogame-pipeline %F\nTryExec=monogame-pipeline\nIcon=monogame\nStartupNotify=true\nTerminal=false\nType=Application\nMimeType=text/mgcb;\nCategories=Development;" | sudo tee --append /usr/share/applications/Monogame\ Pipeline.desktop > /dev/null

#mimetype
echo "Adding mimetype..."
xdg-mime install $DIR/Main/mgcb.xml --novendor
xdg-mime default "Monogame Pipeline.desktop" text/mgcb

#uninstall script
chmod +x $IDIR/uninstall.sh
echo "To uninstall the pipeline please run $IDIR/uninstall.sh"

