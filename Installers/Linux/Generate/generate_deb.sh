#!/bin/bash

chmod +x Data/Main/mgcb
chmod +x Data/Main/monogame-pipeline
chmod +x Data/DEBIAN/postinst

mkdir tmp

cp Data/DEBIAN/. tmp/DEBIAN/ -R

mkdir tmp/opt
cp Data/Pipeline/. tmp/opt/monogame-pipeline/ -R
cp Data/Main/MonoDevelop.MonoGame.mpack tmp/opt/monogame-pipeline/

mkdir tmp/tmp
cp Data/Main/mgcb.xml tmp/tmp/
cp Data/Main/MonoDevelop.MonoGame.mpack tmp/tmp/

mkdir tmp/bin
cp Data/Main/mgcb tmp/bin/
cp Data/Main/monogame-pipeline tmp/bin/

mkdir tmp/usr
mkdir tmp/usr/share
mkdir tmp/usr/share/icons
mkdir tmp/usr/share/icons/gnome
mkdir tmp/usr/share/icons/gnome/scalable
mkdir tmp/usr/share/icons/gnome/scalable/mimetypes
cp Data/Main/monogame.svg tmp/usr/share/icons/gnome/scalable/mimetypes/

mkdir tmp/usr/lib
mkdir tmp/usr/lib/mono
mkdir tmp/usr/lib/mono/xbuild
mkdir tmp/usr/lib/mono/xbuild/MonoGame
mkdir tmp/usr/lib/mono/xbuild/MonoGame/v3.0
cp Data/Main/MonoGame.Content.Builder.targets tmp/usr/lib/mono/xbuild/MonoGame/v3.0/

mkdir tmp/lib
cp Data/Pipeline/libnvcore.so tmp/lib/
cp Data/Pipeline/libnvimage.so tmp/lib/
cp Data/Pipeline/libnvmath.so tmp/lib/
cp Data/Pipeline/libnvtt.so tmp/lib/

dpkg --build tmp monogame-sdk.deb
rm -rf tmp
