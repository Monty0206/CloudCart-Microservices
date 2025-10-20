# CloudCart Microservices

A scalable microservices-based e-commerce platform built with ASP.NET Core, featuring distributed architecture with API Gateway pattern, message queues for async communication, and distributed transactions using saga pattern.

## 🏗️ Architecture Overview

> 📚 **For detailed architecture explanations, design patterns, and learning resources, see [ARCHITECTURE.md](ARCHITECTURE.md)**

```
┌─────────────────────────────────────────────────────────────────┐
│                          API Gateway (Ocelot)                    │
│                         Port: 5000                               │
│         Single Entry Point - Routes All Client Requests          │
└────────────┬──────────┬──────────┬──────────┬─────────┬─────────┘
             │          │          │          │         │
    ┌────────▼──────┐ ┌▼────────┐ ┌▼───────┐ ┌▼──────┐ ┌▼──────┐
    │  Product      │ │Shopping │ │ Order  │ │Payment│ │ User  │
    │  Catalog      │ │  Cart   │ │Service │ │Service│ │Service│
    │  :5001        │ │ :5002   │ │ :5003  │ │ :5004 │ │ :5005 │
    └───────┬───────┘ └┬────────┘ └┬───────┘ └┬──────┘ └┬──────┘
            │          │           │          │         │
            └──────────┴───────────┴──────────┴─────────┘
                              │
                    ┌─────────┴──────────┐
                    │                    │
            ┌───────▼───────┐    ┌──────▼────────┐
            │   RabbitMQ    │    │   MongoDB     │
            │   :5672       │    │   :27017      │
            │   :15672      │    │               │
            │  Event Bus    │    │   NoSQL DB    │
            └───────────────┘    └───────────────┘

📖 Each service is independently deployable and scalable
📖 Services communicate asynchronously via RabbitMQ events
📖 MongoDB provides flexible schema and horizontal scaling
📖 API Gateway handles routing, rate limiting, and authentication
```

## 🚀 Key Features

- **Microservices Architecture**: Independent, scalable services
- **API Gateway**: Centralized routing with Ocelot
- **Event-Driven**: RabbitMQ for async inter-service communication
- **Saga Pattern**: Distributed transaction management
- **MongoDB**: NoSQL database for each microservice
- **Docker**: Containerized services
- **Kubernetes**: Production-ready orchestration
- **Health Checks**: Service monitoring and health endpoints
- **CORS**: Cross-Origin Resource Sharing enabled
- **Swagger/OpenAPI**: API documentation

## 📦 Services

### 1. **Product Catalog Service** (Port: 5001)
- Product management (CRUD operations)
- Category-based filtering
- Search functionality
- Stock management
- Product availability tracking

### 2. **Shopping Cart Service** (Port: 5002)
- Cart management per user
- Add/Remove items
- Quantity updates
- Total calculation

### 3. **Order Service** (Port: 5003)
- Order creation and processing
- Order history
- Order status tracking
- Integration with Payment service

### 4. **Payment Service** (Port: 5004)
- Payment processing
- Payment status tracking
- Integration with Order service
- Event publishing for payment status

### 5. **User Service** (Port: 5005)
- User registration and authentication
- JWT token generation
- User profile management
- Password hashing

### 6. **API Gateway** (Port: 5000)
- Request routing
- Load balancing
- Rate limiting
- Authentication middleware

## 🛠️ Technology Stack

| Technology | Purpose |
|------------|---------|
| **ASP.NET Core 9.0** | Web API framework |
| **Ocelot** | API Gateway |
| **MongoDB** | NoSQL database |
| **RabbitMQ** | Message broker |
| **Redis** | Caching (optional) |
| **Docker** | Containerization |
| **Kubernetes** | Orchestration |
| **Swagger** | API documentation |

## 📋 Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [MongoDB](https://www.mongodb.com/try/download/community) (or use Docker)
- [RabbitMQ](https://www.rabbitmq.com/download.html) (or use Docker)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

## 🚀 Getting Started

### Option 1: Run with Docker Compose (Recommended)

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/CloudCartMicroservices.git
   cd CloudCartMicroservices
   ```

2. **Build and run all services**
   ```bash
   docker-compose up --build
   ```

3. **Access the services**
   - API Gateway: http://localhost:5000
   - Product Catalog: http://localhost:5001/swagger
   - Shopping Cart: http://localhost:5002/swagger
   - Order Service: http://localhost:5003/swagger
   - Payment Service: http://localhost:5004/swagger
   - User Service: http://localhost:5005/swagger
   - RabbitMQ Management: http://localhost:15672 (guest/guest)

### Option 2: Run Locally

1. **Start Infrastructure Services**
   ```bash
   # Start MongoDB
   docker run -d -p 27017:27017 --name mongodb mongo:latest

   # Start RabbitMQ
   docker run -d -p 5672:5672 -p 15672:15672 --name rabbitmq rabbitmq:3-management

   # Start Redis (optional)
   docker run -d -p 6379:6379 --name redis redis:alpine
   ```

2. **Build the solution**
   ```bash
   dotnet build CloudCartMicroservices.sln
   ```

3. **Run each service** (in separate terminals)
   ```bash
   # Terminal 1: Product Catalog
   cd src/Services/ProductCatalog
   dotnet run

   # Terminal 2: Shopping Cart
   cd src/Services/ShoppingCart
   dotnet run

   # Terminal 3: Order Service
   cd src/Services/Order
   dotnet run

   # Terminal 4: Payment Service
   cd src/Services/Payment
   dotnet run

   # Terminal 5: User Service
   cd src/Services/User
   dotnet run

   # Terminal 6: API Gateway
   cd src/ApiGateway
   dotnet run
   ```

## 📚 API Documentation

Once the services are running, access Swagger UI for each service:

- **API Gateway**: http://localhost:5000 (uses Ocelot routing)
- **Product Catalog API**: http://localhost:5001/swagger
- **Shopping Cart API**: http://localhost:5002/swagger
- **Order API**: http://localhost:5003/swagger
- **Payment API**: http://localhost:5004/swagger
- **User API**: http://localhost:5005/swagger

## 🔧 Configuration

Each service has its own `appsettings.json` configuration file:

```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017"
  },
  "MongoDB": {
    "DatabaseName": "ServiceDb"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "ExchangeName": "cloudcart_event_bus"
  }
}
```

## 🧪 Testing

### Using Swagger UI
1. Navigate to any service's Swagger UI
2. Try out the endpoints directly

### Using HTTP Files
Each service includes a `.http` file for testing with REST Client extension:
- `src/Services/ProductCatalog/ProductCatalog.http`
- `src/Services/ShoppingCart/ShoppingCart.http`
- etc.

### Sample API Calls

**Create a Product:**
```bash
curl -X POST http://localhost:5000/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 999.99,
    "category": "Electronics",
    "imageUrl": "https://example.com/laptop.jpg",
    "stockQuantity": 50,
    "tags": ["electronics", "computer", "laptop"]
  }'
```

**Get All Products:**
```bash
curl http://localhost:5000/products?page=1&pageSize=10
```

## 🐳 Docker Commands

```bash
# Build all services
docker-compose build

# Start all services
docker-compose up

# Start in detached mode
docker-compose up -d

# Stop all services
docker-compose down

# View logs
docker-compose logs -f

# Remove volumes
docker-compose down -v
```

## ☸️ Kubernetes Deployment

(Kubernetes manifests would be in the `k8s/` directory)

```bash
# Apply all configurations
kubectl apply -f k8s/

# Check deployments
kubectl get deployments

# Check services
kubectl get services

# Check pods
kubectl get pods
```

## 📊 Event Flow Example

### Order Creation Flow (Saga Pattern)

1. User creates an order → **Order Service**
2. Order Service publishes `OrderCreatedEvent` → **RabbitMQ**
3. **Payment Service** receives event → Processes payment
4. Payment Service publishes `PaymentProcessedEvent` → **RabbitMQ**
5. **Order Service** receives event → Updates order status
6. **Product Catalog** receives event → Updates stock quantity

## 🔐 Authentication

The User Service provides JWT-based authentication:

1. **Register a user**
2. **Login** to receive JWT token
3. Include token in Authorization header: `Bearer {token}`
4. Protected endpoints validate the token

## 🏗️ Project Structure

```
CloudCartMicroservices/
├── src/
│   ├── ApiGateway/                 # Ocelot API Gateway
│   ├── Services/
│   │   ├── ProductCatalog/         # Product management
│   │   ├── ShoppingCart/           # Cart management
│   │   ├── Order/                  # Order processing
│   │   ├── Payment/                # Payment processing
│   │   └── User/                   # User & authentication
│   └── BuildingBlocks/
│       ├── Common/                 # Shared models, DTOs, repositories
│       └── EventBus/               # RabbitMQ event bus
├── k8s/                            # Kubernetes manifests
├── docker-compose.yml              # Docker composition
├── CloudCartMicroservices.sln      # Solution file
└── README.md
```

## 📈 Monitoring

- **RabbitMQ Management UI**: http://localhost:15672
- **Health Checks**: Each service exposes `/health` endpoint
- **Logs**: Check Docker logs or application logs

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License.

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Ocelot for the API Gateway implementation
- RabbitMQ for reliable message queuing
- MongoDB for flexible data storage

## 📞 Support

For issues and questions:
- Create an issue in the repository
- Check existing documentation
- Review Swagger API docs

## 🚧 Roadmap

- [ ] Add JWT authentication to all services
- [ ] Implement circuit breaker pattern (Polly)
- [ ] Add distributed tracing (OpenTelemetry)
- [ ] Implement caching with Redis
- [ ] Add comprehensive unit and integration tests
- [ ] Add API versioning
- [ ] Implement service discovery (Consul)
- [ ] Add Grafana dashboards for monitoring
- [ ] Implement CQRS with separate read models
- [ ] Add API documentation generation

## 📂 Documentation

- **[README.md](README.md)** - Getting started guide and API reference
- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Deep dive into architecture, patterns, and best practices
- **[PROJECT_SUMMARY.md](PROJECT_SUMMARY.md)** - Project status and checklist
- **[.github/copilot-instructions.md](.github/copilot-instructions.md)** - Development guidelines

## 💡 Key Highlights for Portfolio

This project demonstrates:

✅ **Microservices Architecture** - Independently deployable services with clear boundaries  
✅ **Event-Driven Design** - Asynchronous communication with RabbitMQ  
✅ **API Gateway Pattern** - Centralized routing with Ocelot  
✅ **Repository Pattern** - Clean data access abstraction  
✅ **SOLID Principles** - Clean, maintainable code  
✅ **Docker & Kubernetes** - Production-ready containerization  
✅ **NoSQL Database** - MongoDB for flexible schema  
✅ **RESTful API Design** - Best practices with Swagger documentation  
✅ **Health Checks** - Monitoring and orchestration support  
✅ **Comprehensive Documentation** - Well-commented code explaining architecture decisions  

---

**Built with ❤️ using ASP.NET Core Microservices Architecture**

*This project showcases enterprise-level software engineering practices suitable for scalable, production-ready applications.*
