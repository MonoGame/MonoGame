#!/bin/sh

# !!! FIXME: use this to correct our estimates some day.

for feh in shaders/??_?_?/*.bytecode ; do
    DISASM=`echo $feh |perl -w -p -e 's/bytecode\Z/disasm/;'`
    MINE=`./cmake-build/testparse d3d $feh |grep "INSTRUCTION COUNT: " |perl -w -p -e 's/\AINSTRUCTION COUNT: //;'`
    THEIRS=`grep "instruction slots used" $DISASM |perl -w -p -e 's#\A// approximately (\d+) instruction slots used .*?\Z#$1#;'`
    if [ "x$MINE" != "x$THEIRS" ]; then
        echo "$feh $MINE vs. $THEIRS"
    fi
done

