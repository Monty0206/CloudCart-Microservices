# 🚀 CloudCart - After Restart Instructions

## ✅ What We've Done:
1. ✅ Installed Docker Desktop (570 MB)
2. ✅ Installed WSL 2 (Windows Subsystem for Linux)
3. ✅ All dependencies are ready!

## 🔄 NEXT STEPS (After Restart):

### 1. Restart Your Computer
**Important**: WSL 2 requires a restart to activate.

```powershell
# Restart now:
Restart-Computer
```

### 2. After Restart - Open PowerShell and Run:

```powershell
# Navigate to project
cd "C:\Users\Montell Boks\Desktop\CloudCartMicroservices"

# Verify Docker is running
docker --version
docker ps

# Start ALL CloudCart services!
docker compose up --build -d
```

### 3. Wait 2-3 Minutes, Then Test:

```powershell
# Check running containers
docker compose ps

# Test Product API
Invoke-RestMethod -Uri 'http://localhost:5001/health'

# Open Swagger UI in browser
Start-Process "http://localhost:5001/swagger"

# Open RabbitMQ Management UI
Start-Process "http://localhost:15672"  # Login: guest/guest
```

---

## 🎯 Full Command After Restart:

```powershell
cd "C:\Users\Montell Boks\Desktop\CloudCartMicroservices"
docker compose up --build -d
Start-Sleep -Seconds 120
Start-Process "http://localhost:5001/swagger"
Start-Process "http://localhost:15672"
docker compose ps
```

---

## 📊 What Will Start:

1. **MongoDB** (Port 27017) - Database
2. **RabbitMQ** (Ports 5672, 15672) - Message Queue
3. **Redis** (Port 6379) - Cache
4. **API Gateway** (Port 5000) - Entry point
5. **Product Catalog** (Port 5001) - Product service
6. **Shopping Cart** (Port 5002) - Cart service
7. **Order Service** (Port 5003) - Order processing
8. **Payment Service** (Port 5004) - Payments
9. **User Service** (Port 5005) - Authentication

---

## 🧪 Testing After Startup:

### Swagger UIs:
- http://localhost:5001/swagger - Product Catalog
- http://localhost:5002/swagger - Shopping Cart
- http://localhost:5003/swagger - Orders
- http://localhost:5004/swagger - Payments
- http://localhost:5005/swagger - Users

### Infrastructure:
- http://localhost:15672 - RabbitMQ (guest/guest)
- mongodb://localhost:27017 - MongoDB

### Quick API Test:
```powershell
# Create a product
$product = @{
    name = "MacBook Pro"
    description = "Apple laptop"
    price = 2499.99
    category = "Electronics"
    stockQuantity = 25
    tags = @("laptop", "apple")
} | ConvertTo-Json

Invoke-RestMethod -Uri 'http://localhost:5001/api/products' `
    -Method POST `
    -Body $product `
    -ContentType 'application/json'

# Get all products
Invoke-RestMethod -Uri 'http://localhost:5001/api/products?page=1&pageSize=10'
```

---

## 🔍 Troubleshooting:

### If Docker doesn't start after restart:
1. Open Docker Desktop manually from Start Menu
2. Wait for the whale icon to be steady (not animated)
3. Run: `docker ps` to verify

### If containers fail to start:
```powershell
# View logs
docker compose logs -f

# Restart a specific service
docker compose restart product-catalog

# Rebuild everything
docker compose down
docker compose up --build
```

---

## 📝 Useful Commands:

```powershell
# View logs
docker compose logs -f product-catalog

# Stop all services
docker compose down

# Stop and remove data
docker compose down -v

# Restart a service
docker compose restart product-catalog

# View container stats
docker stats

# Access MongoDB
docker exec -it cloudcart-mongodb mongosh

# Access RabbitMQ
Start-Process "http://localhost:15672"
```

---

## 🎉 You're Almost There!

After restart:
1. Docker Desktop will start automatically
2. Run `docker compose up --build -d`
3. Wait 2-3 minutes
4. Open http://localhost:5001/swagger
5. Start testing your microservices! 🚀

---

**See you after the restart! 🔄**
