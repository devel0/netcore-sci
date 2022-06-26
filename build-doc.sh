#!/bin/bash

exdir="$(dirname `readlink -f "$0"`)"

DOCSDIR="$exdir/docs"

rm -fr "$DOCSDIR"

mkdir "$DOCSDIR"

cd "$exdir"

doxygen

rsync -arvx "$exdir/test/" "$DOCSDIR/test/" \
    --exclude=bin \
    --exclude=obj \
    --exclude=201733116857084983789125918513951894152.xlsx

# mkdir -p "$DOCSDIR/data/techdocs"

# rsync -arvx "$exdir/data/techdocs/" "$DOCSDIR/data/techdocs/" \
#     --exclude=bin \
#     --exclude=obj \
#     --exclude=201733116857084983789125918513951894152.xlsx