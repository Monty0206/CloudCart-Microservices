# 🧪 CloudCart Microservices - Testing & Running Guide

## 🚀 Quick Start (3 Steps)

### Option 1: Docker Compose (Recommended - Everything Automated)

```powershell
# Step 1: Make sure Docker Desktop is running
docker --version

# Step 2: Start everything with one command
docker-compose up --build

# Step 3: Wait 2-3 minutes for all services to start, then test!
# API Gateway: http://localhost:5000
# Product API: http://localhost:5001/swagger
```

### Option 2: Using the Helper Script

```powershell
# Run the PowerShell script
.\start.ps1

# Select option 2: "Start all services with Docker Compose"
# Wait for services to start, then test!
```

### Option 3: Local Development (Manual)

```powershell
# Terminal 1: Start Infrastructure
docker run -d -p 27017:27017 --name mongodb mongo:latest
docker run -d -p 5672:5672 -p 15672:15672 --name rabbitmq rabbitmq:3-management

# Terminal 2: Product Catalog Service
cd src/Services/ProductCatalog
dotnet run

# Terminal 3: API Gateway
cd src/ApiGateway
dotnet run

# Services will be available:
# - Product API: http://localhost:5001
# - API Gateway: http://localhost:5000
```

---

## ✅ Step-by-Step Testing Guide

### **Step 1: Verify Everything is Running**

#### Check Docker Containers
```powershell
docker-compose ps

# You should see:
# ✅ cloudcart-api-gateway
# ✅ cloudcart-product-catalog
# ✅ cloudcart-shopping-cart
# ✅ cloudcart-order
# ✅ cloudcart-payment
# ✅ cloudcart-user
# ✅ cloudcart-mongodb
# ✅ cloudcart-rabbitmq
# ✅ cloudcart-redis
```

#### Check Service Health
Open your browser and visit:
- http://localhost:5001/health → Should return "Healthy"
- http://localhost:5002/health → Should return "Healthy"
- http://localhost:5003/health → Should return "Healthy"

---

### **Step 2: Test with Swagger UI (Easiest)**

#### Open Swagger for Product Catalog
1. Navigate to: **http://localhost:5001/swagger**
2. You'll see all available endpoints

#### Create a Product (POST)
1. Click on **POST /api/products**
2. Click "Try it out"
3. Paste this JSON:

```json
{
  "name": "Dell XPS 15 Laptop",
  "description": "High-performance laptop with Intel i7 processor",
  "price": 1299.99,
  "category": "Electronics",
  "imageUrl": "https://example.com/dell-xps-15.jpg",
  "stockQuantity": 50,
  "tags": ["laptop", "dell", "electronics"]
}
```

4. Click "Execute"
5. You should get **201 Created** response with the product data including an ID

#### Get All Products (GET)
1. Click on **GET /api/products**
2. Click "Try it out"
3. Set parameters:
   - page: 1
   - pageSize: 10
4. Click "Execute"
5. You should see the product you just created!

---

### **Step 3: Test via API Gateway**

The API Gateway routes requests to the correct service.

#### Direct to Service (Port 5001)
```
http://localhost:5001/api/products
```

#### Via API Gateway (Port 5000)
```
http://localhost:5000/products
```

**Both should work!** The gateway removes `/api` prefix.

---

### **Step 4: Test with HTTP Files (VS Code)**

If you have the REST Client extension installed:

1. Open `src/Services/ProductCatalog/ProductCatalog.http`
2. Click "Send Request" above any request
3. See the response in a new tab

#### Example Requests in the File:

**Create Product:**
```http
POST http://localhost:5001/api/products
Content-Type: application/json

{
  "name": "iPhone 15 Pro",
  "description": "Latest iPhone",
  "price": 999.99,
  "category": "Smartphones",
  "stockQuantity": 100,
  "tags": ["iphone", "smartphone"]
}
```

**Get All Products:**
```http
GET http://localhost:5001/api/products?page=1&pageSize=10
```

**Search Products:**
```http
GET http://localhost:5001/api/products/search?query=laptop
```

---

### **Step 5: Test with PowerShell/cURL**

#### Create a Product
```powershell
$product = @{
    name = "Wireless Mouse"
    description = "Ergonomic wireless mouse"
    price = 29.99
    category = "Accessories"
    imageUrl = "https://example.com/mouse.jpg"
    stockQuantity = 200
    tags = @("mouse", "wireless")
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5001/api/products" `
    -Method POST `
    -Body $product `
    -ContentType "application/json"
```

#### Get All Products
```powershell
Invoke-RestMethod -Uri "http://localhost:5001/api/products?page=1&pageSize=10" `
    -Method GET
```

#### Using cURL (if installed)
```bash
# Create Product
curl -X POST http://localhost:5001/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Keyboard",
    "description": "Mechanical keyboard",
    "price": 89.99,
    "category": "Accessories",
    "stockQuantity": 75,
    "tags": ["keyboard", "mechanical"]
  }'

# Get Products
curl http://localhost:5001/api/products?page=1&pageSize=10
```

---

### **Step 6: Test Event-Driven Communication**

#### Update Product Stock (Triggers Event)
1. First, create a product and note its ID
2. Update stock:

```powershell
# Via Swagger: PATCH /api/products/{id}/stock
# Or via PowerShell:
$productId = "YOUR_PRODUCT_ID_HERE"
Invoke-RestMethod -Uri "http://localhost:5001/api/products/$productId/stock" `
    -Method PATCH `
    -Body "-5" `
    -ContentType "application/json"
```

3. **Check RabbitMQ Management UI**
   - Open: http://localhost:15672
   - Login: guest / guest
   - Go to "Queues" tab
   - You should see a queue for `ProductStockUpdatedEvent`
   - The event was published! 📨

---

### **Step 7: Access Infrastructure Services**

#### MongoDB
```powershell
# Using MongoDB Compass (GUI)
# Connection String: mongodb://localhost:27017

# Using mongosh (CLI)
docker exec -it cloudcart-mongodb mongosh
> use ProductCatalogDb
> db.products.find().pretty()
```

#### RabbitMQ Management UI
- URL: http://localhost:15672
- Username: `guest`
- Password: `guest`
- Explore:
  - **Exchanges** → cloudcart_event_bus
  - **Queues** → Event queues
  - **Connections** → Active services

#### Redis (if using)
```powershell
docker exec -it cloudcart-redis redis-cli
> KEYS *
> GET some_key
```

---

## 🔍 Troubleshooting

### Services Won't Start

**Check Docker is running:**
```powershell
docker --version
docker-compose --version
```

**View logs:**
```powershell
# All services
docker-compose logs

# Specific service
docker-compose logs product-catalog
docker-compose logs mongodb
docker-compose logs rabbitmq

# Follow logs in real-time
docker-compose logs -f product-catalog
```

### Port Already in Use

**Kill processes using the port:**
```powershell
# Find process on port 5001
netstat -ano | findstr :5001

# Kill it (replace PID with actual process ID)
taskkill /PID <PID> /F
```

### MongoDB Connection Issues

**Test connection:**
```powershell
docker exec -it cloudcart-mongodb mongosh --eval "db.version()"
```

**Check if container is running:**
```powershell
docker ps | findstr mongodb
```

### RabbitMQ Not Accessible

**Check if running:**
```powershell
docker ps | findstr rabbitmq
```

**Wait for startup:**
RabbitMQ takes 30-60 seconds to fully start. Check logs:
```powershell
docker-compose logs rabbitmq
```

---

## 📊 Test Scenarios

### Scenario 1: Full Product Lifecycle

```powershell
# 1. Create Product
$newProduct = @{
    name = "Gaming Laptop"
    description = "High-end gaming laptop"
    price = 1999.99
    category = "Electronics"
    stockQuantity = 25
    tags = @("gaming", "laptop")
} | ConvertTo-Json

$created = Invoke-RestMethod -Uri "http://localhost:5001/api/products" `
    -Method POST -Body $newProduct -ContentType "application/json"

$productId = $created.data.id
Write-Host "Created product with ID: $productId"

# 2. Get Product by ID
$product = Invoke-RestMethod -Uri "http://localhost:5001/api/products/$productId"
Write-Host "Retrieved: $($product.data.name)"

# 3. Update Product
$update = @{
    price = 1799.99
    stockQuantity = 30
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5001/api/products/$productId" `
    -Method PUT -Body $update -ContentType "application/json"
Write-Host "Updated price and stock"

# 4. Search for Product
$results = Invoke-RestMethod -Uri "http://localhost:5001/api/products/search?query=gaming"
Write-Host "Found $($results.data.Count) products matching 'gaming'"

# 5. Update Stock (triggers event)
Invoke-RestMethod -Uri "http://localhost:5001/api/products/$productId/stock" `
    -Method PATCH -Body "-5" -ContentType "application/json"
Write-Host "Reduced stock by 5 units"

# 6. Get Updated Product
$updated = Invoke-RestMethod -Uri "http://localhost:5001/api/products/$productId"
Write-Host "New stock: $($updated.data.stockQuantity)"

# 7. Delete Product
Invoke-RestMethod -Uri "http://localhost:5001/api/products/$productId" -Method DELETE
Write-Host "Product deleted"
```

### Scenario 2: Pagination & Filtering

```powershell
# Create multiple products
1..15 | ForEach-Object {
    $product = @{
        name = "Product $_"
        description = "Description for product $_"
        price = (Get-Random -Minimum 10 -Maximum 100)
        category = if ($_ % 2) { "Electronics" } else { "Accessories" }
        stockQuantity = (Get-Random -Minimum 10 -Maximum 100)
        tags = @("tag$_")
    } | ConvertTo-Json
    
    Invoke-RestMethod -Uri "http://localhost:5001/api/products" `
        -Method POST -Body $product -ContentType "application/json"
}

# Get page 1 (10 items)
$page1 = Invoke-RestMethod -Uri "http://localhost:5001/api/products?page=1&pageSize=10"
Write-Host "Page 1: $($page1.data.items.Count) items, Total: $($page1.data.totalCount)"

# Get page 2
$page2 = Invoke-RestMethod -Uri "http://localhost:5001/api/products?page=2&pageSize=10"
Write-Host "Page 2: $($page2.data.items.Count) items"

# Filter by category
$electronics = Invoke-RestMethod -Uri "http://localhost:5001/api/products?category=Electronics&page=1&pageSize=10"
Write-Host "Electronics: $($electronics.data.totalCount) items"
```

### Scenario 3: Via API Gateway

```powershell
# All requests go through gateway (port 5000)
# Gateway routes to Product Service (port 5001)

# Create via Gateway
$product = @{
    name = "Gateway Test Product"
    price = 49.99
    category = "Test"
    stockQuantity = 10
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/products" `
    -Method POST -Body $product -ContentType "application/json"

# Get via Gateway
Invoke-RestMethod -Uri "http://localhost:5000/products?page=1&pageSize=10"

# Notice: Gateway uses /products (no /api prefix)
#         Direct uses /api/products
```

---

## 🎯 Performance Testing

### Load Test with PowerShell

```powershell
# Create 100 products quickly
$results = 1..100 | ForEach-Object -Parallel {
    $product = @{
        name = "Load Test Product $_"
        price = 99.99
        category = "Test"
        stockQuantity = 100
    } | ConvertTo-Json
    
    Invoke-RestMethod -Uri "http://localhost:5001/api/products" `
        -Method POST -Body $product -ContentType "application/json"
} -ThrottleLimit 10

Write-Host "Created $($results.Count) products"
```

---

## 📈 Monitoring While Testing

### Watch Docker Logs
```powershell
# Terminal 1: Product Catalog logs
docker-compose logs -f product-catalog

# Terminal 2: API Gateway logs
docker-compose logs -f api-gateway

# Terminal 3: MongoDB logs
docker-compose logs -f mongodb
```

### Watch Container Stats
```powershell
docker stats
```

### Monitor RabbitMQ
1. Open http://localhost:15672
2. Go to "Queues" tab
3. Click on a queue
4. Watch messages flow through

---

## 🛑 Stopping Everything

### Docker Compose
```powershell
# Stop all services
docker-compose down

# Stop and remove volumes (clean slate)
docker-compose down -v

# Stop but keep data
docker-compose stop
```

### Local Services
Press `Ctrl+C` in each terminal running `dotnet run`

---

## ✅ Testing Checklist

Use this to verify everything works:

- [ ] Docker containers all running (`docker-compose ps`)
- [ ] Health checks returning "Healthy"
- [ ] Swagger UI accessible (http://localhost:5001/swagger)
- [ ] Can create a product via Swagger
- [ ] Can retrieve products via GET
- [ ] Can search products
- [ ] Can update product
- [ ] Can delete product
- [ ] API Gateway routing works (port 5000)
- [ ] MongoDB contains data
- [ ] RabbitMQ shows events when stock updates
- [ ] No errors in logs (`docker-compose logs`)

---

## 🎓 What You're Testing

When you run these tests, you're demonstrating:

✅ **RESTful API** - CRUD operations  
✅ **Pagination** - Handling large datasets  
✅ **Search** - Full-text search capabilities  
✅ **API Gateway** - Request routing  
✅ **Event-Driven** - RabbitMQ message publishing  
✅ **Microservices** - Independent service operation  
✅ **Containerization** - Docker orchestration  
✅ **NoSQL Database** - MongoDB persistence  
✅ **Health Checks** - Service monitoring  
✅ **Error Handling** - Proper HTTP status codes  

---

## 🚀 Next Steps

1. **Try all test scenarios above**
2. **Watch RabbitMQ UI while updating stock**
3. **Check MongoDB to see persisted data**
4. **Test via API Gateway vs direct service**
5. **Load test with multiple concurrent requests**
6. **Check logs for any errors**

**Now you have a fully functional microservices system running! 🎉**
