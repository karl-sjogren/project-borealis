#!/bin/bash

if [[ ! -f /sqlite/db/Borealis.db ]] ; then
    echo 'No database found, aborting.'
    exit 0
fi

pgloader --with "data only" --with "quote identifiers" --with "truncate" --after /sqlite/post-script.sql \
    sqlite:///sqlite/db/Borealis.db \
    postgresql://postgres:postgres@borealis-postgres2:5432/borealis

mv /sqlite/db/Borealis.db /sqlite/db/Borealis.db.imported
