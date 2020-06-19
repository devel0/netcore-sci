#!/bin/bash

exdir=$(dirname `readlink -f "$0"`)

cd "$exdir"
git submodule init
git submodule update opentk

cd opentk
./build.sh
