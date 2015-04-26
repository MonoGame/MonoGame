#!/bin/sh

#check installation priviledge
if [ "$(id -u)" != "0" ]; then
	echo "Please make sure you are running this installer with sudo or as root." 1>&2
	exit 1
fi

#installation
DIR=$(pwd)
IDIR="/opt/monogame-pipeline"

read -p "The pipeline will be installed in $IDIR, ok(Y, n): " choice
case "$choice" in 
	n|N ) 
	echo "Type in the directory that will be used for installation:" 
	read IDIR
esac

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
if [ ! -f /lib/libnvcore.so ]
then
	ln $IDIR/libnvcore.so /lib/libnvcore.so
fi

if [ ! -f /lib/libnvimage.so ]
then
	ln $IDIR/libnvimage.so /lib/libnvimage.so
fi

if [ ! -f /lib/libnvmath.so ]
then
	ln $IDIR/libnvmath.so /lib/libnvmath.so
fi

if [ ! -f /lib/libnvtt.so ]
then
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
cp $DIR/Main/monogame.svg /usr/share/icons/gnome/scalable/mimetypes/monogame.svg
sudo gtk-update-icon-cache /usr/share/icons/gnome/ -f

#application launcher
if [ -f /usr/share/applications/Monogame\ Pipeline.desktop ]
then
	rm /usr/share/applications/Monogame\ Pipeline.desktop
fi
echo "[Desktop Entry]\nVersion=1.0\nEncoding=UTF-8\nName=MonoGame Pipeline\nGenericName=MonoGame Pipeline\nComment=\nExec=monogame-pipeline %F\nTryExec=monogame-pipeline\nIcon=monogame\nStartupNotify=false\nTerminal=false\nType=Application\nMimeType=text/mgcb;\nCategories=Development;" >> /usr/share/applications/Monogame\ Pipeline.desktop

#mimetype
echo "Adding mimetype..."
xdg-mime install $DIR/Main/mgcb.xml --novendor

#uninstall script
chmod +x $IDIR/uninstall.sh
echo "To uninstall the pipeline please run $IDIR/uninstall.sh"
