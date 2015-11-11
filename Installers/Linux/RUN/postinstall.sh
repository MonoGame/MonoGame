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

if [ -d "$IDIR" ]
then
	rm -rf "$IDIR"
fi

mkdir "$IDIR"
echo "Copying files..."

cp "$DIR/Pipeline/." "$IDIR/" -R
echo "rm -rf $IDIR" >> $IDIR/uninstall.sh

#automatic dependency installer
./Dependencies/dependencies.sh

#setup nvtt libraries
if [ ! -f /lib/libnvcore.so ]; then
	ln $IDIR/libnvcore.so /lib/libnvcore.so
fi

if [ ! -f /lib/libnvimage.so ]; then
	ln $IDIR/libnvimage.so /lib/libnvimage.so
fi

if [ ! -f /lib/libnvmath.so ]; then
	ln $IDIR/libnvmath.so /lib/libnvmath.so
fi

if [ ! -f /lib/libnvtt.so ]; then
	ln $IDIR/libnvtt.so /lib/libnvtt.so
fi

#check GLIBCXX_3.4.20 support
if [ -f /usr/lib/x86_64-linux-gnu/libstdc++.so.6 ]
then
	GREP=$(strings /usr/lib/x86_64-linux-gnu/libstdc++.so.6 | grep GLIBCXX_3.4.20)
	size=${#GREP} 

	if [ ! $size -gt 0 ] 
	then
		echo "Your libstdc++.so.6 does not support GLIBCXX_3.4.20. Want to copy newer version of it?"
		echo "Old version will be copied renamed to libstdc++.so.6.old"
		read -p "(Y, n): " choice
	
		case "$choice" in 
			n|N ) ;;
			*)
			sudo mv /usr/lib/x86_64-linux-gnu/libstdc++.so.6 /usr/lib/x86_64-linux-gnu/libstdc++.so.6.old
			sudo cp $DIR/Main/libstdc++.so.6 /usr/lib/x86_64-linux-gnu/libstdc++.so.6
		esac
	fi
fi

#monodevelop addin
read -p "Install monodevelop addin(Y, n): " choice2
case "$choice2" in 
	n|N ) ;;
	*)
	sudo -H -u $SUDO_USER bash -c "mdtool setup install $DIR/Main/MonoDevelop.MonoGame.mpack"
esac

#MonoGame.xbuild data
if [ -d /usr/lib/mono/xbuild/MonoGame ]; then
	rm -rf /usr/lib/mono/xbuild/MonoGame
fi

mkdir /usr/lib/mono/xbuild/MonoGame
mkdir /usr/lib/mono/xbuild/MonoGame/v3.0

mkdir /usr/lib/mono/xbuild/MonoGame/v3.0/Assemblies/
cp "$DIR/Assemblies/." /usr/lib/mono/xbuild/MonoGame/v3.0/Assemblies/ -R

sudo ln -s /opt/monogame-pipeline /usr/lib/mono/xbuild/MonoGame/v3.0/Tools

sudo cp $DIR/Main/MonoGame.Content.Builder.targets /usr/lib/mono/xbuild/MonoGame/v3.0/

#fix permissions
usr="$SUDO_USER"
if [ -z "$usr" -a "$usr"==" " ]; then
	usr="$USERNAME"
fi
sudo chown -R "$usr" "$IDIR/"

echo "Creating launcher items..."

#monogame pipeline terminal command
if [ -f /bin/monogame-pipeline ]
then
	rm /bin/monogame-pipeline
fi
cp $DIR/Main/monogame-pipeline /bin/monogame-pipeline
chmod +x /bin/monogame-pipeline

#mgcb terminal command
if [ -f /bin/mgcb ]
then
	rm /bin/mgcb
fi
cp $DIR/Main/mgcb /bin/mgcb
chmod +x /bin/mgcb

#application/mimetype icon
if [ ! -d /usr/share/icons/gnome/scalable/mimetypes ]
then
	mkdir /usr/share/icons/gnome/scalable/mimetypes
fi

cp $DIR/Main/monogame.svg /usr/share/icons/gnome/scalable/mimetypes/monogame.svg

if [ -f /usr/share/icons/default/index.theme ]
then
	sudo gtk-update-icon-cache /usr/share/icons/default/ -f
else
	sudo gtk-update-icon-cache /usr/share/icons/gnome/ -f
fi

#application launcher
if [ -f /usr/share/applications/Monogame\ Pipeline.desktop ]
then
	rm /usr/share/applications/Monogame\ Pipeline.desktop
fi
echo -e "[Desktop Entry]\nVersion=1.0\nEncoding=UTF-8\nName=MonoGame Pipeline\nGenericName=MonoGame Pipeline\nComment=Used to create platform specific .xnb files\nExec=monogame-pipeline %F\nTryExec=monogame-pipeline\nIcon=monogame\nStartupNotify=true\nTerminal=false\nType=Application\nMimeType=text/mgcb;\nCategories=Development;" | sudo tee --append /usr/share/applications/Monogame\ Pipeline.desktop > /dev/null

#mimetype
echo "Adding mimetype..."
xdg-mime install $DIR/Main/mgcb.xml --novendor
xdg-mime default "Monogame Pipeline.desktop" text/mgcb

#uninstall script
chmod +x $IDIR/uninstall.sh
echo "To uninstall the pipeline please run $IDIR/uninstall.sh"
