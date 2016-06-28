#!/bin/bash

# Functions
echodep()
{
	line=" - $1"
	
	while [ ${#line} -lt 50 ]
	do
		line="$line."
	done
    
	if eval "$2"
	then
		line="$line\e[32m[Found]\e[0m"
	else
		line="$line\e[31m[Not Found]\e[0m"
	fi
	
	echo -e "$line"
}

# Check installation priviledge
if [ "$(id -u)" != "0" ]; then
	echo "Please make sure you are running this installer with sudo or as root." 1>&2
	exit 1
fi

# Check previous versions
if type "mgcb" > /dev/null 2>&1
then
	echo "Previous version detected, trying to uninstall..."
	
	# Try and uninstall previus versions
	if [ -f /opt/monogame/uninstall.sh ]
	then
		sudo sh /opt/monogame/uninstall.sh
	elif [ -f /opt/MonoGameSDK/uninstall.sh ]
	then
		sudo sh /opt/MonoGameSDK/uninstall.sh
	else
		echo "Could not uninstall, please uninstall any previous version of MonoGame SDK manually." 1>&2
		exit 1
	fi
fi

DIR=$(pwd)
IDIR="/usr/lib/mono/xbuild/MonoGame/v3.0"

# Show dependency list
echo "Dependencies:"
echodep "mono-runtime" "type 'mono' > /dev/null 2>&1"
echodep "libopenal" "ldconfig -p | grep -q libopenal"
echodep "gtk-sharp3" "type 'gacutil' > /dev/null 2>&1 && gacutil /l gtk-sharp | grep -q 3.0.0.0"
echo ""
echo "Optional Dependencies:"
echodep "monodevelop" "type 'mdtool' > /dev/null 2>&1"
echodep "rider" "type 'rider' > /dev/null 2>&1"
echodep "referenceassemblies-pcl / mono-pcl" "test -d /usr/lib/mono/xbuild/Microsoft/Portable"
echodep "ttf-mscorefonts-installer / mscore-fonts" "fc-list | grep -q Arial"
echo ""
read -p "Continue (Y, n): " choice2
case "$choice2" in 
	n|N ) exit ;;
	*) ;;
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

# Rider stuff
if type "rider" > /dev/null 2>&1
then
	echo "Installing Rider files..."
	
	FINDCOMMAND=$(type -a rider)
	COMMAND=$(echo $FINDCOMMAND| cut -d' ' -f 3)
	
	FINDRIDER=$(cat $COMMAND | grep "RUN_PATH")
	RIDER=$(echo $FINDRIDER| cut -d"'" -f 2)
	
	RIDERDIR=$(dirname $(dirname $RIDER))
	RXBUILD="$RIDERDIR/lib/ReSharperHost/linux-x64/mono/lib/mono/xbuild/MonoGame"
	
	mkdir -p "$RXBUILD"
	ln -s "$IDIR" "$RXBUILD/v3.0"
fi

# MonoDevelop addin
if type "mdtool" > /dev/null 2>&1
then
	echo "Installing MonoDevelop Addin..."
	sudo -H -u $SUDO_USER bash -c "mdtool setup install -y $DIR/Main/MonoDevelop.MonoGame.mpack  > /dev/null"
fi

# Monogame Pipeline terminal commands
echo "Creating launcher items..."

cat > /usr/bin/monogame-pipeline-tool <<'endmsg'
#!/bin/bash
mono /usr/lib/mono/xbuild/MonoGame/v3.0/Tools/Pipeline.exe "$@"
endmsg
chmod +x /usr/bin/monogame-pipeline-tool

cat > /usr/bin/mgcb <<'endmsg'
#!/bin/bash
mono /usr/lib/mono/xbuild/MonoGame/v3.0/Tools/MGCB.exe "$@"
endmsg
chmod +x /usr/bin/mgcb

# MonoGame icon
mkdir -p /usr/share/icons/hicolor/scalable/mimetypes
cp $DIR/Main/monogame.svg /usr/share/icons/hicolor/scalable/mimetypes/monogame.svg
gtk-update-icon-cache /usr/share/icons/hicolor/ -f

# Application launcher
cat > /usr/share/applications/MonogamePipeline.desktop <<'endmsg'
[Desktop Entry]
Version=1.0
Encoding=UTF-8
Name=MonoGame Pipeline Tool
GenericName=MonoGame Pipeline Tool
Comment=Creates platform specific content files.
Exec=monogame-pipeline-tool %F
TryExec=monogame-pipeline-tool
Icon=monogame
StartupNotify=true
Terminal=false
Type=Application
MimeType=text/mgcb;
Categories=Development;
endmsg

# Mimetype
echo "Adding mimetype..."
xdg-mime install $DIR/Main/mgcb.xml --novendor > /dev/null
xdg-mime default "MonogamePipeline.desktop" text/mgcb

# Uninstall script
chmod +x $IDIR/uninstall.sh
ln -s $IDIR/uninstall.sh /usr/bin/monogame-uninstall

echo "To uninstall MonoGame SDK you can run \"monogame-uninstall\" from terminal."
