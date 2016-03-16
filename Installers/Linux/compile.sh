#!/bin/bash
dpkg --build tmp_deb monogame-sdk.deb
./../../ThirdParty/Dependencies/makeself/makeself.sh tmp_run/ monogame-sdk.run "Monogame Pipeline Installer" ./postinstall.sh
