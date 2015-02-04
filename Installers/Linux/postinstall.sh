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
  n|N ) echo "Type in the directory that will be used for installation:" 
		read IDIR
esac

if [ -d "$IDIR" ]
then
	rm -rf "$IDIR"
fi

mkdir "$IDIR"
echo "Copying files..."

cp "$DIR/." "$IDIR/" -R

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
echo "#!/bin/bash\nmono $IDIR/Pipeline.exe \"\$@\"" >> /bin/monogame-pipeline
chmod +x /bin/monogame-pipeline

#mgcb terminal command
if [ -f /bin/mgcb ]
then
	rm /bin/mgcb
fi
echo "#!/bin/bash\nmono $IDIR/MGCB.exe \"\$@\"" >> /bin/mgcb
chmod +x /bin/mgcb

#application icon
if [ -f /usr/share/applications/Monogame\ Pipeline.desktop ]
then
	rm /usr/share/applications/Monogame\ Pipeline.desktop
fi
echo "[Desktop Entry]\nVersion=1.0\nEncoding=UTF-8\nName=MonoGame Pipeline\nGenericName=MonoGame Pipeline\nComment=\nExec=monogame-pipeline %F\nTryExec=monogame-pipeline\nIcon=$IDIR/monogame.ico\nStartupNotify=false\nTerminal=false\nType=Application\nMimeType=text/mgcb;text/plain;\nCategories=Development;" >> /usr/share/applications/Monogame\ Pipeline.desktop

#mimetype
echo "Adding mimetype..."

GREP=$(grep "text/mgcb" /etc/mime.types)
size=${#GREP} 

if [ ! $size -gt 0 ] 
then
	echo "text/mgcb \t\t\t\tmgcb" >> /etc/mime.types
fi

#uninstall script
chmod +x $IDIR/uninstall.sh
echo "To uninstall the pipeline please run $IDIR/uninstall.sh"
