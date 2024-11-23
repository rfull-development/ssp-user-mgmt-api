#!/bin/bash
# Copyright (c) 2024 RFull Development
# This source code is managed under the MIT license. See LICENSE in the project root.

# Install development tools
# see https://www.postgresql.org/download/linux/ubuntu/
apt update -y
apt install -y curl ca-certificates

install -d /usr/share/postgresql-common/pgdg
curl -o /usr/share/postgresql-common/pgdg/apt.postgresql.org.asc --fail https://www.postgresql.org/media/keys/ACCC4CF8.asc
sh -c 'echo "deb [signed-by=/usr/share/postgresql-common/pgdg/apt.postgresql.org.asc] https://apt.postgresql.org/pub/repos/apt $(lsb_release -cs)-pgdg main" > /etc/apt/sources.list.d/pgdg.list'

apt update -y
apt install -y postgresql-client-16
exit 0
