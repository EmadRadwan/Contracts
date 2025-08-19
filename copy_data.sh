#!/bin/bash

# Define the backup directory
BACKUP_DIR=/Users/emadradwan/Documents/ofbiz_runtime

# Remove old backup data
rm -rf "${BACKUP_DIR:?}"/*

# Copy data from the new container volume to host
docker run --rm -v ofbiz_runtime_jdk11-5:/ofbiz/runtime -v "${BACKUP_DIR:?}":/backup busybox cp -r /ofbiz/runtime/. /backup/

echo "Data copy completed."
