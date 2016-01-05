#!/bin/bash

read -p "Do you want to automatically install required packages(Y, n): " choice
case "$choice" in 
	n|N ) exit;
esac

#check linux version and execute appropriate script
OS=$(lsb_release -si)
VER=$(lsb_release -sr)

red='\033[0;31m'
NC='\033[0m'

if [ $OS = Ubuntu ]
then
	if [ ${VER%.*} -ge 14 ]
	then
		./Dependencies/dependencies_deb.sh
		exit
	fi
fi

if [ $OS = Fedora ]
then
	if [ $VER -ge 23 ]
	then
		./Dependencies/dependencies_fedora.sh
		exit
	fi
fi

echo -e "${red}There is no automatic installer of dependencies for your version of linux, please look at http://www.monogame.net/documentation/?page=Setting_Up_MonoGame_Linux for information on how to install MonoGame.${NC}"
