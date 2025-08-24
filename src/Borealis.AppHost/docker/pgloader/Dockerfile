# Script to migrate old sqlite database to postgres
FROM dimitri/pgloader:latest AS pgloader
WORKDIR /sqlite

COPY migrate.sh migrate.sh
COPY post-script.sql post-script.sql

RUN chmod +x /sqlite/migrate.sh

SHELL ["/bin/bash", "-c"]
ENTRYPOINT [ "/sqlite/migrate.sh" ]
