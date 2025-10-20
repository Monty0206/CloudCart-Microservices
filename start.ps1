# CloudCart Microservices - Quick Start Script
# This script helps you start the infrastructure and services

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "  CloudCart Microservices - Quick Start" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is running
$dockerRunning = docker info 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker is not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}

Write-Host "✓ Docker is running" -ForegroundColor Green
Write-Host ""

# Prompt user for start option
Write-Host "Select an option:" -ForegroundColor Yellow
Write-Host "1. Start infrastructure only (MongoDB, RabbitMQ, Redis)"
Write-Host "2. Start all services with Docker Compose"
Write-Host "3. Build solution only"
Write-Host "4. Run Product Catalog service locally"
Write-Host "5. Clean all Docker containers and volumes"
Write-Host ""

$choice = Read-Host "Enter your choice (1-5)"

switch ($choice) {
    "1" {
        Write-Host "`nStarting infrastructure services..." -ForegroundColor Cyan
        
        # Start MongoDB
        Write-Host "Starting MongoDB..." -ForegroundColor Yellow
        docker run -d -p 27017:27017 --name cloudcart-mongodb `
            -e MONGO_INITDB_ROOT_USERNAME=admin `
            -e MONGO_INITDB_ROOT_PASSWORD=password123 `
            mongo:latest
        
        # Start RabbitMQ
        Write-Host "Starting RabbitMQ..." -ForegroundColor Yellow
        docker run -d -p 5672:5672 -p 15672:15672 --name cloudcart-rabbitmq `
            -e RABBITMQ_DEFAULT_USER=guest `
            -e RABBITMQ_DEFAULT_PASS=guest `
            rabbitmq:3-management
        
        # Start Redis
        Write-Host "Starting Redis..." -ForegroundColor Yellow
        docker run -d -p 6379:6379 --name cloudcart-redis redis:alpine
        
        Write-Host "`n✓ Infrastructure started successfully!" -ForegroundColor Green
        Write-Host "`nAccess points:" -ForegroundColor Cyan
        Write-Host "  - MongoDB: mongodb://localhost:27017"
        Write-Host "  - RabbitMQ Management: http://localhost:15672 (guest/guest)"
        Write-Host "  - Redis: localhost:6379"
    }
    
    "2" {
        Write-Host "`nStarting all services with Docker Compose..." -ForegroundColor Cyan
        docker-compose up --build -d
        
        Write-Host "`n✓ All services started successfully!" -ForegroundColor Green
        Write-Host "`nAccess points:" -ForegroundColor Cyan
        Write-Host "  - API Gateway: http://localhost:5000"
        Write-Host "  - Product Catalog: http://localhost:5001/swagger"
        Write-Host "  - Shopping Cart: http://localhost:5002/swagger"
        Write-Host "  - Order Service: http://localhost:5003/swagger"
        Write-Host "  - Payment Service: http://localhost:5004/swagger"
        Write-Host "  - User Service: http://localhost:5005/swagger"
        Write-Host "  - RabbitMQ Management: http://localhost:15672 (guest/guest)"
        Write-Host "`nTo view logs: docker-compose logs -f" -ForegroundColor Yellow
        Write-Host "To stop: docker-compose down" -ForegroundColor Yellow
    }
    
    "3" {
        Write-Host "`nBuilding solution..." -ForegroundColor Cyan
        dotnet build CloudCartMicroservices.sln
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "`n✓ Build succeeded!" -ForegroundColor Green
        } else {
            Write-Host "`n✗ Build failed!" -ForegroundColor Red
        }
    }
    
    "4" {
        Write-Host "`nEnsure infrastructure is running first (option 1)..." -ForegroundColor Yellow
        Write-Host "Starting Product Catalog service..." -ForegroundColor Cyan
        
        Set-Location src/Services/ProductCatalog
        dotnet run
    }
    
    "5" {
        Write-Host "`nCleaning Docker containers and volumes..." -ForegroundColor Cyan
        docker-compose down -v
        
        Write-Host "Removing individual containers..." -ForegroundColor Yellow
        docker rm -f cloudcart-mongodb cloudcart-rabbitmq cloudcart-redis 2>$null
        
        Write-Host "`n✓ Cleanup completed!" -ForegroundColor Green
    }
    
    default {
        Write-Host "`nInvalid choice!" -ForegroundColor Red
    }
}

Write-Host ""
