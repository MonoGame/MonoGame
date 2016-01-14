#!/bin/bash
find tmp_deb/ -exec chown 1000:1000 {} \;
dpkg --build tmp_deb monogame-sdk.deb
./../../ThirdParty/Dependencies/makeself/makeself.sh tmp_run/ monogame-sdk.run "Monogame Pipeline Installer" ./postinstall.sh
