#!/bin/bash

mkdir tmp

cp Data/Pipeline/. tmp/Pipeline/ -R
cp Data/Main/. tmp/Main/ -R
cp Data/RUN/postinstall.sh tmp/
cp Data/RUN/uninstall.sh tmp/Pipeline/

mkdir tmp/Dependencies
cp Data/RUN/Dependencies/. tmp/Dependencies -R

./Makeself/makeself.sh tmp/ monogame-linux.run "Monogame Pipeline Installer" ./postinstall.sh
rm -rf tmp
