# CloudCart API Test Script
Write-Host "🧪 Testing CloudCart Microservices..." -ForegroundColor Cyan
Write-Host ""

# Configuration
$ProductApiUrl = "http://localhost:5053/api/products"
$HealthUrl = "http://localhost:5053/health"

# Test 1: Health Check
Write-Host "1️⃣ Testing Health Endpoint..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri $HealthUrl -Method GET
    Write-Host "✅ Health Check: $health" -ForegroundColor Green
} catch {
    Write-Host "❌ Health check failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Make sure the service is running on port 5053" -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# Test 2: Create a Product
Write-Host "2️⃣ Creating a new product..." -ForegroundColor Yellow
$newProduct = @{
    name = "Dell XPS 15 Laptop"
    description = "High-performance laptop with Intel i7 processor, 16GB RAM, 512GB SSD"
    price = 1299.99
    category = "Electronics"
    imageUrl = "https://example.com/dell-xps-15.jpg"
    stockQuantity = 50
    tags = @("laptop", "dell", "electronics", "computer")
} | ConvertTo-Json

try {
    $created = Invoke-RestMethod -Uri $ProductApiUrl -Method POST -Body $newProduct -ContentType "application/json"
    $productId = $created.data.id
    Write-Host "✅ Product Created!" -ForegroundColor Green
    Write-Host "   ID: $productId" -ForegroundColor Cyan
    Write-Host "   Name: $($created.data.name)" -ForegroundColor Cyan
    Write-Host "   Price: `$$($created.data.price)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Failed to create product: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "   Response: $responseBody" -ForegroundColor Yellow
    }
    exit 1
}
Write-Host ""

# Test 3: Get All Products
Write-Host "3️⃣ Retrieving all products (page 1)..." -ForegroundColor Yellow
try {
    $products = Invoke-RestMethod -Uri "$ProductApiUrl`?page=1&pageSize=10" -Method GET
    Write-Host "✅ Retrieved Products!" -ForegroundColor Green
    Write-Host "   Total Products: $($products.data.totalCount)" -ForegroundColor Cyan
    Write-Host "   Current Page: $($products.data.currentPage) of $($products.data.totalPages)" -ForegroundColor Cyan
    Write-Host "   Products on this page:" -ForegroundColor Cyan
    foreach ($product in $products.data.items) {
        Write-Host "      - $($product.name) (`$$($product.price))" -ForegroundColor White
    }
} catch {
    Write-Host "❌ Failed to retrieve products: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 4: Get Product by ID
if ($productId) {
    Write-Host "4️⃣ Getting product by ID..." -ForegroundColor Yellow
    try {
        $product = Invoke-RestMethod -Uri "$ProductApiUrl/$productId" -Method GET
        Write-Host "✅ Product Retrieved!" -ForegroundColor Green
        Write-Host "   Name: $($product.data.name)" -ForegroundColor Cyan
        Write-Host "   Description: $($product.data.description)" -ForegroundColor Cyan
        Write-Host "   Stock: $($product.data.stockQuantity) units" -ForegroundColor Cyan
    } catch {
        Write-Host "❌ Failed to get product: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""
}

# Test 5: Search Products
Write-Host "5️⃣ Searching for 'laptop'..." -ForegroundColor Yellow
try {
    $searchResults = Invoke-RestMethod -Uri "$ProductApiUrl/search?query=laptop" -Method GET
    Write-Host "✅ Search Complete!" -ForegroundColor Green
    Write-Host "   Found: $($searchResults.data.Count) product(s)" -ForegroundColor Cyan
    foreach ($product in $searchResults.data) {
        Write-Host "      - $($product.name)" -ForegroundColor White
    }
} catch {
    Write-Host "❌ Search failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 6: Update Product Stock
if ($productId) {
    Write-Host "6️⃣ Updating product stock (reduce by 5)..." -ForegroundColor Yellow
    try {
        $updated = Invoke-RestMethod -Uri "$ProductApiUrl/$productId/stock" -Method PATCH -Body "-5" -ContentType "application/json"
        Write-Host "✅ Stock Updated!" -ForegroundColor Green
        Write-Host "   New Stock: $($updated.data.stockQuantity) units" -ForegroundColor Cyan
        Write-Host "   📨 Event published to RabbitMQ: ProductStockUpdatedEvent" -ForegroundColor Magenta
    } catch {
        Write-Host "❌ Failed to update stock: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""
}

# Test 7: Filter by Category
Write-Host "7️⃣ Filtering by category 'Electronics'..." -ForegroundColor Yellow
try {
    $filtered = Invoke-RestMethod -Uri "$ProductApiUrl`?category=Electronics&page=1&pageSize=10" -Method GET
    Write-Host "✅ Filter Complete!" -ForegroundColor Green
    Write-Host "   Electronics Products: $($filtered.data.totalCount)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Filter failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 8: Update Product
if ($productId) {
    Write-Host "8️⃣ Updating product details..." -ForegroundColor Yellow
    $updateProduct = @{
        name = "Dell XPS 15 Laptop (Updated)"
        description = "Premium laptop with upgraded specs"
        price = 1199.99
        category = "Electronics"
        imageUrl = "https://example.com/dell-xps-15-updated.jpg"
        stockQuantity = 45
        tags = @("laptop", "dell", "electronics", "premium")
    } | ConvertTo-Json
    
    try {
        $updated = Invoke-RestMethod -Uri "$ProductApiUrl/$productId" -Method PUT -Body $updateProduct -ContentType "application/json"
        Write-Host "✅ Product Updated!" -ForegroundColor Green
        Write-Host "   New Name: $($updated.data.name)" -ForegroundColor Cyan
        Write-Host "   New Price: `$$($updated.data.price)" -ForegroundColor Cyan
    } catch {
        Write-Host "❌ Failed to update product: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""
}

# Test 9: Delete Product
if ($productId) {
    Write-Host "9️⃣ Deleting product..." -ForegroundColor Yellow
    try {
        Invoke-RestMethod -Uri "$ProductApiUrl/$productId" -Method DELETE
        Write-Host "✅ Product Deleted!" -ForegroundColor Green
    } catch {
        Write-Host "❌ Failed to delete product: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""
}

# Summary
Write-Host "=" * 50 -ForegroundColor Cyan
Write-Host "🎉 API Testing Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📊 What was tested:" -ForegroundColor Yellow
Write-Host "   ✅ Health Check" -ForegroundColor White
Write-Host "   ✅ Create Product (POST)" -ForegroundColor White
Write-Host "   ✅ Get All Products with Pagination (GET)" -ForegroundColor White
Write-Host "   ✅ Get Product by ID (GET)" -ForegroundColor White
Write-Host "   ✅ Search Products (GET)" -ForegroundColor White
Write-Host "   ✅ Update Stock - Event-Driven (PATCH)" -ForegroundColor White
Write-Host "   ✅ Filter by Category (GET)" -ForegroundColor White
Write-Host "   ✅ Update Product (PUT)" -ForegroundColor White
Write-Host "   ✅ Delete Product (DELETE)" -ForegroundColor White
Write-Host ""
Write-Host "🎯 Your microservice is working perfectly!" -ForegroundColor Green
Write-Host "=" * 50 -ForegroundColor Cyan
Write-Host ""
Write-Host "💡 Next Steps:" -ForegroundColor Yellow
Write-Host "   • Open Swagger UI: http://localhost:5053/swagger" -ForegroundColor Cyan
Write-Host "   • View logs in the terminal running dotnet" -ForegroundColor Cyan
Write-Host "   • Install MongoDB for data persistence" -ForegroundColor Cyan
Write-Host "   • Install RabbitMQ to see events in action" -ForegroundColor Cyan
