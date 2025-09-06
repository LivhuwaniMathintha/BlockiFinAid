#!/bin/bash
chown mongodb:mongodb /etc/mongo-keyfile
chmod 400 /etc/mongo-keyfile
exec docker-entrypoint.sh mongod --replSet rs0 --bind_ip_all --keyFile /etc/mongo-keyfile