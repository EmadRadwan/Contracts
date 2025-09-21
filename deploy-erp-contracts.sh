#!/bin/bash

set -e  # Stop on first error

# Define variables
REPO_URL="https://github.com/EmadRadwan/Contracts.git"
DEST_PATH="/home/ubuntu/erp-contracts"
MYSQL_CONFIG_DIR="/home/ubuntu/mysql-config/contracts"
MYSQL_CONFIG_FILE="$MYSQL_CONFIG_DIR/my.cnf"
COMPOSE_FILE="docker-compose.vm.yml"

echo "🚀 Starting ERP Contracts Deployment on Host..."

# Stop and remove existing Docker Compose services to release file locks
echo "🛑 Stopping and removing Docker Compose services..."
if [ -f "$DEST_PATH/$COMPOSE_FILE" ]; then
    # REFACTOR: Check for file existence before running docker-compose down to avoid errors if file is missing
    cd "$DEST_PATH"
    sudo docker-compose -f "$COMPOSE_FILE" down
fi

# Delete the existing project folder
echo "🧹 Deleting existing project folder..."
sudo rm -rf "$DEST_PATH"

# Clone the repository from GitHub
echo "📥 Cloning repository from $REPO_URL..."
git clone "$REPO_URL" "$DEST_PATH"

# Navigate to the project directory
# REFACTOR: Added error handling to ensure directory exists before changing to it
if [ -d "$DEST_PATH" ]; then
    cd "$DEST_PATH"
else
    echo "❌ Error: Failed to clone repository or directory $DEST_PATH not found."
    exit 1
fi

# Set file permissions
echo "🔧 Setting file permissions..."
sudo find "$DEST_PATH" -type f -exec chmod 644 {} +
sudo find "$DEST_PATH" -type d -exec chmod 755 {} +

# Create MySQL configuration
echo "🛠️ Creating MySQL configuration..."
sudo mkdir -p "$MYSQL_CONFIG_DIR"
# REFACTOR: Use a here-document to create my.cnf for cleaner and more reliable file creation
sudo bash -c "cat > $MYSQL_CONFIG_FILE" <<EOF
[mysqld]
bind-address=0.0.0.0
EOF
sudo chmod 644 "$MYSQL_CONFIG_FILE"

# Build and start Docker Compose services
echo "🚀 Building and starting Docker Compose services..."
# REFACTOR: Ensure docker-compose up includes --build to rebuild images from fresh source
sudo docker-compose -f "$COMPOSE_FILE" up -d --build

echo "✅ Deployment Completed Successfully!"

# Verify services are running
echo "🔍 Verifying running services..."
sudo docker-compose -f "$COMPOSE_FILE" ps