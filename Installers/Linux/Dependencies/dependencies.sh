#!/bin/bash

read -p "Do you want to automatically install required packages(Y, n)" choice
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
	case "$VER" in 
	14.04|14.10 ) 
		./Dependencies/dependencies_deb.sh
		exit
	esac
fi

echo -e "${red}There is no automatic installer of dependencies for your version of linux, please look at http://www.monogame.net/documentation/?page=Setting_Up_MonoGame_Linux for information on how to install MonoGame.${NC}"
