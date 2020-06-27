#!/bin/bash

exdir=$(dirname `readlink -f "$0"`)

which rsync
if [ "$?" != "0" ]; then
    echo "missing rsync command"
    exit 1
fi

if [ ! -e "$exdir/../Avalonia" ]; then
    echo "please clone https://github.com/SearchAThing-forks/Avalonia into $exdir/../Avalonia"
    exit 1
fi

cd "$exdir/../Avalonia"
git checkout 72aa864b435e1082cffa018a45d62f8c27323cd0
./build.sh Package

if [ "$?" != "0" ]; then
    echo "error occurred during build avalonia"
    exit 1
fi

rsync -arvx --delete "$exdir/../Avalonia/artifacts/nuget/" "$exdir/avalonia-0.9.999/"
