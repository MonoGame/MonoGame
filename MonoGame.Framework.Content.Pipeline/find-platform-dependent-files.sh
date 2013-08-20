#!/bin/bash

CSFILES=$(find ./ -name \*.cs | sort)
PLATFORMS="Linux MacOS Windows"

for ocs in $CSFILES; do
  ocs=${ocs:2}
  cs=$ocs
  is=$ocs
  while [[ "$cs" == *"/"* ]]; do
    cs=${cs/\//\\\\}
  done
  while [[ "$is" == *"/"* ]]; do
    is=${is/\//\\}
  done
  notin=" "
  for platform in $PLATFORMS; do
    result=$(grep "\"$cs" MonoGame.Framework.Content.Pipeline.$platform.csproj)
    if [ "$result" == "" ]; then
      notin="$notin$platform "
    fi
  done
  if [ "$notin" == " " ]; then
    echo "<Compile Include=\"${is}\" />"
  else
    platresult=""
    for platform in $PLATFORMS; do
      if [[ "$notin" == *" $platform "* ]]; then
        # do nothing
        true
      else
        if [ "$platresult" == "" ]; then
          platresult="$platform"
        else
          platresult="$platresult,$platform"
        fi
      fi
    done
    echo "<Compile Include=\"${is}\">"
    echo "  <Platforms>$platresult</Platforms>"
    echo "</Compile>"
  fi
done
