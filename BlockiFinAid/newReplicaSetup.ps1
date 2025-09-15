# Requires -Version 5.1

Write-Host "--- Starting MongoDB Replica Set and Mongo Express Setup ---" -ForegroundColor Green

# --- Step 1: Clean up existing containers and networks ---

$containerCheck = docker ps -a -f "name=mongo-rs" --format "{{.ID}}"
if ($containerCheck) {
    Write-Host "Removing existing 'mongo-rs' container..." -ForegroundColor Yellow
    docker rm -f mongo-rs
}

$meContainerCheck = docker ps -a -f "name=mongo-express" --format "{{.ID}}"
if ($meContainerCheck) {
    Write-Host "Removing existing 'mongo-express' container..." -ForegroundColor Yellow
    docker rm -f mongo-express
}

$networkCheck = docker network ls -f "name=mongo-network" --format "{{.ID}}"
if ($networkCheck) {
    Write-Host "Removing existing 'mongo-network' network..." -ForegroundColor Yellow
    docker network rm mongo-network
}

# --- Step 2: Create the Docker network and MongoDB container ---

Write-Host "Creating the Docker network 'mongo-network'..." -ForegroundColor Green
docker network create mongo-network

Write-Host "Creating and running the MongoDB container..." -ForegroundColor Green
docker run --name mongo-rs -d -p 27017:27017 --network mongo-network mongo:latest --replSet rs0

Write-Host "MongoDB container is starting up. Waiting 10 seconds before initiating the replica set..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# --- Step 3: Initiate the replica set and verify its status ---

Write-Host "Initiating the MongoDB replica set..." -ForegroundColor Green
Write-Host "Using 'localhost:27017' for the replica set host to ensure it's accessible from the host machine." -ForegroundColor Yellow
docker exec mongo-rs mongosh --eval "rs.initiate({ _id: 'rs0', members: [{ _id: 0, host: 'localhost:27017' }] })"

Write-Host "Replica set initiated. Waiting 5 seconds for status..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

Write-Host "Verifying the replica set status..." -ForegroundColor Green
docker exec mongo-rs mongosh --eval "rs.status()"

# --- Step 4: Create and run the Mongo Express container ---

Write-Host "Creating and running the Mongo Express container..." -ForegroundColor Green
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