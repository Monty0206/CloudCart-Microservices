using Ocelot.DependencyInjection;
using Ocelot.Middleware;

/*
 * API Gateway - Ocelot Implementation
 * 
 * The API Gateway is the single entry point for all client requests in our microservices architecture.
 * 
 * Why Use an API Gateway?
 * ========================
 * 1. Simplified Client Experience: Clients call one endpoint instead of multiple services
 * 2. Request Routing: Intelligently routes requests to appropriate microservices
 * 3. Load Balancing: Distributes traffic across service instances
 * 4. Security: Centralized authentication, authorization, and rate limiting
 * 5. Cross-Cutting Concerns: Logging, monitoring, caching in one place
 * 6. Protocol Translation: Can expose different protocols (REST, gRPC, WebSockets)
 * 
 * Ocelot Features Used:
 * ====================
 * - Request aggregation (combine multiple service calls)
 * - Rate limiting (prevent abuse)
 * - Caching (improve performance)
 * - Request/Response transformation
 * - Service discovery integration
 * - Quality of Service (circuit breaking, timeouts)
 * 
 * Architecture Pattern: API Gateway Pattern (Backend for Frontend - BFF)
 * 
 * Request Flow:
 * Client → API Gateway (Port 5000) → Routes to:
 *   - /products/* → Product Catalog Service (Port 5001)
 *   - /cart/* → Shopping Cart Service (Port 5002)
 *   - /orders/* → Order Service (Port 5003)
 *   - /payments/* → Payment Service (Port 5004)
 *   - /users/* → User Service (Port 5005)
 */

var builder = WebApplication.CreateBuilder(args);

// ====================================================================================
// OCELOT CONFIGURATION - Load routing and policies from ocelot.json
// ====================================================================================
// ocelot.json defines:
// - Routes: URL patterns and their downstream services
// - Rate limiting rules
// - Load balancing strategies
// - Authentication requirements
// - QoS (Quality of Service) settings
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Register Ocelot services in DI container
// This includes routing, load balancing, and all middleware components
builder.Services.AddOcelot();

// ====================================================================================
// CORS CONFIGURATION - Allow cross-origin requests
// ====================================================================================
// Essential for web frontends (React, Angular, Vue) hosted on different domains
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()      // Production: Restrict to specific origins
              .AllowAnyMethod()       // GET, POST, PUT, DELETE, etc.
              .AllowAnyHeader();      // Allow custom headers (Authorization, etc.)
    });
});

// ====================================================================================
// BUILD APPLICATION
// ====================================================================================
var app = builder.Build();

// Apply CORS before routing
app.UseCors("AllowAll");

// ====================================================================================
// OCELOT MIDDLEWARE - This does all the routing magic!
// ====================================================================================
// When a request comes in:
// 1. Ocelot matches the URL pattern from ocelot.json
// 2. Applies rate limiting if configured
// 3. Transforms the request if needed
// 4. Routes to the downstream service
// 5. Transforms the response
// 6. Returns to the client
await app.UseOcelot();

// ====================================================================================
// START THE GATEWAY
// ====================================================================================
// This gateway acts as a reverse proxy and intelligent router
// All client traffic flows through here, making it easy to:
// - Monitor all API calls
// - Apply security policies
// - Cache responses
// - Handle failures gracefully
app.Run();
