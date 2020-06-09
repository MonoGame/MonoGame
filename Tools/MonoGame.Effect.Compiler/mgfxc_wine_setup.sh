#!/bin/bash
# This script is used to setup the needed Wine environment
# so that mgfxc can be run on Linux / macOS systems.

# check dependencies
if ! type "wine64" > /dev/null 2>&1
then
    echo "wine64 not found"
    exit 1
fi

if ! type "7z" > /dev/null 2>&1
then
    echo "7z not found"
    exit 1
fi

# init wine stuff
export WINEARCH=win64
export WINEPREFIX=$HOME/.winemonogame
wine64 wineboot

TEMP_DIR="${TMPDIR:-/tmp}"
SCRIPT_DIR="$TEMP_DIR/winemg2"
mkdir -p "$SCRIPT_DIR"

# get dotnet
DOTNET_URL="https://download.visualstudio.microsoft.com/download/pr/adeab8b1-1c44-41b2-b12a-156442f307e9/65ebf805366410c63edeb06e53959383/dotnet-sdk-3.1.201-win-x64.zip"
curl $DOTNET_URL --output "$SCRIPT_DIR/dotnet-sdk.zip"
7z x "$SCRIPT_DIR/dotnet-sdk.zip" -o"$WINEPREFIX/drive_c/windows/system32/"

# get d3dcompiler_47
FIREFOX_URL="https://download-installer.cdn.mozilla.net/pub/firefox/releases/62.0.3/win64/ach/Firefox%20Setup%2062.0.3.exe"
curl $FIREFOX_URL --output "$SCRIPT_DIR/firefox.exe"
7z x "$SCRIPT_DIR/firefox.exe" -o"$SCRIPT_DIR/firefox_data/"
cp -f "$SCRIPT_DIR/firefox_data/core/d3dcompiler_47.dll" "$WINEPREFIX/drive_c/windows/system32/d3dcompiler_47.dll"

# append MGFXC_WINE_PATH env variable
echo "export MGFXC_WINE_PATH=$HOME/.winemonogame" >> ~/.profile
echo "export MGFXC_WINE_PATH=$HOME/.winemonogame" >> ~/.zprofile

# cleanup
rm -rf "$SCRIPT_DIR"
