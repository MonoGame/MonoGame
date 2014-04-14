#!/bin/bash

# Get the directory of the script itself
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

cd $DIR

sed -E '/{F759DE08-E160-4BB4-9A09-404D5694A4EC}/ {
    r MonoTouch-ProjectTypeGuids.txt
}' MonoGame.Tests.Assets.csproj > MonoGame.Tests.Assets.iOS.csproj
