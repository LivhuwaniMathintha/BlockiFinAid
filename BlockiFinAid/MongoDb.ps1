# Requires -Version 5.1

Write-Host "--- Starting MongoDB Replica Set and Mongo Express Setup ---" -ForegroundColor Green

# --- Step 1: Create the MongoDB container and network ---

# Check if the mongo-rs container already exists and remove it if it does
$containerCheck = docker ps -a -f "name=mongo-rs" --format "{{.ID}}"
if ($containerCheck) {
    Write-Host "Removing existing 'mongo-rs' container..."
    docker rm -f mongo-rs
}

# Check if the mongo-network already exists and remove it if it does
$networkCheck = docker network ls -f "name=mongo-network" --format "{{.ID}}"
if ($networkCheck) {
    Write-Host "Removing existing 'mongo-network' network..."
    docker network rm mongo-network
}

Write-Host "Creating and running the MongoDB container..."
docker run --name mongo-rs -d -p 27017:27017 mongo:latest --replSet rs0

Write-Host "Creating the Docker network 'mongo-network'..."
docker network create mongo-network

Write-Host "Connecting the 'mongo-rs' container to 'mongo-network'..."
docker network connect mongo-network mongo-rs

Write-Host "MongoDB container is starting up. Waiting 10 seconds before initiating the replica set..."
Start-Sleep -Seconds 10

# --- Step 2 & 3: Initiate the replica set and verify its status ---
Write-Host "Initiating the MongoDB replica set..."
# The `mongosh --eval` command runs the replica set initialization from outside the container
docker exec mongo-rs mongosh --eval "rs.initiate({ _id: 'rs0', members: [{ _id: 0, host: 'mongo-rs:27017' }] })"

Write-Host "Replica set initiated. Waiting 5 seconds for status..."
Start-Sleep -Seconds 5

Write-Host "Verifying the replica set status..."
docker exec mongo-rs mongosh --eval "rs.status()"

# --- Step 4: Create and run the Mongo Express container ---

# Check if the mongo-express container already exists and remove it
$meContainerCheck = docker ps -a -f "name=mongo-express" --format "{{.ID}}"
if ($meContainerCheck) {
    Write-Host "Removing existing 'mongo-express' container..."
    docker rm -f mongo-express
}

Write-Host "Creating and running the Mongo Express container..."
docker run `
  --name mongo-express `
  -d `
  -p 8081:8081 `
  --network mongo-network `
  -e ME_CONFIG_MONGODB_SERVER=mongo-rs `
  -e ME_CONFIG_MONGODB_ENABLE_ADMIN=true `
  mongo-express

Write-Host "--- Setup complete! ---" -ForegroundColor Green
Write-Host "You can now access Mongo Express at http://localhost:8081" -ForegroundColor Yellow
