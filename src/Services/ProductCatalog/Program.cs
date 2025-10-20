using MongoDB.Driver;
using ProductCatalog.Repositories;
using ProductCatalog.Services;
using EventBus.Abstractions;
using EventBus.RabbitMQ;

/*
 * Product Catalog Microservice - Entry Point
 * 
 * This service manages the product catalog for the CloudCart e-commerce platform.
 * It demonstrates:
 * - Microservices architecture with independent deployment
 * - MongoDB for flexible, document-based data storage
 * - RabbitMQ integration for event-driven communication
 * - Repository pattern for clean data access abstraction
 * - RESTful API design with Swagger documentation
 * - Health check endpoints for monitoring and orchestration
 * 
 * Architecture Pattern: Clean Architecture with separation of concerns
 * - Controllers: Handle HTTP requests/responses
 * - Services: Business logic layer
 * - Repositories: Data access layer
 * - Models: Domain entities
 */

var builder = WebApplication.CreateBuilder(args);

// ====================================================================================
// SERVICE CONFIGURATION - Dependency Injection Setup
// ====================================================================================

// Add ASP.NET Core MVC Controllers
// This enables the API controller endpoints for handling HTTP requests
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // OpenAPI/Swagger for API documentation and testing

// ====================================================================================
// MONGODB CONFIGURATION - NoSQL Database Setup
// ====================================================================================
// MongoDB is chosen for its flexible schema, horizontal scalability, and JSON-like documents
// which work well with product catalog data that may have varying attributes

var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
var mongoDatabaseName = builder.Configuration["MongoDB:DatabaseName"] ?? "ProductCatalogDb";

// Singleton: One MongoDB client instance shared across the application for connection pooling
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoConnectionString));

// Scoped: Each HTTP request gets its own database instance
builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDatabaseName);
});

// ====================================================================================
// RABBITMQ CONFIGURATION - Event Bus for Microservices Communication
// ====================================================================================
// RabbitMQ enables asynchronous, decoupled communication between microservices
// Example: When stock is updated, an event is published to notify other services
// This implements the Event-Driven Architecture pattern

var rabbitMQHost = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
var rabbitMQExchange = builder.Configuration["RabbitMQ:ExchangeName"] ?? "cloudcart_event_bus";

// Singleton: One event bus instance for publishing and subscribing to events
builder.Services.AddSingleton<IEventBus>(sp => new RabbitMQEventBus(rabbitMQHost, rabbitMQExchange, sp));

// ====================================================================================
// REPOSITORY & SERVICE REGISTRATION - Dependency Injection
// ====================================================================================
// Repository Pattern: Abstracts data access logic from business logic
// This makes the code more testable and maintainable

// Scoped lifetime: New instance per HTTP request, disposed after request completes
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

// ====================================================================================
// CORS CONFIGURATION - Cross-Origin Resource Sharing
// ====================================================================================
// Enables the API to be called from web applications hosted on different domains
// Essential for modern SPA (Single Page Application) frontends

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()      // Allow requests from any domain
              .AllowAnyMethod()       // Allow GET, POST, PUT, DELETE, etc.
              .AllowAnyHeader();      // Allow custom headers
    });
});

// ====================================================================================
// HEALTH CHECKS - Monitoring and Orchestration Support
// ====================================================================================
// Health checks allow container orchestrators (Kubernetes, Docker Swarm) and 
// load balancers to determine if the service is ready to receive traffic
builder.Services.AddHealthChecks();

// ====================================================================================
// BUILD APPLICATION
// ====================================================================================
var app = builder.Build();

// ====================================================================================
// HTTP REQUEST PIPELINE CONFIGURATION - Middleware Order Matters!
// ====================================================================================

// Development-only features: Swagger UI for API testing and documentation
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();      // Generates OpenAPI specification
    app.UseSwaggerUI();    // Provides interactive API documentation UI
}

// Enable CORS before authentication/authorization
app.UseCors("AllowAll");

// Authentication & Authorization middleware (for future JWT implementation)
app.UseAuthorization();

// Map controller endpoints to handle incoming HTTP requests
app.MapControllers();

// Health check endpoint for monitoring service availability
// Used by Kubernetes liveness/readiness probes and load balancers
app.MapHealthChecks("/health");

// ====================================================================================
// START THE APPLICATION
// ====================================================================================
// This starts the Kestrel web server and begins listening for HTTP requests
app.Run();
