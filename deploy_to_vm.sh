#!/bin/bash

set -e  # Stop on first error

# VM Details
VM_USER="ubuntu"
VM_HOST="129.146.22.240"
PRIVATE_KEY="$HOME/.ssh/id_rsa"
VM_DEST_PATH="/home/ubuntu/erp-project"

# SSH options
SSH_OPTIONS="-i $PRIVATE_KEY -o StrictHostKeyChecking=no"

# List of required folders/files
INCLUDE_FILES=(
  "BusinessOne.sln"
  "Dockerfile.vm"
  "docker-compose.vm.yml"
  "API"
  "Application"
  "Infrastructure"
  "Domain"
  "Persistence"
)

echo "🚀 Starting Deployment to VM ($VM_HOST)..."

# Stop and remove Docker Compose services to release file locks
echo "🛑 Stopping and removing Docker Compose services..."
ssh $SSH_OPTIONS "$VM_USER@$VM_HOST" <<EOF
  cd $VM_DEST_PATH
  if [ -f docker-compose.vm.yml ]; then
    sudo docker-compose -f docker-compose.vm.yml down
  fi
EOF

# Delete the entire remote project folder with sudo
echo "🧹 Deleting remote project folder..."
ssh $SSH_OPTIONS "$VM_USER@$VM_HOST" "sudo rm -rf $VM_DEST_PATH"

# Ensure destination directory exists on the VM
ssh $SSH_OPTIONS "$VM_USER@$VM_HOST" "mkdir -p $VM_DEST_PATH"

# Rsync all files at once (recursive & verbose)
rsync -av --progress -e "ssh $SSH_OPTIONS" --exclude=".git" --exclude="bin" --exclude="obj" \
    "${INCLUDE_FILES[@]}" "$VM_USER@$VM_HOST:$VM_DEST_PATH/"

# Set correct file permissions (verbose)
ssh $SSH_OPTIONS "$VM_USER@$VM_HOST" <<EOF
  echo "🔧 Setting file permissions..."
  find $VM_DEST_PATH -type f -exec chmod 644 {} +;
  find $VM_DEST_PATH -type d -exec chmod 755 {} +;
  echo "✅ File permissions updated!"
EOF

# Start Docker Compose services
echo "🚀 Starting Docker Compose services..."
ssh $SSH_OPTIONS "$VM_USER@$VM_HOST" <<EOF
  cd $VM_DEST_PATH
  sudo docker-compose -f docker-compose.vm.yml up -d
  echo "✅ Docker Compose services started!"
EOF

echo "✅ Deployment to VM Completed Successfully!"