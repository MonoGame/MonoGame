#!/bin/bash

mkdir tmp

cp Data/DEBIAN/. tmp/DEBIAN/ -R

mkdir tmp/opt
cp Data/Pipeline/. tmp/opt/monogame-pipeline/ -R
cp Data/Main/mgcb.xml tmp/opt/monogame-pipeline/

mkdir tmp/lib
cp Data/Pipeline/libnvcore.so tmp/lib/
cp Data/Pipeline/libnvimage.so tmp/lib/
cp Data/Pipeline/libnvmath.so tmp/lib/
cp Data/Pipeline/libnvtt.so tmp/lib/

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

dpkg-deb --build tmp monogame-pipeline.deb
rm -rf tmp
