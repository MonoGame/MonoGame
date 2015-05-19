#!/bin/bash

chmod +x Data/Main/mgcb
chmod +x Data/Main/monogame-pipeline
chmod +x Data/RUN/postinstall.sh
chmod +x Data/RUN/uninstall.sh
chmod +x Data/RUN/Dependencies/dependencies*
chmod +x Makeself/makeself.sh

mkdir tmp

cp Data/Pipeline/. tmp/Pipeline/ -R
cp Data/Main/. tmp/Main/ -R
cp Data/RUN/postinstall.sh tmp/
cp Data/RUN/uninstall.sh tmp/Pipeline/

mkdir tmp/Dependencies
cp Data/RUN/Dependencies/. tmp/Dependencies -R

./Makeself/makeself.sh tmp/ monogame-linux.run "Monogame Pipeline Installer" ./postinstall.sh
rm -rf tmp
